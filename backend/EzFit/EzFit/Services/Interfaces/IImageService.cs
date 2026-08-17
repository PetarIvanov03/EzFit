using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IImageService
    {
        Task<List<Image>> ProcessAsync(Stream imageStream);
    }
}
