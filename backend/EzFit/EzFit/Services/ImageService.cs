using EzFit.Exceptions;
using EzFit.Options;
using EzFit.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class ImageService : IImageService
    {
        private const int TargetWidth = 1000;
        private const double MaxRatio = 2.5;
        private const int OverlapPx = 80;
        private const int LandscapeMaxSide = 1600;

        private readonly UploadsOptions _uploadsOptions;

        public ImageService(IOptions<UploadsOptions> uploadsOptions)
        {
            _uploadsOptions = uploadsOptions.Value;
        }

        public async Task<List<Image>> ProcessAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            await ValidateDimensionsAsync(imageStream, cancellationToken);

            using var img = await LoadImageAsync(imageStream, cancellationToken);

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

        // Reads only the header via Image.IdentifyAsync — rejects oversized images
        // before Image.LoadAsync would decode the full pixel buffer into memory.
        private async Task ValidateDimensionsAsync(Stream imageStream, CancellationToken cancellationToken)
        {
            ImageInfo info;
            try
            {
                info = await Image.IdentifyAsync(imageStream, cancellationToken);
            }
            catch (Exception ex) when (ex is UnknownImageFormatException || ex is InvalidImageContentException)
            {
                throw new ImageValidationException("Unrecognized or invalid image format.", ex);
            }

            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            if (info.Width > _uploadsOptions.MaxDimension || info.Height > _uploadsOptions.MaxDimension)
            {
                throw new ImageValidationException(
                    $"Image dimensions exceed the maximum allowed ({_uploadsOptions.MaxDimension}px per side).");
            }

            var pixelCount = (long)info.Width * info.Height;
            if (pixelCount > _uploadsOptions.MaxPixels)
            {
                throw new ImageValidationException(
                    $"Image resolution exceeds the maximum allowed ({_uploadsOptions.MaxPixels} pixels).");
            }
        }

        // Image.LoadAsync can still throw on a file whose header parsed cleanly but whose
        // body is corrupt/truncated — map that to the same 400 path as a bad header.
        private static async Task<Image> LoadImageAsync(Stream imageStream, CancellationToken cancellationToken)
        {
            try
            {
                return await Image.LoadAsync(imageStream, cancellationToken);
            }
            catch (Exception ex) when (ex is UnknownImageFormatException || ex is InvalidImageContentException)
            {
                throw new ImageValidationException("Unrecognized or invalid image format.", ex);
            }
        }
    }
}
