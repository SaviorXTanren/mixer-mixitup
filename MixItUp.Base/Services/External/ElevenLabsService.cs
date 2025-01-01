using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class ElevenLabsVoiceModel
    {
        public string voice_id { get; set; }
        public string name { get; set; }
    }

    public class ElevenLabsService : ITextToSpeechConnectableService
    {
        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.ElevenLabs; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        public string Name { get { return Resources.Elevenlabs; } }

        public bool IsConnected { get; private set; }

        private IEnumerable<TextToSpeechVoice> voicesCache;

        public Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return voicesCache; }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            try
            {
                List<TextToSpeechVoice> voices = new List<TextToSpeechVoice>();
                using (AdvancedHttpClient client = new AdvancedHttpClient("https://api.elevenlabs.io/v1"))
                {
                    client.AddHeader("xi-api-key", ChannelSession.Settings.ElevenLabsAPIKey);

                    JObject body = new JObject();
                    body["text"] = text;
                    body["voice_settings"] = new JObject()
                    {
                        { "stability", 1 },
                        { "similarity_boost", 1 }
                    };

                    HttpResponseMessage response = await client.PostAsync($"text-to-speech/{voice}", AdvancedHttpClient.CreateContentFromObject(body));
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        MemoryStream stream = new MemoryStream();
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                        {
                            responseStream.CopyTo(stream);
                            stream.Position = 0;
                        }
                        await ServiceManager.Get<IAudioService>().PlayMP3Stream(stream, volume, outputDevice, waitForFinish: waitForFinish);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<Result> TestAccess()
        {
            this.voicesCache = await this.GetVoicesInternal();
            if (this.voicesCache != null)
            {
                return new Result();
            }
            return new Result(Resources.ElevenLabsNoVoicesReturn);
        }

        private async Task<IEnumerable<TextToSpeechVoice>> GetVoicesInternal()
        {
            try
            {
                List<TextToSpeechVoice> voices = new List<TextToSpeechVoice>();
                using (AdvancedHttpClient client = new AdvancedHttpClient("https://api.elevenlabs.io/v1"))
                {
                    client.AddHeader("xi-api-key", ChannelSession.Settings.ElevenLabsAPIKey);

                    JObject response = await client.GetJObjectAsync("voices");
                    if (response != null && response.ContainsKey("voices"))
                    {
                        foreach (JObject v in (JArray)response["voices"])
                        {
                            if (v.TryGetValue("voice_id", out JToken voice_id) && v.TryGetValue("name", out JToken name))
                            {
                                voices.Add(new TextToSpeechVoice(voice_id.ToString(), name.ToString()));
                            }
                        }
                        return voices;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
    }
}
