using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class AlejoPronoun
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string subject { get; set; }
        [DataMember]
        [JsonProperty("object")]
        public string obj { get; set; }
        [DataMember]
        public bool singular { get; set; }
    }

    [DataContract]
    public class AlejoUserPronoun
    {
        [DataMember]
        public string channel_id { get; set; }
        [DataMember]
        public string channel_login { get; set; }
        [DataMember]
        public string pronoun_id { get; set; }
        [DataMember]
        public string alt_pronoun_id { get; set; }
    }

    public class AlejoPronounsService
    {
        public const string BaseAddress = "https://api.pronouns.alejo.io/v1/";

        private Dictionary<string, AlejoPronoun> pronounIDLookup = new Dictionary<string, AlejoPronoun>();

        public AlejoPronounsService() { }

        public async Task Initialize()
        {
            this.pronounIDLookup = await this.GetPronounIDs();
        }

        public async Task<AlejoUserPronoun> GetPronounData(string twitchLogin)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(AlejoPronounsService.BaseAddress))
                {
                    return await client.GetAsync<AlejoUserPronoun>($"users/{twitchLogin}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public string GetPronoun(string pronounID, string altPronounID)
        {
            try
            {
                if (string.IsNullOrEmpty(pronounID) || !this.pronounIDLookup.TryGetValue(pronounID, out AlejoPronoun pronoun))
                {
                    return string.Empty;
                }

                if (pronoun.singular)
                {
                    return pronoun.subject;
                }

                string first = pronoun.subject;
                string second = pronoun.obj;
                if (!string.IsNullOrWhiteSpace(altPronounID) && this.pronounIDLookup.TryGetValue(altPronounID, out AlejoPronoun altPronoun))
                {
                    second = altPronoun.subject;
                }

                return $"{first}/{second}";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return string.Empty;
            }
        }

        private async Task<Dictionary<string, AlejoPronoun>> GetPronounIDs()
        {
            Dictionary<string, AlejoPronoun> results = new Dictionary<string, AlejoPronoun>();
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(AlejoPronounsService.BaseAddress))
                {
                    string result = await client.GetStringAsync("pronouns");
                    if (!string.IsNullOrEmpty(result))
                    {
                        foreach (var kvp in JObject.Parse(result))
                        {
                            results[kvp.Key] = kvp.Value.ToObject<AlejoPronoun>();
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
