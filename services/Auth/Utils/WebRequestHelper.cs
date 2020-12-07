using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Koasta.Service.Auth.Utils
{
    public class WebRequestHelper
    {
        public async Task<T> GetAsync<T>(string addr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addr);

            using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            using Stream stream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(stream);

            return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false));
        }
    }
}
