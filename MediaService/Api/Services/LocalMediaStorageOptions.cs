namespace MediaService.Api.Services;

public class LocalMediaStorageOptions
{
    public string RootPath { get; set; } = "uploads";
    public string PublicBaseUrl { get; set; } = "http://localhost:5199";
    public string InternalApiKey { get; set; } = string.Empty;
    public int MaxFileSizeMb { get; set; } = 5;
    public int ImageQuality { get; set; } = 80;
}
