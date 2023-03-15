using Cert.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cert.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly CertificateService _certificateService = null;
        private readonly KeyService _keyService = null;
        private readonly INodeServices _nodeServices;
        public CertificateController(CertificateService certificateService, KeyService keyService, INodeServices nodeServices)
        {
            _certificateService = certificateService;
            _keyService = keyService;
            _nodeServices = nodeServices;
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

        [HttpPost("Sign")]
        public async Task<IActionResult> Sign([FromBody] string dataInBase64)
        {
            try
            {
                var data = Encoding.UTF8.GetString(Convert.FromBase64String(dataInBase64));

                var res = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/scripts/signed_data.js", data).Result;

                return Ok(res);

                //byte[] f = Convert.FromBase64String(res);

                //var test = Encoding.UTF8.GetString(f);

                //var httpClient = new HttpClient();

                //string url = "http://fs.tax.gov.ua:8609/fs/cmd" + $"?randomseed={DateTime.Now.Ticks}";
                //var binaryStreamContent = new MemoryStream(f);

                //var streamContent = new StreamContent(binaryStreamContent);
                //streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                //// Отправляем POST-запрос и получаем ответ
                //var response = await httpClient.PostAsync(url, streamContent);

                //if (response.IsSuccessStatusCode)
                //{
                //    string responseContent = await response.Content.ReadAsStringAsync();
                //    Console.WriteLine("Response received with length: " + responseContent.Length);
                //    return Ok(response.IsSuccessStatusCode);
                //}
                //else
                //{
                //    string responseContent = await response.Content.ReadAsStringAsync();
                //    Console.WriteLine("Error: " + response.StatusCode);
                //    return BadRequest(response.IsSuccessStatusCode);
                //}
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
