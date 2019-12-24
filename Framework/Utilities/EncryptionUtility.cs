using System;
using System.Text;
using System.Security.Cryptography;

namespace PmSoft.Utilities
{
    public static class EncryptionUtility
    {
        public static string Base64_Decode(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            byte[] bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64_Encode(string str)
        {
            return Base64_Encode(str, Encoding.UTF8);
        }

        public static string Base64_Encode(string str, Encoding encoding)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return Convert.ToBase64String(encoding.GetBytes(str));
        }


        public static string MD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] byt, bytHash;
            byt = System.Text.Encoding.UTF8.GetBytes(str);
            bytHash = md5.ComputeHash(byt);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp;
        }

        public static string MD5_16(string str)
        {
            return MD5(str).Substring(8, 0x10);
        }

        public static string SymmetricDncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return new PmSoft.Utilities.SymmetricEncrypt(encryptType) { IVString = ivString, KeyString = keyString }.Decrypt(str);
        }

        public static string SymmetricEncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
        {
            if ((string.IsNullOrEmpty(str) || string.IsNullOrEmpty(ivString)) || string.IsNullOrEmpty(keyString))
            {
                return str;
            }
            return new PmSoft.Utilities.SymmetricEncrypt(encryptType) { IVString = ivString, KeyString = keyString }.Encrypt(str);
        }
    }
}
