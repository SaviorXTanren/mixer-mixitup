using MixItUp.Base.ViewModel.Chat;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class BetterTTVEmoteModel : ChatEmoteViewModelBase
    {
        public string id { get; set; }
        public string channel { get; set; }
        public string code { get; set; }
        public string imageType { get; set; }

        public override string ID { get { return this.id; } protected set { } }
        public override string Name { get { return this.code; } protected set { } }
        public override string ImageURL { get { return string.Format("https://cdn.betterttv.net/emote/{0}/3x", this.id); } protected set { } }
        public override string AnimatedImageURL { get { return string.Format("https://cdn.betterttv.net/emote/{0}/3x", this.id); } protected set { } }
        public override string OverlayAnimatedImageURL { get { return string.Format("https://cdn.betterttv.net/emote/{0}/3x.webp", this.id); } }

        public override bool IsAnimated { get { return string.Equals(this.imageType, "gif", StringComparison.OrdinalIgnoreCase); } }
    }

    public class BetterTTVService
    {
        public IReadOnlyDictionary<string, BetterTTVEmoteModel> BetterTTVEmotes { get { return this.betterTTVEmotes; } }
        private ConcurrentDictionary<string, BetterTTVEmoteModel> betterTTVEmotes = new ConcurrentDictionary<string, BetterTTVEmoteModel>();

        public async Task DownloadGlobalBetterTTVEmotes()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    List<BetterTTVEmoteModel> emotes = new List<BetterTTVEmoteModel>();

                    HttpResponseMessage response = await client.GetAsync("https://api.betterttv.net/3/cached/emotes/global");
                    if (response.IsSuccessStatusCode)
                    {
                        foreach (BetterTTVEmoteModel emote in await response.ProcessResponse<List<BetterTTVEmoteModel>>())
                        {
                            this.betterTTVEmotes[emote.code] = emote;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task DownloadTwitchBetterTTVEmotes(string twitchID) { await this.DownloadPlatformBetterTTVEmotes("twitch/" + twitchID); }

        public async Task DownloadYouTubeBetterTTVEmotes(string youtubeID) { await this.DownloadPlatformBetterTTVEmotes("youtube/" + youtubeID); }

        private async Task DownloadPlatformBetterTTVEmotes(string subUrl)
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

            foreach (BetterTTVEmoteModel emote in emotes)
            {
                this.betterTTVEmotes[emote.code] = emote;
            }
        }
    }
}
