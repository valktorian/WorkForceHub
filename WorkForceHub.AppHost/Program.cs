var builder = DistributedApplication.CreateBuilder(args);

var localEnvironment = LoadLocalEnvironment();
var localJwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? localEnvironment.GetValueOrDefault("JWT_SECRET_KEY")
    ?? Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
var localInternalApiKey = Environment.GetEnvironmentVariable("MEDIA_INTERNAL_API_KEY")
    ?? localEnvironment.GetValueOrDefault("MEDIA_INTERNAL_API_KEY")
    ?? Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
var postgresUser = GetLocalSetting("POSTGRES_USER", "admin");
var postgresPassword = GetLocalSetting("POSTGRES_PASSWORD", "admin");
var mongoUser = GetLocalSetting("MONGO_INITDB_ROOT_USERNAME", "root");
var mongoPassword = GetLocalSetting("MONGO_INITDB_ROOT_PASSWORD", "root");
var mongoConnectionString = $"mongodb://{Uri.EscapeDataString(mongoUser)}:{Uri.EscapeDataString(mongoPassword)}@localhost:27017/admin?authSource=admin";

AddService("gateway", @"..\WorkForceHub.Gateway\WorkForceHub.Gateway.csproj")
    .WithEnvironment("GatewayMode", "Local");

AddCommandService("account-command", @"..\AccountService\Command\Api\AccountService.Command.Api.csproj", "account_write");
AddQueryService("account-query", @"..\AccountService\Query\Api\AccountService.Query.Api.csproj");
AddCommandService("profile-command", @"..\ProfileService\Command\Api\ProfileService.Command.Api.csproj", "profile_write");
AddQueryService("profile-query", @"..\ProfileService\Query\Api\ProfileService.Query.Api.csproj");
AddCommandService("time-command", @"..\TimeService\Command\Api\TimeService.Command.Api.csproj", "time_write");
AddQueryService("time-query", @"..\TimeService\Query\Api\TimeService.Query.Api.csproj");
AddCommandService("evolution-command", @"..\EvolutionService\Command\Api\EvolutionService.Command.Api.csproj", "evolution_write");
AddQueryService("evolution-query", @"..\EvolutionService\Query\Api\EvolutionService.Query.Api.csproj");
AddService("media", @"..\MediaService\Api\MediaService.Api.csproj");

builder.Build().Run();

IResourceBuilder<ProjectResource> AddCommandService(string name, string projectPath, string databaseName)
{
    var connectionString = $"Host=localhost;Port=55433;Database={databaseName};Username={postgresUser};Password={postgresPassword}";

    return AddService(name, projectPath)
        .WithEnvironment("ConnectionStrings__DefaultConnection", connectionString)
        .WithEnvironment("Kafka__BootstrapServers", "localhost:29092");
}

IResourceBuilder<ProjectResource> AddQueryService(string name, string projectPath)
{
    return AddService(name, projectPath)
        .WithEnvironment("ConnectionStrings__ReadDatabase", mongoConnectionString)
        .WithEnvironment("Kafka__BootstrapServers", "localhost:29092");
}

IResourceBuilder<ProjectResource> AddService(string name, string projectPath)
{
    return builder.AddProject(name, projectPath, launchProfileName: "http")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
        .WithEnvironment("Jwt__SecretKey", localJwtSecret)
        .WithEnvironment("Storage__InternalApiKey", localInternalApiKey);
}

string GetLocalSetting(string name, string fallback)
{
    return Environment.GetEnvironmentVariable(name)
        ?? localEnvironment.GetValueOrDefault(name)
        ?? fallback;
}

static Dictionary<string, string> LoadLocalEnvironment()
{
    foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(startPath);

        for (var depth = 0; directory is not null && depth < 8; depth++, directory = directory.Parent)
        {
            var environmentPath = Path.Combine(directory.FullName, ".env");
            if (!File.Exists(environmentPath))
            {
                continue;
            }

            return File.ReadLines(environmentPath)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 && !line.StartsWith('#'))
                .Select(line => line.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim().Trim('"', '\''), StringComparer.OrdinalIgnoreCase);
        }
    }

    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
