using System.Collections.Generic;

namespace MixItUp.Base.Services
{
    public class SecretsService
    {
        private Dictionary<string, string> secrets = new Dictionary<string, string>();

        public void AddSecret(string key, string secret)
        {
            this.secrets[key] = secret;
        }

        public string GetSecret(string key)
        {
            if (this.secrets.ContainsKey(key))
            {
                return this.secrets[key];
            }
            return null;
        }

        public virtual string Encrypt(string data) { return data; }

        public virtual string Decrypt(string data) { return data; }
    }
}
