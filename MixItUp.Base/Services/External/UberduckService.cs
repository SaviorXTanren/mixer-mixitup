using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class UberduckVoicesModel
    {
        public string category { get; set; }
        public string display_name { get; set; }
        public bool is_private { get; set; }
        public string name { get; set; }
        public string voicemodel_uuid { get; set; }
        public string language { get; set; }
    }

    public class UberduckService : ITextToSpeechConnectableService, IExternalService
    {
        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.Uberduck; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        public string Name { get { return Resources.Uberduck; } }

        public bool IsConnected { get; private set; }

        private IEnumerable<TextToSpeechVoice> voices;

        public Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return voices; }

        public Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> TestAccess()
        {
            this.voices = await this.GetVoicesInternal();
            if (this.voices != null)
            {
                return new Result();
            }
            return new Result(Resources.UberduckNoVoicesReturned);
        }

        private async Task<IEnumerable<TextToSpeechVoice>> GetVoicesInternal()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient("https://api.uberduck.ai/"))
                {
                    client.Timeout = new TimeSpan(0, 0, 15);
                    HttpResponseMessage response = await client.GetAsync("voices");
                    if (response.IsSuccessStatusCode)
                    {
                        IEnumerable<UberduckVoicesModel> voices = await response.ProcessResponse<List<UberduckVoicesModel>>();
                        if (voices != null && voices.Count() > 0)
                        {
                            return voices.Select(v => new TextToSpeechVoice(v.voicemodel_uuid, $"{v.display_name} - {v.language}"));
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
    }
}
