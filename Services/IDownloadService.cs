using System.Threading.Tasks;

namespace ForgeDerivative.Services
{
    public interface IDownloadService
    {
        Task Download(string urn,string accessToken);
    }
}