using Mixer.Base.Model.Channel;
using MixItUp.Base.Model.Chat.Mixer;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mixer
{
    public interface IMixrElixrService
    {
        Task<IEnumerable<MixrElixrEmoteModel>> GetChannelEmotes(ChannelModel channel);
    }

    public class MixrElixrService : IMixrElixrService
    {
        private const string BaseAddress = "https://api.mixrelixr.com/v1/";

        public async Task<IEnumerable<MixrElixrEmoteModel>> GetChannelEmotes(ChannelModel channel)
        {
            List<MixrElixrEmoteModel> results = new List<MixrElixrEmoteModel>();
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixrElixrService.BaseAddress))
                {
                    MixrElixrEmotePackageModel emotePackage = await client.GetAsync<MixrElixrEmotePackageModel>("emotes/" + channel.id.ToString());
                    if (emotePackage != null)
                    {
                        if (emotePackage.globalEmotes != null)
                        {
                            this.SetEmoteUrls(emotePackage.globalEmotes, emotePackage.globalEmoteUrlTemplate);
                            results.AddRange(emotePackage.globalEmotes);
                        }

                        if (emotePackage.channelEmotes != null)
                        {
                            this.SetEmoteUrls(emotePackage.channelEmotes, emotePackage.channelEmoteUrlTemplate);
                            results.AddRange(emotePackage.channelEmotes);
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

        private void SetEmoteUrls(IEnumerable<MixrElixrEmoteModel> emotes, string urlTemplate)
        {
            if (emotes != null && !string.IsNullOrEmpty(urlTemplate))
            {
                urlTemplate = "https:" + urlTemplate;
                foreach (MixrElixrEmoteModel emote in emotes)
                {
                    emote.Url = urlTemplate.Replace("{{emoteId}}", emote.id);
                }
            }
        }
    }
}
