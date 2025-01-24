using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class FrankerFaceZEmoteModel : ChatEmoteViewModelBase
    {
        public string id { get; set; }
        public string name { get; set; }
        public JObject urls { get; set; }

        public override string ID { get { return this.id; } protected set { } }
        public override string Name { get { return this.name; } protected set { } }
        public override string ImageURL
        {
            get
            {
                if (this.urls != null && this.urls.Count > 0)
                {
                    return this.urls.ContainsKey("2") ? this.urls["2"].ToString() : this.urls[this.urls.GetKeys().First()].ToString();
                }
                return string.Empty;
            }
            protected set { }
        }
    }

    public class FrankerFaceZService
    {
        public IDictionary<string, FrankerFaceZEmoteModel> FrankerFaceZEmotes { get { return this.frankerFaceZEmotes; } }
        private Dictionary<string, FrankerFaceZEmoteModel> frankerFaceZEmotes = new Dictionary<string, FrankerFaceZEmoteModel>();

        public async Task DownloadGlobalFrankerFaceZEmotes() { await this.DownloadFrankerFaceZEmotes("https://api.frankerfacez.com/v1/set/global"); }

        public async Task DownloadTwitchFrankerFaceZEmotes(string channelName) { await this.DownloadFrankerFaceZEmotes("https://api.frankerfacez.com/v1/room/" + channelName); }

        private async Task DownloadFrankerFaceZEmotes(string url)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    JObject jobj = await client.GetJObjectAsync(url);
                    if (jobj != null && jobj.ContainsKey("sets"))
                    {
                        JObject setsJObj = (JObject)jobj["sets"];
                        foreach (var kvp in setsJObj)
                        {
                            JObject setJObj = (JObject)kvp.Value;
                            if (setJObj != null && setJObj.ContainsKey("emoticons"))
                            {
                                JArray emoticonsJArray = (JArray)setJObj["emoticons"];
                                foreach (FrankerFaceZEmoteModel emote in emoticonsJArray.ToTypedArray<FrankerFaceZEmoteModel>())
                                {
                                    this.frankerFaceZEmotes[emote.name] = emote;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
