using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureWorkshopApp.Helpers;
using AzureWorkshopApp.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AzureWorkshopApp.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly TelemetryClient _telemetryClient;

        public ImagesController(IStorageService storageService, TelemetryClient telemetryClient)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _telemetryClient = telemetryClient;
        }

        // POST /api/images/upload
        [Authorize(Roles = "Uploader")] 
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {

            var configValidation = _storageService.ValidateConfiguration();

            if (!configValidation.IsValid())
                return BadRequest(configValidation.GetErrors());

            if (files.Count == 0)
                return BadRequest("No files received from the upload");

            foreach (var formFile in files)
            {
                if (!FileFormatHelper.IsImage(formFile))
                {
                    return new UnsupportedMediaTypeResult();
                }
                if (formFile.Length <= 0)
                {
                    continue;
                }

                _telemetryClient.TrackEvent("UPLOADED_FILE", new Dictionary<string, string>
                {
                    { "FILE_NAME", formFile.FileName},
                    { "CONTENT_LENGTH", formFile.Length.ToString()}
                });

                using (var stream = formFile.OpenReadStream())
                {
                    if (await _storageService.UploadFileToStorage(stream, formFile.FileName))
                    {
                        return new AcceptedResult();
                    }
                }
            }

            return BadRequest("Look like the image couldnt upload to the storage");
        }

        // GET /api/images
        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var configValidation = _storageService.ValidateConfiguration();

            if (!configValidation.IsValid()) return BadRequest(configValidation.GetErrors());

            var imageUrls = await _storageService.GetImageUrls();

            return new ObjectResult(imageUrls);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetImageNames()
        {
            var configValidation = _storageService.ValidateConfiguration();

            if (!configValidation.IsValid()) return BadRequest(configValidation.GetErrors());

            var imageNames = await _storageService.GetImageNames();

            return new ObjectResult(JsonConvert.SerializeObject(imageNames));
        }

        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> GetImagesDirect(string name)
        {
            var configValidation = _storageService.ValidateConfiguration();

            if (!configValidation.IsValid()) return BadRequest(configValidation.GetErrors());

            var imageBlob = await _storageService.GetImage(name);
            await imageBlob.FetchAttributesAsync();
            var bytes = new byte[imageBlob.Properties.Length];
            await imageBlob.DownloadToByteArrayAsync(bytes, 0);
            return new ObjectResult(Convert.ToBase64String(bytes));
        }
    }
}