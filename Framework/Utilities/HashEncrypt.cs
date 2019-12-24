namespace PmSoft.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class HashEncrypt
    {
        private HashEncryptType _mbytHashType;
        private HashAlgorithm _mhash;
        private string _mstrHashString;
        private string _mstrOriginalString;
        private bool mboolUseSalt;
        private short msrtSaltLength;
        private string mstrSaltValue;

        public HashEncrypt()
        {
            this.mstrSaltValue = string.Empty;
            this.msrtSaltLength = 8;
            this._mbytHashType = HashEncryptType.MD5;
        }

        public HashEncrypt(HashEncryptType hashType)
        {
            this.mstrSaltValue = string.Empty;
            this.msrtSaltLength = 8;
            this._mbytHashType = hashType;
        }

        public HashEncrypt(HashEncryptType hashType, string originalString)
        {
            this.mstrSaltValue = string.Empty;
            this.msrtSaltLength = 8;
            this._mbytHashType = hashType;
            this._mstrOriginalString = originalString;
        }

        public HashEncrypt(HashEncryptType hashType, string originalString, bool useSalt, string saltValue)
        {
            this.mstrSaltValue = string.Empty;
            this.msrtSaltLength = 8;
            this._mbytHashType = hashType;
            this._mstrOriginalString = originalString;
            this.mboolUseSalt = useSalt;
            this.mstrSaltValue = saltValue;
        }

        public string CreateSalt()
        {
            byte[] data = new byte[8];
            new RNGCryptoServiceProvider().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public string Encrypt()
        {
            this.SetEncryptor();
            if (this.mboolUseSalt && (this.mstrSaltValue.Length == 0))
            {
                this.mstrSaltValue = this.CreateSalt();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(this.mstrSaltValue + this._mstrOriginalString);
            return Convert.ToBase64String(this._mhash.ComputeHash(bytes));
        }

        public string Encrypt(string originalString)
        {
            this._mstrOriginalString = originalString;
            return this.Encrypt();
        }

        public string Encrypt(string originalString, bool useSalt)
        {
            this._mstrOriginalString = originalString;
            this.mboolUseSalt = useSalt;
            return this.Encrypt();
        }

        public string Encrypt(string originalString, string saltValue)
        {
            this._mstrOriginalString = originalString;
            this.mstrSaltValue = saltValue;
            return this.Encrypt();
        }

        public string Encrypt(string originalString, HashEncryptType hashType)
        {
            this._mstrOriginalString = originalString;
            this._mbytHashType = hashType;
            return this.Encrypt();
        }

        public string Encrypt(string originalString, HashEncryptType hashType, string saltValue)
        {
            this._mstrOriginalString = originalString;
            this._mbytHashType = hashType;
            this.mstrSaltValue = saltValue;
            return this.Encrypt();
        }

        public void Reset()
        {
            this.mstrSaltValue = string.Empty;
            this._mstrOriginalString = string.Empty;
            this._mstrHashString = string.Empty;
            this.mboolUseSalt = false;
            this._mbytHashType = HashEncryptType.MD5;
            this._mhash = null;
        }

        private void SetEncryptor()
        {
            switch (this._mbytHashType)
            {
                case HashEncryptType.MD5:
                    this._mhash = new MD5CryptoServiceProvider();
                    return;

                case HashEncryptType.SHA1:
                    this._mhash = new SHA1CryptoServiceProvider();
                    return;

                case HashEncryptType.SHA256:
                    this._mhash = new SHA256Managed();
                    return;

                case HashEncryptType.SHA384:
                    this._mhash = new SHA384Managed();
                    return;

                case HashEncryptType.SHA512:
                    this._mhash = new SHA512Managed();
                    return;
            }
        }

        public string HashString
        {
            get
            {
                return this._mstrHashString;
            }
            set
            {
                this._mstrHashString = value;
            }
        }

        public HashEncryptType HashType
        {
            get
            {
                return this._mbytHashType;
            }
            set
            {
                if (this._mbytHashType != value)
                {
                    this._mbytHashType = value;
                    this._mstrOriginalString = string.Empty;
                    this._mstrHashString = string.Empty;
                    this.SetEncryptor();
                }
            }
        }

        public string OriginalString
        {
            get
            {
                return this._mstrOriginalString;
            }
            set
            {
                this._mstrOriginalString = value;
            }
        }

        public short SaltLength
        {
            get
            {
                return this.msrtSaltLength;
            }
            set
            {
                this.msrtSaltLength = value;
            }
        }

        public string SaltValue
        {
            get
            {
                return this.mstrSaltValue;
            }
            set
            {
                this.mstrSaltValue = value;
            }
        }

        public bool UseSalt
        {
            get
            {
                return this.mboolUseSalt;
            }
            set
            {
                this.mboolUseSalt = value;
            }
        }
    }
}

