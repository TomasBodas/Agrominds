using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PasswordEncrypter
    {
        public static string EncryptPassword(string pContraseña)
        {
            //Utilizamos la función de hash criptográfico proporcionada por la clase SHA256Managed
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                //La contraseña se convierte en una secuencia de bytes utilizando la codificación UTF-8
                byte[] bytes = Encoding.UTF8.GetBytes(pContraseña);
                //Se calcula el hash utilizando SHA256Managed.ComputeHash
                byte[] hash = sha256.ComputeHash(bytes);
                //El hash resultante se convierte en una cadena hexadecimal utilizando StringBuilder
                StringBuilder sb = new StringBuilder();
                {
                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                }
                //Se devuelve como resultado
                return sb.ToString();
            }
        }

        public static string EncryptData(string password)
        {
            string keys = "8hFfpJj/z9m5tpfr5O/xkA1eMzxQLF9Y3e4dUFL9eSk=";
            string ivs = "7Q67SD9gIQY4i/vsJgguIw==";

            byte[] key = Convert.FromBase64String(keys);
            byte[] iv = Convert.FromBase64String(ivs);

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes long after Base64 decoding", nameof(key));

            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes long after Base64 decoding", nameof(iv));

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(password);
                        }
                    }

                    byte[] encryptedBytes = ms.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string DecryptData(string encryptedPassword)
        {
            string keys = "8hFfpJj/z9m5tpfr5O/xkA1eMzxQLF9Y3e4dUFL9eSk=";
            string ivs = "7Q67SD9gIQY4i/vsJgguIw==";

            byte[] key = Convert.FromBase64String(keys);
            byte[] iv = Convert.FromBase64String(ivs);

            if (string.IsNullOrEmpty(encryptedPassword))
                throw new ArgumentNullException(nameof(encryptedPassword));

            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes long after Base64 decoding", nameof(key));

            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes long after Base64 decoding", nameof(iv));

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
