using System.Threading.Tasks;

namespace Koasta.Shared.Crypto
{
    public interface ICryptoHelper
    {
        string Generate(string password, int iterations = 1000);

        bool IsValid(string testPassword, string origDelimHash);

        string GenerateToken();

        Task<string> EncryptString(string plainInput);

        Task<string> DecryptString(string cipherText);
    }
}
