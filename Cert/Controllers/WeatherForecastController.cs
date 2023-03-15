using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using TestWrapJS;

namespace Cert.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly INodeServices _nodeServices;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, INodeServices nodeServices)
        {
            _logger = logger;
            _nodeServices = nodeServices;
        }

        //[HttpGet]
        //public ActionResult Get()
        //{
        //    return  Content(Test.F1("123"));
        //}

        //[HttpGet]
        //public ActionResult Get()
        //{
        //    Test.F2();
        //    return Ok();
        //}

        //[HttpGet]
        //public ActionResult Get()
        //{
        //    Test.F3();
        //    return Ok();
        //}
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string res = null;
            try
            {
                //res = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/examples/test.js").Result;
                //res = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/examples/my_unpack.js").Result;
                //res = _nodeServices.InvokeAsync<object>("Scripts/jkurwa/examples/test2.js", "2 * 6").Result;
                //res = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/scripts/signed_data.js").Result;


                //using (var st = new StreamReader(@"D:\testCert\test.txt"))
                //{
                //    string fileContents = st.ReadToEnd();
                //    //var t = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/scripts/sign_data.js", fileContents).Result;
                //    res = _nodeServices.InvokeAsync<byte[]>("Scripts/jkurwa/scripts/sign_data.js", fileContents).Result;
                //    var tes = 3;
                //}

                var data = "{\"UID\":null,\"ShiftState\":1,\"ShiftId\":19684103,\"OpenShiftFiscalNum\":\"833882684\",\"ZRepPresent\":false,\"Testing\":true,\"Name\":\"Печатка №1 для РРО ФОП БОНДАРЕНКО ОЛЕКСАНДР АНДРІЙОВИЧ\",\"SubjectKeyId\":\"cdbc83967efae5552e6f6e078b57e2c9d526375f6f4bd723800bb31c79cdaa06\",\"FirstLocalNum\":396,\"NextLocalNum\":397,\"LastFiscalNum\":\"833882684\",\"OfflineSupported\":true,\"ChiefCashier\":true,\"OfflineSessionId\":\"4886665\",\"OfflineSeed\":\"656687435890700\",\"OfflineNextLocalNum\":\"1\",\"OfflineSessionDuration\":\"0\",\"OfflineSessionsMonthlyDuration\":\"0\",\"OfflineSessionRolledBack\":false,\"OfflineSessionRollbackCmdUID\":null,\"Closed\":false,\"TaxObject\":null}";

                res = _nodeServices.InvokeAsync<string>("Scripts/jkurwa/scripts/signed_data.js", data).Result;

                byte[] f = Convert.FromBase64String(res);

                var test = Encoding.UTF8.GetString(f);

                var httpClient = new HttpClient();

                string url = "http://fs.tax.gov.ua:8609/fs/cmd" + $"?randomseed={DateTime.Now.Ticks}";
                var binaryStreamContent = new MemoryStream(f);

                var streamContent = new StreamContent(binaryStreamContent);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                // Отправляем POST-запрос и получаем ответ
                var response = await httpClient.PostAsync(url, streamContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response received with length: " + responseContent.Length);
                    return Ok(response.IsSuccessStatusCode);
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error: " + response.StatusCode);
                    return BadRequest(response.IsSuccessStatusCode);
                }


                //using (var fileStream = new FileStream(@"D:\testCert\" + Guid.NewGuid() + ".txt", FileMode.Create, FileAccess.Write))
                //{
                //    fileStream.Write(res, 0, res.Length); // записываем массив байтов в файл
                //}

                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ReadCert")]
        public IActionResult ReadCert()
        {
            object res = null;
            try
            {
                res = _nodeServices.InvokeAsync<IDictionary<object, IDictionary<object, object>>>("Scripts/jkurwa/scripts/read_cert.js").Result;
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    return Ok(_nodeServices.InvokeAsync<string>("Scripts/jkurwa/examples/test2.js", "2 * 6").Result);
        //}

        //public async Task Test()
        //{
        //    byte[] res = null;
        //    try
        //    {
        //        var data = "{\"UID\":null,\"ShiftState\":1,\"ShiftId\":19684103,\"OpenShiftFiscalNum\":\"833882684\",\"ZRepPresent\":false,\"Testing\":true,\"Name\":\"Печатка №1 для РРО ФОП БОНДАРЕНКО ОЛЕКСАНДР АНДРІЙОВИЧ\",\"SubjectKeyId\":\"cdbc83967efae5552e6f6e078b57e2c9d526375f6f4bd723800bb31c79cdaa06\",\"FirstLocalNum\":396,\"NextLocalNum\":397,\"LastFiscalNum\":\"833882684\",\"OfflineSupported\":true,\"ChiefCashier\":true,\"OfflineSessionId\":\"4886665\",\"OfflineSeed\":\"656687435890700\",\"OfflineNextLocalNum\":\"1\",\"OfflineSessionDuration\":\"0\",\"OfflineSessionsMonthlyDuration\":\"0\",\"OfflineSessionRolledBack\":false,\"OfflineSessionRollbackCmdUID\":null,\"Closed\":false,\"TaxObject\":null}";

        //        res = _nodeServices.InvokeAsync<byte[]>("Scripts/jkurwa/scripts/sign_data.js", data).Result;

        //        var test = Encoding.UTF8.GetString(res);

        //        var httpClient = new HttpClient();

        //        string url = "http://fs.tax.gov.ua:8609/fs/cmd" + $"?randomseed={DateTime.Now.Ticks}";
        //        var binaryStreamContent = new MemoryStream(res);

        //        var streamContent = new StreamContent(binaryStreamContent);
        //        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        //        // Отправляем POST-запрос и получаем ответ
        //        var response = await httpClient.PostAsync(url, streamContent);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string responseContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine("Response received with length: " + responseContent.Length);
        //        }
        //        else
        //        {
        //            string responseContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine("Error: " + response.StatusCode);
        //        }


        //        using (var fileStream = new FileStream(@"D:\testCert\" + Guid.NewGuid() + ".txt", FileMode.Create, FileAccess.Write))
        //        {
        //            fileStream.Write(res, 0, res.Length); // записываем массив байтов в файл
        //        }

        //        Console.WriteLine("Ok");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Bad");
        //    }
        //}
    }
}
