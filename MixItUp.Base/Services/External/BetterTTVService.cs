using MixItUp.Base.ViewModel.Chat;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class BetterTTVEmoteModel : IChatEmoteViewModel
    {
        public string id { get; set; }
        public string channel { get; set; }
        public string code { get; set; }
        public string imageType { get; set; }

        public string ID { get { return this.id; } }
        public string Name { get { return this.code; } }
        public string ImageURL { get { return string.Format("https://cdn.betterttv.net/emote/{0}/1x", this.id); } }

        public bool IsGIF { get { return string.Equals(this.imageType, "gif", StringComparison.OrdinalIgnoreCase); } }
    }

    public class BetterTTVService
    {
        public async Task<IEnumerable<BetterTTVEmoteModel>> GetGlobalBetterTTVEmotes()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    List<BetterTTVEmoteModel> emotes = new List<BetterTTVEmoteModel>();

                    HttpResponseMessage response = await client.GetAsync("https://api.betterttv.net/3/cached/emotes/global");
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.ProcessResponse<List<BetterTTVEmoteModel>>();
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return new List<BetterTTVEmoteModel>();
        }

        public async Task<IEnumerable<BetterTTVEmoteModel>> GetTwitchBetterTTVEmotes(string twitchID) { return await this.GetPlatformBetterTTVEmotes("twitch/" + twitchID); }

        public async Task<IEnumerable<BetterTTVEmoteModel>> GetYouTubeBetterTTVEmotes(string youtubeID) { return await this.GetPlatformBetterTTVEmotes("youtube/" + youtubeID); }

        private async Task<IEnumerable<BetterTTVEmoteModel>> GetPlatformBetterTTVEmotes(string subUrl)
        {
            List<BetterTTVEmoteModel> emotes = new List<BetterTTVEmoteModel>();

            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.betterttv.net/3/cached/users/" + subUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        JObject jobj = await response.ProcessJObjectResponse();
                        if (jobj != null)
                        {
                            JToken channelEmotes = jobj.SelectToken("channelEmotes");
                            if (channelEmotes != null)
                            {
                                emotes.AddRange(((JArray)channelEmotes).ToTypedArray<BetterTTVEmoteModel>());
                            }

                            JToken sharedEmotes = jobj.SelectToken("sharedEmotes");
                            if (sharedEmotes != null)
                            {
                                emotes.AddRange(((JArray)sharedEmotes).ToTypedArray<BetterTTVEmoteModel>());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return emotes;
        }
    }
}
