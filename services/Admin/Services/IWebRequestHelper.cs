using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public interface IWebRequestHelper
    {
        Task<T> GetAsync<T>(string addr);
    }
}
