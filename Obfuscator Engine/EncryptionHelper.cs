using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace String_Encryption
{
    class EncryptionHelper
    {
        public static string Decrypt(string clearText)
        {
            string Kek = clearText.Split('.')[0].Replace("-ProtectedByTrinity-", "").Replace("Æ", "");
            return Encoding.UTF8.GetString(Convert.FromBase64String(Kek));
        }
    }
}
