using EzFit.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class ImageService : IImageService
    {
        private const int TargetWidth = 1000;
        private const double MaxRatio = 2.5;
        private const int OverlapPx = 80;
        private const int LandscapeMaxSide = 1600;

        public async Task<List<Image>> ProcessAsync(Stream imageStream)
        {
            using var img = await Image.LoadAsync(imageStream);

            var originalWidth = img.Width;
            var originalHeight = img.Height;
            var isPortrait = originalHeight > originalWidth;

            var tiles = new List<Image>();

            if (!isPortrait)
            {
                var longerSide = Math.Max(originalWidth, originalHeight);
                if (longerSide > LandscapeMaxSide)
                {
                    if (originalWidth >= originalHeight)
                        img.Mutate(x => x.Resize(LandscapeMaxSide, 0));
                    else
                        img.Mutate(x => x.Resize(0, LandscapeMaxSide));
                }

                tiles.Add(img.Clone(x => { }));
                return tiles;
            }

            img.Mutate(x => x.Resize(TargetWidth, 0));
            var resizedHeight = img.Height;
            var ratio = (double)resizedHeight / TargetWidth;

            if (ratio <= MaxRatio)
            {
                tiles.Add(img.Clone(x => { }));
                return tiles;
            }

            var tileHeight = (int)(TargetWidth * MaxRatio);
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

            return tiles;
        }
    }
}