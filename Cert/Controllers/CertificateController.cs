using Cert.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cert.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly CertificateService _certificateService = null;
        private readonly KeyService _keyService = null;
        public CertificateController(CertificateService certificateService, KeyService keyService)
        {
            _certificateService = certificateService;
            _keyService = keyService;
        }

        [HttpPost("DecodeCertificate")]
        public ActionResult DecodeCertificate(IFormFile file)
        {
            long length = file.Length;
            if (length < 0)
                return BadRequest();

            byte[] bytes = new byte[length];

            using (var fileStream = file.OpenReadStream())
            {
                fileStream.Read(bytes, 0, (int)file.Length);
            }

            var cert = _certificateService.Read(bytes);
            return Ok();
        }

        [HttpPost("ReadKey")]
        public ActionResult ReadKey(IFormFile file, string password)
        {
            byte[] encryptedData;
            using (var stream = file.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    encryptedData = memoryStream.ToArray();
                }
            }

            //var key = _keyService.Read(encryptedData, password);
            _keyService.Read();

            return Ok();
        }
    }
}
