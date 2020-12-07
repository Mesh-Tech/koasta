using Koasta.Shared.Configuration;
using Koasta.Shared.Exceptions;
using Koasta.Shared.Types;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Koasta.Shared.Crypto
{
    public class CryptoHelper : ICryptoHelper
    {
        private const int TokenLength = 41;

        private readonly IKeyStoreHelper _keyStoreHelper;
        private readonly ISettings _settings;
        private readonly Constants _constants;

        public CryptoHelper(IKeyStoreHelper keyStoreHelper, Constants constants, ISettings settings)
        {
            _keyStoreHelper = keyStoreHelper;
            _constants = constants;
            _settings = settings;
        }

        public string Generate(string toHash, int iterations = 1000)
        {
            if (string.IsNullOrWhiteSpace(toHash))
            {
                throw new ArgumentNullException(nameof(toHash));
            }

            //generate a random salt for hashing
            var salt = new byte[24];
            using var provider = new RNGCryptoServiceProvider();
            provider.GetBytes(salt);

            //hash password given salt and iterations (default to 1000)
            //iterations provide difficulty when cracking
            using var pbkdf2 = new Rfc2898DeriveBytes(toHash, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(24);

            //return delimited string with salt | #iterations | hash
            return Convert.ToBase64String(salt) + "|" + iterations + "|" +
                Convert.ToBase64String(hash);
        }

        public bool IsValid(string testPassword, string origDelimHash)
        {
            //extract original values from delimited hash text
            var origHashedParts = origDelimHash.Split('|');
            var origSalt = Convert.FromBase64String(origHashedParts[0]);
            var origIterations = int.Parse(origHashedParts[1], CultureInfo.InvariantCulture);
            var origHash = origHashedParts[2];

            //generate hash from test password and original salt and iterations
            using var pbkdf2 = new Rfc2898DeriveBytes(testPassword, origSalt, origIterations);
            byte[] testHash = pbkdf2.GetBytes(24);

            //if hash values match then return success
            var test = Convert.ToBase64String(testHash);
            if (test == origHash)
                return true;

            //no match return false
            return false;
        }

        public string GenerateToken()
        {
            using RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider();
            byte[] tokenBuffer = new byte[TokenLength];
            cryptRNG.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }

        public async Task<string> EncryptString(string plainInput)
        {
            if (!_settings.Meta.EnableAccessTokenEcryption)
            {
                return plainInput;
            }

            using var aes = new AesCryptoServiceProvider();

            var aesIvKey = await _keyStoreHelper.GetKeyParameterValue(_constants.AesIV256Key).ConfigureAwait(true);
            var aesKey = await _keyStoreHelper.GetKeyParameterValue(_constants.Aes256Key).ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(aesIvKey) || string.IsNullOrWhiteSpace(aesKey))
            {
                throw new InvalidKeyException("Unable to get AES Keys");
            }

            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(aesIvKey);
            aes.Key = Encoding.UTF8.GetBytes(aesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert string to byte array
            byte[] src = Encoding.Unicode.GetBytes(plainInput);

            // encryption
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings
                return Convert.ToBase64String(dest);
            }
        }

        public async Task<string> DecryptString(string cipher)
        {
            if (!_settings.Meta.EnableAccessTokenEcryption)
            {
                return cipher;
            }

            using var aes = new AesCryptoServiceProvider();
            
            var aesIvKey = await _keyStoreHelper.GetKeyParameterValue(_constants.AesIV256Key).ConfigureAwait(true);
            var aesKey = await _keyStoreHelper.GetKeyParameterValue(_constants.Aes256Key).ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(aesIvKey) || string.IsNullOrWhiteSpace(aesKey))
            {
                throw new InvalidKeyException("Unable to get AES Keys");
            }

            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(aesIvKey);
            aes.Key = Encoding.UTF8.GetBytes(aesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert Base64 strings to byte array
            byte[] src = System.Convert.FromBase64String(cipher);

            // decryption
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }
    }
}
