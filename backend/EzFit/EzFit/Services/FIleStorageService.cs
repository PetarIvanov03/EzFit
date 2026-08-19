using EzFit.Options;
using EzFit.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class FileStorageService : IFileStorageService
    {
        private static readonly Dictionary<string, (string Extension, Func<int, ImageEncoder> CreateEncoder)> EncodersByFormat = new()
        {
            ["Webp"] = ("webp", quality => new WebpEncoder { Quality = quality }),
            ["Jpeg"] = ("jpg", quality => new JpegEncoder { Quality = quality }),
            ["Png"] = ("png", _ => new PngEncoder()),
        };

        private readonly ImageStorageOptions _options;

        public FileStorageService(IOptions<ImageStorageOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateBaseName(int userId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 4);
            return $"{userId}_{timestamp}_{suffix}";
        }

        public async Task SaveAsync(string baseName, List<Image> images, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(_options.UploadsRoot);

            var (extension, createEncoder) = GetFormat();
            var encoder = createEncoder(_options.Quality);

            for (int i = 0; i < images.Count; i++)
            {
                var fileName = images.Count == 1
                    ? $"{baseName}.{extension}"
                    : $"{baseName}_tile{i + 1}.{extension}";

                var path = Path.Combine(_options.UploadsRoot, fileName);

                await using var stream = File.Create(path);
                await images[i].SaveAsync(stream, encoder, cancellationToken);

                images[i].Dispose();
            }
        }

        // baseName has no extension/tile suffix of its own (see SaveAsync), so a single
        // base name may have saved as either "{baseName}.{ext}" or several
        // "{baseName}_tile{n}.{ext}" files — a prefix match deletes whichever it was.
        public Task DeleteAsync(string baseName, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_options.UploadsRoot))
            {
                return Task.CompletedTask;
            }

            foreach (var path in Directory.EnumerateFiles(_options.UploadsRoot, $"{baseName}*"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Delete(path);
            }

            return Task.CompletedTask;
        }

        private (string Extension, Func<int, ImageEncoder> CreateEncoder) GetFormat()
        {
            return EncodersByFormat.TryGetValue(_options.Format, out var format)
                ? format
                : EncodersByFormat["Webp"];
        }
    }
}
