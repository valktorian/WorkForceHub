var builder = DistributedApplication.CreateBuilder(args);

var appEnvironment = GetRequiredSetting("AppHost:Environment");
var launchProfile = GetRequiredSetting("AppHost:LaunchProfile");
var postgresHost = GetRequiredSetting("LocalInfrastructure:Postgres:Host");
var postgresPort = GetRequiredSetting("LocalInfrastructure:Postgres:Port");
var mongoHost = GetRequiredSetting("LocalInfrastructure:Mongo:Host");
var mongoPort = GetRequiredSetting("LocalInfrastructure:Mongo:Port");
var mongoDatabase = GetRequiredSetting("LocalInfrastructure:Mongo:Database");
var mongoAuthSource = GetRequiredSetting("LocalInfrastructure:Mongo:AuthSource");
var kafkaBootstrapServers = GetRequiredSetting("LocalInfrastructure:Kafka:BootstrapServers");
var mediaBaseUrl = GetRequiredSetting("LocalInfrastructure:Media:BaseUrl");

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
var mongoConnectionString = $"mongodb://{Uri.EscapeDataString(mongoUser)}:{Uri.EscapeDataString(mongoPassword)}@{mongoHost}:{mongoPort}/{mongoDatabase}?authSource={Uri.EscapeDataString(mongoAuthSource)}";

AddService("gateway", GetProjectPath("Gateway"))
    .WithEnvironment("GatewayMode", "Local");

AddCommandService("account-command", GetProjectPath("AccountCommand"), GetRequiredSetting("Databases:Account"));
AddQueryService("account-query", GetProjectPath("AccountQuery"));
AddCommandService("profile-command", GetProjectPath("ProfileCommand"), GetRequiredSetting("Databases:Profile"))
    .WithEnvironment("MediaStorage__BaseUrl", mediaBaseUrl)
    .WithEnvironment("MediaStorage__InternalApiKey", localInternalApiKey);
AddQueryService("profile-query", GetProjectPath("ProfileQuery"));
AddCommandService("time-command", GetProjectPath("TimeCommand"), GetRequiredSetting("Databases:Time"));
AddQueryService("time-query", GetProjectPath("TimeQuery"));
AddCommandService("evolution-command", GetProjectPath("EvolutionCommand"), GetRequiredSetting("Databases:Evolution"));
AddQueryService("evolution-query", GetProjectPath("EvolutionQuery"));
AddService("media", GetProjectPath("Media"));

builder.Build().Run();

IResourceBuilder<ProjectResource> AddCommandService(string name, string projectPath, string databaseName)
{
    var connectionString = $"Host={postgresHost};Port={postgresPort};Database={databaseName};Username={postgresUser};Password={postgresPassword}";

    return AddService(name, projectPath)
        .WithEnvironment("ConnectionStrings__DefaultConnection", connectionString)
        .WithEnvironment("Kafka__BootstrapServers", kafkaBootstrapServers);
}

IResourceBuilder<ProjectResource> AddQueryService(string name, string projectPath)
{
    return AddService(name, projectPath)
        .WithEnvironment("ConnectionStrings__ReadDatabase", mongoConnectionString)
        .WithEnvironment("Kafka__BootstrapServers", kafkaBootstrapServers);
}

IResourceBuilder<ProjectResource> AddService(string name, string projectPath)
{
    return builder.AddProject(name, projectPath, launchProfileName: launchProfile)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", appEnvironment)
        .WithEnvironment("DOTNET_ENVIRONMENT", appEnvironment)
        .WithEnvironment("Jwt__SecretKey", localJwtSecret)
        .WithEnvironment("Storage__InternalApiKey", localInternalApiKey);
}

string GetProjectPath(string name) => GetRequiredSetting($"Projects:{name}");

string GetRequiredSetting(string key)
{
    return builder.Configuration[key]
        ?? throw new InvalidOperationException($"Required AppHost configuration '{key}' is missing.");
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
