using Microsoft.AspNetCore.Mvc;
using PinRism.Lib;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PinRism.Lib.Models.DTOs;

namespace DotOcrAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly GeminiOcrService _ocrService;
        private readonly ILogger<OcrController> _logger;

        public OcrController(GeminiOcrService ocrService, ILogger<OcrController> logger)
        {
            _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("extract-text")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExtractText([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded or file is empty.");
                return BadRequest("No file uploaded or file is empty.");
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning("Unsupported file type uploaded: {ContentType}", file.ContentType);
                return BadRequest("Unsupported file type. Please upload an image (e.g., JPEG, PNG).");
            }

            OcrResultDto resultDto = new OcrResultDto(); 

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();

                _logger.LogInformation("Received image file: {FileName}, Size: {Length} bytes, Type: {ContentType}",
                                       file.FileName, file.Length, file.ContentType);


                resultDto .Meta.FileName = file.FileName;
                resultDto.Meta.Length = imageData.Length;
                resultDto.Meta.MimeType = file.ContentType;

                string extractedText = await _ocrService.ExtractTextFromImageAsync(imageData, file.ContentType);

                if (string.IsNullOrEmpty(extractedText))
                {
                    _logger.LogInformation("Text extraction completed, but no text was found for file: {FileName}", file.FileName);

                    resultDto.Success = false;
                    resultDto.Error = "No text found in the image.";
                    return Ok(resultDto);
                }

                _logger.LogInformation("Text successfully extracted from file: {FileName}", file.FileName);

                resultDto.Success = true;
                resultDto.Data.RawText = extractedText;

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the image for text extraction.");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during text extraction.");
            }
        }
    }
}
