using System.Threading.Tasks;

namespace Koasta.Shared.Crypto
{
    public interface IKeyStoreHelper
    {
        Task<string> GetKeyParameterValue(string key);
    }
}
