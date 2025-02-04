using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class GetVoicesRequestModel
    {
        [DataMember]
        public string userId { get; set; }
        [DataMember]
        public string apiKey { get; set; }

        public GetVoicesRequestModel(string userID, string apiKey)
        {
            this.userId = userID;
            this.apiKey = apiKey;
        }
    }

    [DataContract]
    public class GetVoicesResponseModel
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public JObject message { get; set; }

        public string Plan
        {
            get
            {
                if (this.message != null && this.message.TryGetValue("plan", out JToken plan) && ((JObject)plan).TryGetValue("name", out JToken name))
                {
                    return name.ToString();
                }
                return string.Empty;
            }
        }

        public IEnumerable<string> Voices
        {
            get
            {
                if (this.message != null && this.message.TryGetValue("voices", out JToken value) && value is JArray)
                {
                    return ((JArray)value).ToTypedArray<string>();
                }
                return new List<string>();
            }
        }
    }

    [DataContract]
    public class GenerateTTSRequestModel
    {
        [DataMember]
        public string userId { get; set; }
        [DataMember]
        public string key { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public bool ai { get; set; } = true;
        [DataMember]
        public JObject details { get; set; } = new JObject()
        {
            { "provider", "mix-it-up" }
        };

        public GenerateTTSRequestModel(string userID, string key, string voice, string message)
        {
            this.userId = userID;
            this.key = key;
            this.message = $"{voice}: {message}";
        }
    }

    [DataContract]
    public class GenerateTTSResponseModel
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public JObject data { get; set; }

        public string URL
        {
            get
            {
                if (this.data != null && this.data.TryGetValue("link", out JToken value))
                {
                    return value.ToString();
                }
                return string.Empty;
            }
        }
    }

    public class TTSMonsterService : OAuthExternalServiceBase, ITTSMonsterService
    {
        private static readonly HashSet<string> BlockedVoices = new HashSet<string>() { "diana", "dagoth", "tate", "gordon", "megan" };

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.TTSMonster; } }

        public int VolumeMinimum { get { return 0; } }
        public int VolumeMaximum { get { return 100; } }
        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }
        public int PitchMaximum { get { return 0; } }
        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }
        public int RateMaximum { get { return 0; } }
        public int RateDefault { get { return 0; } }

        public override string Name { get { return Resources.TTSMonster; } }

        private List<TextToSpeechVoice> voicesCache = new List<TextToSpeechVoice>();

        public TTSMonsterService() : base(string.Empty) { }

        public override Task<Result> Connect()
        {
            return Task.FromResult(new Result(false));
        }

        public override Task Disconnect() { return Task.CompletedTask; }

        protected override async Task<Result> InitializeInternal()
        {
            try
            {
                this.voicesCache.Clear();
                using (AdvancedHttpClient client = new AdvancedHttpClient("https://wutface.tts.monster/"))
                {
                    GetVoicesResponseModel response = await client.PostAsync<GetVoicesResponseModel>(string.Empty, AdvancedHttpClient.CreateContentFromObject(new GetVoicesRequestModel(this.token.clientID, this.token.accessToken)));
                    if (response != null)
                    {
                        foreach (string voice in response.Voices)
                        {
                            if (!TTSMonsterService.BlockedVoices.Contains(voice))
                            {
                                this.voicesCache.Add(new TextToSpeechVoice(voice));
                            }
                        }

                        if (this.voicesCache.Count > 0)
                        {
                            return new Result();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.TTSMonsterFailedToGetVoices);
        }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }

        public override OAuthTokenModel GetOAuthTokenCopy()
        {
            if (this.token != null)
            {
                return new OAuthTokenModel()
                {
                    clientID = this.token.clientID,
                    accessToken = this.token.accessToken,
                };
            }
            return null;
        }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return this.voicesCache; }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient("https://us-central1-tts-monster.cloudfunctions.net/"))
                {
                    client.Timeout = new TimeSpan(0, 0, 10);

                    JObject content = new JObject();
                    content["data"] = JObject.FromObject(new GenerateTTSRequestModel(this.token.clientID, this.token.accessToken, voice, text));
                    GenerateTTSResponseModel response = await client.PostAsync<GenerateTTSResponseModel>("generateTTS", AdvancedHttpClient.CreateContentFromObject(content));
                    if (response != null && response.status == 200)
                    {
                        string filename = response.URL.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        filename = filename.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        using (WebClient webClient = new WebClient())
                        {
                            string filePath = Path.Combine(ServiceManager.Get<IFileService>().GetTempFolder(), filename);
                            await webClient.DownloadFileTaskAsync(new Uri(response.URL), filePath);
                            await ServiceManager.Get<IAudioService>().Play(filePath, volume, outputDevice, waitForFinish: waitForFinish);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
