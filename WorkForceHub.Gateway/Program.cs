using AspNetCoreRateLimit;
using Infrastructure.Api.Authentication;
using Infrastructure.Api.Observability;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Polly;
using Polly.Extensions.Http;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

var ocelotFile = builder.Environment.IsEnvironment("Docker") ? "ocelot.docker.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

builder.Services.AddWorkForceHubTracing(builder.Configuration, "WorkForceHub.Gateway");

var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
var allowedOrigins = builder.Configuration.GetSection("Gateway:Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddHttpClient("SwaggerClient").AddPolicyHandler(retryPolicy);
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options => options.AddPolicy("GatewayCors", p =>
{
    p.AllowAnyMethod().AllowAnyHeader();

    if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
    {
        p.AllowAnyOrigin();
        return;
    }

    p.WithOrigins(allowedOrigins);
}));

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration).AddPolly();

var app = builder.Build();

var swaggerSources = builder.Configuration.GetSection("SwaggerSources").Get<Dictionary<string, string[]>>() ?? new Dictionary<string, string[]>();

app.UseHttpsRedirection();
app.UseCors("GatewayCors");
UseCorrelationId(app);
app.UseAuthentication();
app.UseAuthorization();
UseGatewayApiAuthenticationGate(app);
UseMediaApiKeyInjection(app, builder.Configuration);
app.UseIpRateLimiting();

app.MapGet("/health", () => Results.Ok(new
{
    Service = "WorkForceHub.Gateway",
    Status = "healthy",
}));

app.Map("/swagger", swaggerApp =>
{
    swaggerApp.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/gateway-docs/v1/openapi.json", "WorkForceHub Unified");
    });
});

app.MapGet("/gateway-docs/v1/openapi.json", async (IHttpClientFactory httpClientFactory, IMemoryCache cache, CancellationToken ct) =>
{
    return await cache.GetOrCreateAsync("unified_swagger", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        var client = httpClientFactory.CreateClient("SwaggerClient");
        var merged = new JsonObject
        {
            ["openapi"] = "3.0.1",
            ["info"] = new JsonObject { ["title"] = "WorkForceHub API", ["version"] = "v1" },
            ["paths"] = new JsonObject(),
            ["components"] = new JsonObject
            {
                ["schemas"] = new JsonObject(),
                ["securitySchemes"] = new JsonObject()
            },
            ["security"] = new JsonArray
            {
                new JsonObject
                {
                    ["Bearer"] = new JsonArray()
                }
            }
        };

        foreach (var (serviceName, urls) in swaggerSources)
        {
            var json = await TryFetchSwaggerDocumentAsync(client, urls, ct);
            if (json is null) continue;

            if (json["paths"] is JsonObject paths)
                foreach (var path in paths)
                    MergeSwaggerPath((JsonObject)merged["paths"]!, path.Key, path.Value);

            if (json["components"]?["schemas"] is JsonObject schemas)
                foreach (var schema in schemas)
                    merged["components"]!["schemas"]![schema.Key] = schema.Value?.DeepClone();

            if (json["components"]?["securitySchemes"] is JsonObject securitySchemes)
                foreach (var scheme in securitySchemes)
                    merged["components"]!["securitySchemes"]![scheme.Key] = scheme.Value?.DeepClone();
        }

        return merged;
    });
});

app.MapWhen(ctx =>
    !ctx.Request.Path.StartsWithSegments("/health")
    && !ctx.Request.Path.StartsWithSegments("/swagger")
    && !ctx.Request.Path.StartsWithSegments("/gateway-docs"),
    branch => branch.UseOcelot());

app.Run();

static async Task<JsonObject?> TryFetchSwaggerDocumentAsync(HttpClient client, IEnumerable<string> urls, CancellationToken ct)
{
    foreach (var url in urls)
    {
        try { var response = await client.GetAsync(url, ct); if (response.IsSuccessStatusCode) return JsonNode.Parse(await response.Content.ReadAsStringAsync(ct))?.AsObject(); } catch { }
    }
    return null;
}

static void MergeSwaggerPath(JsonObject targetPaths, string pathKey, JsonNode? sourcePath)
{
    if (sourcePath is not JsonObject sourceOperations)
    {
        return;
    }

    if (targetPaths[pathKey] is not JsonObject targetOperations)
    {
        targetPaths[pathKey] = sourceOperations.DeepClone();
        return;
    }

    foreach (var operation in sourceOperations)
    {
        targetOperations[operation.Key] = operation.Value?.DeepClone();
    }
}

static bool IsPublicGatewayRequest(HttpRequest request)
{
    var path = request.Path;

    return path.StartsWithSegments("/health")
        || path.StartsWithSegments("/swagger")
        || path.StartsWithSegments("/gateway-docs")
        || path.StartsWithSegments("/docs")
        || string.Equals(path.Value, "/api/auth/login", StringComparison.OrdinalIgnoreCase)
        || (HttpMethods.IsPost(request.Method)
            && string.Equals(path.Value, "/api/accounts", StringComparison.OrdinalIgnoreCase));
}

static bool IsProtectedApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api") && !IsPublicGatewayRequest(request);
}

static IApplicationBuilder UseCorrelationId(IApplicationBuilder app)
{
    return app.Use(async (context, next) =>
    {
        const string headerName = "X-Correlation-ID";
        var correlationId = context.Request.Headers[headerName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[headerName] = correlationId;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[headerName] = correlationId;
            return Task.CompletedTask;
        });

        await next();
    });
}

static IApplicationBuilder UseGatewayApiAuthenticationGate(IApplicationBuilder app)
{
    return app.Use(async (context, next) =>
    {
        if (IsProtectedApiRequest(context.Request) && context.User.Identity?.IsAuthenticated != true)
        {
            await context.ChallengeAsync();
            return;
        }

        await next();
    });
}

static IApplicationBuilder UseMediaApiKeyInjection(IApplicationBuilder app, IConfiguration configuration)
{
    return app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api/media"))
        {
            var apiKey = configuration["Storage:InternalApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
                context.Request.Headers["X-Internal-Api-Key"] = apiKey;
        }
        await next();
    });
}
