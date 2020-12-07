using Koasta.Service.Admin.Models;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public interface IFeedService
    {
         Task<Feed> GetFeed();
    }
}
