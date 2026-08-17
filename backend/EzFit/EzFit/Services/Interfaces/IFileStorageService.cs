using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IFileStorageService
    {
        string GenerateBaseName(int userId);
        Task SaveAsync(string baseName, List<Image> images);
    }
}
