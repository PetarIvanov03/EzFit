using EzFit.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _uploadsRoot;

        public FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _uploadsRoot = _configuration["ImageStorage:UploadsRoot"] ?? "App_Data/uploads";
        }

        public string GenerateBaseName(int userId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 4);
            return $"{userId}_{timestamp}_{suffix}";
        }

        public async Task SaveAsync(string baseName, List<Image> images)
        {
            Directory.CreateDirectory(_uploadsRoot);

            var extension = GetExtension();
            var encoder = GetEncoder();

            for (int i = 0; i < images.Count; i++)
            {
                var fileName = images.Count == 1
                    ? $"{baseName}.{extension}"
                    : $"{baseName}_tile{i + 1}.{extension}";

                var path = Path.Combine(_uploadsRoot, fileName);

                await using var stream = File.Create(path);
                await images[i].SaveAsync(stream, encoder);

                images[i].Dispose();
            }
        }

        private ImageEncoder GetEncoder()
        {
            var format = _configuration["ImageStorage:Format"] ?? "Webp";
            var quality = int.Parse(_configuration["ImageStorage:Quality"] ?? "80");

            return format switch
            {
                "Webp" => new WebpEncoder { Quality = quality },
                "Jpeg" => new JpegEncoder { Quality = quality },
                "Png" => new PngEncoder(),
                _ => new WebpEncoder { Quality = quality }
            };
        }

        private string GetExtension()
        {
            var format = _configuration["ImageStorage:Format"] ?? "Webp";
            return format switch
            {
                "Webp" => "webp",
                "Jpeg" => "jpg",
                "Png" => "png",
                _ => "webp"
            };
        }
    }
}