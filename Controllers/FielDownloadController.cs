using System.Threading.Tasks;
using ForgeDerivative.Services;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace ForgeDerivative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FielDownloadController : Controller
    {
        private readonly IDownloadService downloadService;

         private Credentials Credentials { get; set; }
        public FielDownloadController(IDownloadService _downloadService)
        {
            downloadService = _downloadService;
        }

        [HttpPost]
        [Route("download")]
        public async Task Download([FromBody]dynamic obj)
        {
            string urn = obj.urn;
            Credentials = await Credentials.FromSessionAsync();
            await downloadService.Download(urn,Credentials.TokenInternal);
        }
    }
}