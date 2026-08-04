using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace EzFit.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageTestController : ControllerBase
{
    private const int TargetWidth = 1000;
    private const double MaxRatio = 2.5;
    private const int OverlapPx = 80;

    // POST api/imagetest  (form-data: image=<файл>)
    [HttpPost]
    public async Task<IActionResult> Test(IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Няма качено изображение.");

        using var inputStream = image.OpenReadStream();
        using var img = await Image.LoadAsync(inputStream);

        var originalWidth = img.Width;
        var originalHeight = img.Height;

        // Resize до целевата ширина, пропорционална височина
        img.Mutate(x => x.Resize(TargetWidth, 0));
        var resizedHeight = img.Height;
        var ratio = (double)resizedHeight / TargetWidth;

        var tiles = new List<Image>();

        if (ratio <= MaxRatio)
        {
            tiles.Add(img.Clone(x => { }));
        }
        else
        {
            var tileHeight = (int)(TargetWidth * MaxRatio); // макс. височина на едно парче
            var step = tileHeight - OverlapPx;
            var y = 0;

            while (y < resizedHeight)
            {
                var cropHeight = Math.Min(tileHeight, resizedHeight - y);
                var tile = img.Clone(x => x.Crop(new Rectangle(0, y, TargetWidth, cropHeight)));
                tiles.Add(tile);

                if (y + cropHeight >= resizedHeight) break;
                y += step;
            }
        }

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                var entry = archive.CreateEntry($"tile_{i + 1}.jpg");
                using var entryStream = entry.Open();
                await tiles[i].SaveAsync(entryStream, new JpegEncoder { Quality = 85 });
                tiles[i].Dispose();
            }
        }

        Response.Headers.Append("X-Original-Dimensions", $"{originalWidth}x{originalHeight}");
        Response.Headers.Append("X-Resized-Dimensions", $"{TargetWidth}x{resizedHeight}");
        Response.Headers.Append("X-Ratio", ratio.ToString("F2"));
        Response.Headers.Append("X-Tile-Count", tiles.Count.ToString());

        return File(zipStream.ToArray(), "application/zip", "tiles.zip");
    }
}