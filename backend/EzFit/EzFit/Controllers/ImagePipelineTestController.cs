using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ImagePipelineTestController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IFileStorageService _fileStorageService;

    public ImagePipelineTestController(IImageService imageService, IFileStorageService fileStorageService)
    {
        _imageService = imageService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> Test(IFormFile image)
    {
        var tiles = await _imageService.ProcessAsync(image.OpenReadStream());
        var baseName = _fileStorageService.GenerateBaseName(1);
        await _fileStorageService.SaveAsync(baseName, tiles);

        return Ok(new { baseName, tileCount = tiles.Count });
    }
}