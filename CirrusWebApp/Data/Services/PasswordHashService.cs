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
        public readonly Encoding _encoding;
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

        public string GeneratePasswordHash(string PasswordText)
        {
            HashAlgorithm alg = new SHA512Managed();
            byte[] passwordBytes = _encoding.GetBytes(PasswordText);

            return _encoding.GetString(alg.ComputeHash(passwordBytes));
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

        /// <summary>
        /// Verifies the clear text <paramref name="PasswordText"/> with the Salt and <paramref name="HashToCheck"/>
        /// </summary>
        /// <param name="PasswordText"></param>
        /// <param name="Salt"></param>
        /// <param name="HashToCheck"></param>
        /// <returns></returns>
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

        public bool VerifyPassword(byte[] PasswordBytes, string Salt, string HashToCheck)
        {
            bool bSuccess = false;

            HashAlgorithm alg = new SHA512Managed();

            byte[] saltBytes = _encoding.GetBytes(Salt);

            byte[] saltedHashBytes = new byte[saltBytes.Length + PasswordBytes.Length];

            for (int i = 0; i < Salt.Length; i++)
            {
                saltedHashBytes[i] = saltBytes[i];
            }
            for (int j = 0; j < PasswordBytes.Length; j++)
            {
                saltedHashBytes[Salt.Length + j] = PasswordBytes[j];
            }

            if (_encoding.GetString(alg.ComputeHash(saltedHashBytes)) == HashToCheck)
            {
                bSuccess = true;
            }

            return bSuccess;
        }
    }
}
