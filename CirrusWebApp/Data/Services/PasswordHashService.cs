using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Services
{
    public class PasswordHashService
    {
        private static Encoding _encoding;
        private static int _saltSize;
        private static readonly RNGCryptoServiceProvider rNg = new RNGCryptoServiceProvider();
        public readonly string RandomSalt = null;

        public PasswordHashService()
        {
            _encoding = Encoding.ASCII;
            _saltSize = 32;
            RandomSalt = GenerateRandomSalt(_saltSize);
        }

        public PasswordHashService(Encoding Encoding)
        {
            _encoding = Encoding;
            _saltSize = 32;
            RandomSalt = GenerateRandomSalt(_saltSize);
        }

        public PasswordHashService(int SaltSize)
        {
            _encoding = Encoding.ASCII;
            _saltSize = SaltSize;
            RandomSalt = GenerateRandomSalt(_saltSize);
        }

        public PasswordHashService(Encoding Encoding, int SaltSize)
        {
            _encoding = Encoding;
            _saltSize = SaltSize;
            RandomSalt = GenerateRandomSalt(_saltSize);
        }

        private string GenerateRandomSalt(int saltSize)
        {
            byte[] saltBytes = new byte[saltSize];
            rNg.GetBytes(saltBytes);
            return _encoding.GetString(saltBytes);
        }

        public string GenerateSaltedHash(string PassWordText)
        {
            HashAlgorithm alg = new SHA512Managed();

            byte[] saltBytes = _encoding.GetBytes(RandomSalt);
            byte[] passwordBytes = _encoding.GetBytes(PassWordText);

            byte[] saltedHashBytes = new byte[saltBytes.Length + passwordBytes.Length];

            for (int i = 0; i < RandomSalt.Length; i++)
            {
                saltedHashBytes[i] = saltBytes[i];
            }
            for (int j = 0; j < PassWordText.Length; j++)
            {
                saltedHashBytes[RandomSalt.Length + j] = passwordBytes[j];
            }

            return _encoding.GetString(alg.ComputeHash(saltedHashBytes));
        }

        public bool VerifyPassword(string PasswordText, string Salt, string HashToCheck)
        {
            bool bSuccess = false;

            HashAlgorithm alg = new SHA512Managed();

            byte[] saltBytes = _encoding.GetBytes(Salt);
            byte[] passwordBytes = _encoding.GetBytes(PasswordText);

            byte[] saltedHashBytes = new byte[saltBytes.Length + passwordBytes.Length];

            for (int i = 0; i < Salt.Length; i++)
            {
                saltedHashBytes[i] = saltBytes[i];
            }
            for (int j = 0; j < PasswordText.Length; j++)
            {
                saltedHashBytes[Salt.Length + j] = passwordBytes[j];
            }

            if (_encoding.GetString(alg.ComputeHash(saltedHashBytes)) == HashToCheck)
            {
                bSuccess = true;
            }

            return bSuccess;
        }
    }
}
