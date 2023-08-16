using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class AlejoPronounsService
    {
        public const string BaseAddress = "https://pronouns.alejo.io/api/";

        private Dictionary<string, string> pronounIDLookup = new Dictionary<string, string>();

        public AlejoPronounsService() { }

        public async Task Initialize()
        {
            this.pronounIDLookup = await this.GetPronounIDs();
        }

        public async Task<string> GetPronounID(string twitchLogin)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(AlejoPronounsService.BaseAddress))
                {
                    string result = await client.GetStringAsync($"users/{twitchLogin}");
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jobject = JArray.Parse(result).First as JObject;
                        if (jobject != null)
                        {
                            return jobject["pronoun_id"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public string GetPronoun(string pronounID)
        {
            if (!string.IsNullOrEmpty(pronounID) && this.pronounIDLookup.TryGetValue(pronounID, out string pronoun))
            {
                return pronoun;
            }
            return string.Empty;
        }

        private async Task<Dictionary<string, string>> GetPronounIDs()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(AlejoPronounsService.BaseAddress))
                {
                    string result = await client.GetStringAsync("pronouns");
                    if (!string.IsNullOrEmpty(result))
                    {
                        foreach (JObject jobject in JArray.Parse(result))
                        {
                            results[jobject["name"].ToString()] = jobject["display"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return results;
        }
    }
}
