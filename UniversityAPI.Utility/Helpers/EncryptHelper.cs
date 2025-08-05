using System.Text;
using Jose;
using Jose.keys;
using UniversityAPI.Utility;

namespace System.Security.Cryptography
{
    public sealed class EncryptHelper
    {
        private static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");
        private static readonly string fingerprint = ConfigHelper.GetDefaultValue<string>("fingerprint");
        private static readonly bool is_FIPS_enabled = ConvertHelper.ToBool(ConfigHelper.GetDefaultValue<bool>("is_FIPS_enabled"));

        public static string EncryptByMd5(string plainText)
        {
            plainText = plainText.Trim();

            if (is_FIPS_enabled)
            {
                return EncryptBySha256(plainText);
            }

            using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] hashedData = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(plainText));

                var builder = new StringBuilder();
                for (int i = 0; i < hashedData.Length; i++)
                {
                    builder.Append(hashedData[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string EncryptByAesCbc(string plainText, string secretKey)
        {
            string guidText = secretKey.ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return EncryptByAesCbc(plainText, key, iv);
        }

        public static string EncryptByAesCbc(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            var secretKey = ConfigHelper.GetPublicCipherKey<string>();
            string guidText = secretKey.ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return EncryptByAesCbc(plainText, key, iv);
        }

        public static string EncryptByAesCbc(string plainText, Guid guid)
        {
            string guidText = guid.ToString().ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return EncryptByAesCbc(plainText, key, iv);
        }

        public static string EncryptByAesCbc(string plainText, string key, string iv, int keySize = 128)
        {
            if (is_FIPS_enabled)
            {
                return EncryptByAesCbcViaCryptoServiceProvider(plainText, key, iv, keySize);
            }

            using (var rijndael = GetAesCbcRijndaelManaged(key, iv, keySize))
            using (var transform = rijndael.CreateEncryptor())
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherTextBytes = transform.TransformFinalBlock(plainTextBytes, 0, plainText.Length);

                return Convert.ToBase64String(cipherTextBytes);
            }
        }

        public static string EncryptByAesCbcViaCryptoServiceProvider(string plainText, string key, string iv, int keySize = 128)
        {
            using (var aesAlg = GetAesCryptoProvider(key, iv, keySize))
            using (var transform = aesAlg.CreateEncryptor())
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherTextBytes = transform.TransformFinalBlock(plainTextBytes, 0, plainText.Length);

                return Convert.ToBase64String(cipherTextBytes);
            }
        }

        public static string DecryptByAesCbcViaCryptoServiceProvider(string cipherText, string secretKey)
        {
            string guidText = secretKey.ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return DecryptByAesCbcViaCryptoServiceProvider(cipherText, key, iv);
        }

        public static string DecryptByAesCbcViaCryptoServiceProvider(string cipherText, string key, string iv, int keySize = 128)
        {
            using (var aesAlg = GetAesCryptoProvider(key, iv, keySize))
            using (var transform = aesAlg.CreateDecryptor())
            {
                var cipherTextBytes = Convert.FromBase64String(cipherText.Replace(" ", "+"));
                byte[] plainTextBytes = transform.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

                return Encoding.UTF8.GetString(plainTextBytes);
            }
        }

        public static string DecryptByAesCbc(string cipherText, string secretKey)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            if (is_FIPS_enabled)
            {
                return DecryptByAesCbcViaCryptoServiceProvider(cipherText, secretKey);
            }

            string guidText = secretKey.ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return DecryptByAesCbc(cipherText, key, iv);
        }

        public static string DecryptByAesCbc(string cipherText, Guid guid)
        {
            string guidText = guid.ToString().ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return DecryptByAesCbc(cipherText, key, iv);
        }

        public static string DecryptByAesCbc(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            var secretKey = ConfigHelper.GetPublicCipherKey<string>();
            string guidText = secretKey.ToLowerInvariant().Replace("-", "");
            string key = ConvertHelper.ToBase64String(guidText);
            string iv = ConvertHelper.ToBase64String(guidText.Substring(10, 16));

            return DecryptByAesCbc(cipherText, key, iv);
        }

        public static string DecryptByAesCbc(string cipherText, string key, string iv, int keySize = 128)
        {
            if (is_FIPS_enabled)
            {
                return DecryptByAesCbcViaCryptoServiceProvider(cipherText, key, iv, keySize);
            }

            using (var rijndael = GetAesCbcRijndaelManaged(key, iv, keySize))
            using (var transform = rijndael.CreateDecryptor())
            {
                var cipherTextBytes = Convert.FromBase64String(cipherText.Replace(" ", "+"));
                byte[] plainTextBytes = transform.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

                return Encoding.UTF8.GetString(plainTextBytes);
            }
        }

        public static Stream GenerateStreamFromString(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string EncryptBySha256(string plainText)
        {
            if (StringHelper.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            using (var sha256 = SHA256Managed.Create())
            {
                byte[] plainTextBytes = Encoding.Default.GetBytes(plainText);
                byte[] hashBytes = sha256.ComputeHash(plainTextBytes);

                var builder = new StringBuilder();
                for (int i = 0, j = hashBytes.Length; i < j; i++)
                {
                    builder.AppendFormat("{0:x2}", hashBytes[i]);
                }

                return builder.ToString();
            }
        }

        public static string EncryptBySha512(string plainText)
        {
            using (var sha512 = SHA512Managed.Create())
            {
                byte[] plainTextBytes = Encoding.Default.GetBytes(plainText);
                byte[] hashBytes = sha512.ComputeHash(plainTextBytes);

                var builder = new StringBuilder();
                for (int i = 0, j = hashBytes.Length; i < j; i++)
                {
                    builder.AppendFormat("{0:x2}", hashBytes[i]);
                }

                return builder.ToString();
            }
        }

        public static string EncryptByDes(string originalString)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException("The string which needs to be encrypted cannot be null.");
            }

            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            var writer = new StreamWriter(cryptoStream);

            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();

            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public static string DecryptByDes(string cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
            {
                throw new ArgumentNullException("The string which needs to be encrypted cannot be null.");
            }

            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString));
            var cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            var reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }

        public static string EncryptPassword(string plainText)
        {
            return EncryptBySha512(plainText);
        }

        public static string EncryptPassword(string plainText, Guid userId)
        {
            return EncryptBySha512($"{plainText}{userId.ToString().Substring(0, 32)}");
        }

        public static string NewPassword(string userName)
        {
            return string.Format("LMS{0}{1}", DateTimeHelper.StandardNow().Year, userName.ToLowerInvariant());
        }

        public static string EncryptFileName(string fileName, string salt = "yim")
        {
            var extension = EncryptByMd5($"{StringHelper.ToPureUpperNoSpace(fileName)}&{salt}");
            return $"{fileName}.{extension}";
        }

        private static RijndaelManaged GetAesCbcRijndaelManaged(string key, string iv, int keySize = 128)
        {
            var rijndael = new RijndaelManaged();

            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;
            rijndael.KeySize = keySize;
            rijndael.BlockSize = 128;

            var keyTextBytes = Encoding.UTF8.GetBytes(key);
            var keyBytes = new byte[16];
            var keyLength = keyTextBytes.Length > keyBytes.Length ? keyBytes.Length : keyTextBytes.Length;
            Array.Copy(keyTextBytes, keyBytes, keyLength);
            rijndael.Key = keyBytes;

            var ivTextBytes = Encoding.UTF8.GetBytes(iv);
            var ivBytes = new byte[16];
            var ivLength = ivTextBytes.Length > ivBytes.Length ? ivBytes.Length : ivTextBytes.Length;
            Array.Copy(ivTextBytes, ivBytes, ivLength);
            rijndael.IV = ivBytes;

            return rijndael;
        }

        private static AesCryptoServiceProvider GetAesCryptoProvider(string key, string iv, int keySize = 128)
        {
            AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider();

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.KeySize = keySize;
            aesAlg.BlockSize = 128;

            var keyTextBytes = Encoding.UTF8.GetBytes(key);
            var keyBytes = new byte[16];
            var keyLength = keyTextBytes.Length > keyBytes.Length ? keyBytes.Length : keyTextBytes.Length;
            Array.Copy(keyTextBytes, keyBytes, keyLength);
            aesAlg.Key = keyBytes;

            var ivTextBytes = Encoding.UTF8.GetBytes(iv);
            var ivBytes = new byte[16];
            var ivLength = ivTextBytes.Length > ivBytes.Length ? ivBytes.Length : ivTextBytes.Length;
            Array.Copy(ivTextBytes, ivBytes, ivLength);
            aesAlg.IV = ivBytes;

            return aesAlg;
        }

        public static string EncryptIdentity(string identityField)
        {
            return EncryptField(identityField, fingerprint, "fingerprint");
        }

        public static string DecryptIdentity(string identityField)
        {
            return DecryptField(identityField, fingerprint, "fingerprint");
        }

        private static string EncryptField(string field, string keyValue, string keyName)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }
            return EncryptByAesCbc(field, keyValue, StringHelper.Reverse(keyValue));
        }

        public static string DecryptField(string field, string keyValue, string keyName)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }
            return DecryptByAesCbc(field, keyValue, StringHelper.Reverse(keyValue));
        }

        public static string EncryptByAesCbcSalt(string input, string password)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = EncryptByAesCbcSalt(bytesToBeEncrypted, passwordBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public static byte[] EncryptByAesCbcSalt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 2, 4, 6, 8, 1, 3, 5, 7 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static string DecryptByAesCbcSalt(string input, string password)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = DecryptByAesCbcSalt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }

        public static byte[] DecryptByAesCbcSalt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = new byte[] { 2, 4, 6, 8, 1, 3, 5, 7 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        #region X5092Certificate Encryption

        public static string EncryptByEllipticCurve(string plainText, object key, JwsAlgorithm algorithm)
        {
            return JWT.Encode(plainText, key, algorithm);
        }

        public static string EncryptByEllipticCurve(string plainText, ECParameters param, JwsAlgorithm algorithm, CngKeyUsages usage = CngKeyUsages.None)
        {
            return JWT.Encode(plainText, EccKey.New(param.Q.X, param.Q.Y, param.D, usage: usage), algorithm);
        }

        public static string DecryptByEllipticCurve(string plainText, ECParameters param, CngKeyUsages usage = CngKeyUsages.None)
        {
            return JWT.Decode(plainText, EccKey.New(param.Q.X, param.Q.Y, param.D, usage: usage));
        }

        public static string EncryptByRSA(string plainText, object key, JwsAlgorithm algorithm)
        {
            return JWT.Encode(plainText, key, algorithm);
        }

        public static string EncryptByRSA(string plainText, RSACryptoServiceProvider provider, JwsAlgorithm algorithm)
        {
            return JWT.Encode(plainText, provider, algorithm);
        }

        public static string DecryptByRSA(string plainText, object key)
        {
            return JWT.Decode(plainText, key);
        }

        public static string DecryptByRSA(string plainText, RSACryptoServiceProvider provider)
        {
            return JWT.Decode(plainText, provider);
        }

        #endregion X5092Certificate Encryption
    }
}