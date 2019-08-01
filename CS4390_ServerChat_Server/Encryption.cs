using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CS4390_ServerChat_Server {
    public static class Encryption {
        public static byte[] Encrypt(string messageSent, string Cipher) {
            using (var CryptoMD5 = new MD5CryptoServiceProvider()) {
                using (var TripleDES = new TripleDESCryptoServiceProvider()) {
                    TripleDES.Key = CryptoMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(Cipher));
                    TripleDES.Mode = CipherMode.ECB;
                    TripleDES.Padding = PaddingMode.PKCS7;

                    using (var crypt = TripleDES.CreateEncryptor()) {
                        byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageSent);
                        return crypt.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                    }
                }
            }
        }

        public static string Decrypt(byte[] encryptedMessage, string Cipher) {
            using (var CryptoMD5 = new MD5CryptoServiceProvider()) {
                using (var TripleDES = new TripleDESCryptoServiceProvider()) {
                    TripleDES.Key = CryptoMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(Cipher));
                    TripleDES.Mode = CipherMode.ECB;
                    TripleDES.Padding = PaddingMode.PKCS7;

                    using (var crypt = TripleDES.CreateDecryptor()) {
                        byte[] totalBytes = crypt.TransformFinalBlock(encryptedMessage, 0, encryptedMessage.Length);
                        return UTF8Encoding.UTF8.GetString(totalBytes);
                    }
                }
            }
        }
    }
}
