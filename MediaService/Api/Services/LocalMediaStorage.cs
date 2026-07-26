using Infrastructure.Api.Common;
using Infrastructure.Api.Storage;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace MediaService.Api.Services;

public class LocalMediaStorage : ILocalMediaStorage
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    private readonly LocalMediaStorageOptions _options;
    private readonly string _rootPath;

    public LocalMediaStorage(IWebHostEnvironment environment, IOptions<LocalMediaStorageOptions> options)
    {
        _options = options.Value;
        _rootPath = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);

        Directory.CreateDirectory(_rootPath);
    }

    public string RootPath => _rootPath;

    public async Task<ExternalFileUploadResult> SaveImageAsync(
        string category,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw ApiException.BadRequest("Only JPG, JPEG, and PNG images are supported.");
        }

        var maxBytes = _options.MaxFileSizeMb * 1024L * 1024L;
        if (file.Length > maxBytes)
        {
            throw ApiException.BadRequest($"Image exceeds the {_options.MaxFileSizeMb} MB limit.");
        }

        const string extension = ".jpg";
        var imageQuality = Math.Clamp(_options.ImageQuality, 1, 100);

        var normalizedCategory = string.IsNullOrWhiteSpace(category) ? "misc" : category.Trim().ToLowerInvariant();
        var relativeDirectory = Path.Combine(normalizedCategory, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
        var absoluteDirectory = Path.Combine(_rootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var generatedFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteDirectory, generatedFileName);

        await using var stream = file.OpenReadStream();
        using var codec = SKCodec.Create(stream);

        if (codec is null
            || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png))
        {
            throw ApiException.BadRequest("The uploaded file is not a valid JPG, JPEG, or PNG image.");
        }

        using var sourceBitmap = SKBitmap.Decode(codec);
        if (sourceBitmap is null)
        {
            throw ApiException.BadRequest("The uploaded image could not be decoded.");
        }

        var outputInfo = new SKImageInfo(
            sourceBitmap.Width,
            sourceBitmap.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Opaque);

        using var outputBitmap = new SKBitmap(outputInfo);
        using (var canvas = new SKCanvas(outputBitmap))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(
                sourceBitmap,
                0,
                0,
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
            canvas.Flush();
        }

        using var outputImage = SKImage.FromBitmap(outputBitmap);
        using var compressedImage = outputImage.Encode(SKEncodedImageFormat.Jpeg, imageQuality);
        if (compressedImage is null)
        {
            throw ApiException.Internal("The uploaded image could not be compressed.");
        }

        await using var target = File.Create(absolutePath);
        compressedImage.SaveTo(target);
        await target.FlushAsync(cancellationToken);

        var storageKey = string.Join('/', relativeDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Append(generatedFileName));
        var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/media/{storageKey}";

        return new ExternalFileUploadResult(
            file.FileName,
            "image/jpeg",
            new FileInfo(absolutePath).Length,
            storageKey,
            url,
            DateTime.UtcNow);
    }
}
