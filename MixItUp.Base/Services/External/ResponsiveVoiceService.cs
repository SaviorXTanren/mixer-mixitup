using MixItUp.Base.Model.Overlay;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class ResponsiveVoiceService : ITextToSpeechService
    {
        public static readonly IEnumerable<TextToSpeechVoice> AvailableVoices = new List<TextToSpeechVoice>()
        {
            new TextToSpeechVoice("Afrikaans Male"),
            new TextToSpeechVoice("Albanian Male"),
            new TextToSpeechVoice("Arabic Female"),
            new TextToSpeechVoice("Arabic Male"),
            new TextToSpeechVoice("Armenian Male"),
            new TextToSpeechVoice("Australian Female"),
            new TextToSpeechVoice("Australian Male"),
            new TextToSpeechVoice("Bangla Bangladesh Female"),
            new TextToSpeechVoice("Bangla Bangladesh Male"),
            new TextToSpeechVoice("Bangla India Female"),
            new TextToSpeechVoice("Bangla India Male"),
            new TextToSpeechVoice("Bosnian Male"),
            new TextToSpeechVoice("Brazilian Portuguese Female"),
            new TextToSpeechVoice("Catalan Male"),
            new TextToSpeechVoice("Chinese (Hong Kong) Female"),
            new TextToSpeechVoice("Chinese (Hong Kong) Male"),
            new TextToSpeechVoice("Chinese Female"),
            new TextToSpeechVoice("Chinese Male"),
            new TextToSpeechVoice("Chinese Taiwan Female"),
            new TextToSpeechVoice("Chinese Taiwan Male"),
            new TextToSpeechVoice("Croatian Male"),
            new TextToSpeechVoice("Czech Female"),
            new TextToSpeechVoice("Danish Female"),
            new TextToSpeechVoice("Deutsch Female"),
            new TextToSpeechVoice("Deutsch Male"),
            new TextToSpeechVoice("Dutch Female"),
            new TextToSpeechVoice("Dutch Male"),
            new TextToSpeechVoice("Esperanto Male"),
            new TextToSpeechVoice("Estonian Male"),
            new TextToSpeechVoice("Fallback UK Female"),
            new TextToSpeechVoice("Filipino Female"),
            new TextToSpeechVoice("Finnish Female"),
            new TextToSpeechVoice("French Canadian Female"),
            new TextToSpeechVoice("French Canadian Male"),
            new TextToSpeechVoice("French Female"),
            new TextToSpeechVoice("French Male"),
            new TextToSpeechVoice("Greek Female"),
            new TextToSpeechVoice("Hindi Female"),
            new TextToSpeechVoice("Hindi Male"),
            new TextToSpeechVoice("Hungarian Female"),
            new TextToSpeechVoice("Icelandic Female"),
            new TextToSpeechVoice("Indonesian Female"),
            new TextToSpeechVoice("Indonesian Male"),
            new TextToSpeechVoice("Italian Female"),
            new TextToSpeechVoice("Italian Male"),
            new TextToSpeechVoice("Japanese Female"),
            new TextToSpeechVoice("Japanese Male"),
            new TextToSpeechVoice("Korean Female"),
            new TextToSpeechVoice("Korean Male"),
            new TextToSpeechVoice("Latin Male"),
            new TextToSpeechVoice("Latvian Male"),
            new TextToSpeechVoice("Macedonian Male"),
            new TextToSpeechVoice("Moldavian Female"),
            new TextToSpeechVoice("Montenegrin Male"),
            new TextToSpeechVoice("Nepali"),
            new TextToSpeechVoice("Norwegian Female"),
            new TextToSpeechVoice("Norwegian Male"),
            new TextToSpeechVoice("Polish Female"),
            new TextToSpeechVoice("Polish Male"),
            new TextToSpeechVoice("Portuguese Female"),
            new TextToSpeechVoice("Portuguese Male"),
            new TextToSpeechVoice("Romanian Female"),
            new TextToSpeechVoice("Russian Female"),
            new TextToSpeechVoice("Serbian Male"),
            new TextToSpeechVoice("Serbo-Croatian Male"),
            new TextToSpeechVoice("Sinhala"),
            new TextToSpeechVoice("Slovak Female"),
            new TextToSpeechVoice("Spanish Female"),
            new TextToSpeechVoice("Spanish Latin American Female"),
            new TextToSpeechVoice("Spanish Latin American Male"),
            new TextToSpeechVoice("Swahili Male"),
            new TextToSpeechVoice("Swedish Female"),
            new TextToSpeechVoice("Swedish Male"),
            new TextToSpeechVoice("Tamil Female"),
            new TextToSpeechVoice("Tamil Male"),
            new TextToSpeechVoice("Thai Female"),
            new TextToSpeechVoice("Thai Male"),
            new TextToSpeechVoice("Turkish Female"),
            new TextToSpeechVoice("Turkish Male"),
            new TextToSpeechVoice("UK English Female"),
            new TextToSpeechVoice("UK English Male"),
            new TextToSpeechVoice("Ukrainian Female"),
            new TextToSpeechVoice("US English Female"),
            new TextToSpeechVoice("US English Male"),
            new TextToSpeechVoice("Vietnamese Female"),
            new TextToSpeechVoice("Vietnamese Male"),
            new TextToSpeechVoice("Welsh Male"),
        };

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.ResponsiveVoice; } }

        public int VolumeMinimum { get { return 0; } }
        public int VolumeMaximum { get { return 100; } }
        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }
        public int PitchMaximum { get { return 200; } }
        public int PitchDefault { get { return 100; } }

        public int RateMinimum { get { return 0; } }
        public int RateMaximum { get { return 150; } }
        public int RateDefault { get { return 100; } }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return ResponsiveVoiceService.AvailableVoices; }

        private HashSet<string> completedRequests = new HashSet<string>();

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(overlayEndpointID);
            if (overlay != null)
            {
                if (waitForFinish)
                {
                    overlay.OnPacketReceived += Overlay_OnPacketReceived;
                }

                OverlayResponsiveVoiceTextToSpeechV3Model ttsRequset = new OverlayResponsiveVoiceTextToSpeechV3Model(text, voice, ((double)volume) / 100.0, ((double)pitch) / 100.0, ((double)rate) / 100.0, waitForFinish);
                await overlay.ResponsiveVoice(ttsRequset);

                if (waitForFinish)
                {
                    DateTimeOffset start = DateTimeOffset.Now;
                    do
                    {
                        await Task.Delay(100);
                    } while (!this.completedRequests.Contains(ttsRequset.ID.ToString()) && (DateTimeOffset.Now - start).TotalMinutes < 1);

                    this.completedRequests.Remove(ttsRequset.ID.ToString());
                    overlay.OnPacketReceived -= Overlay_OnPacketReceived;
                }
            }
        }

        private void Overlay_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            if (packet.Type.Equals("ResponseVoiceTextToSpeechComplete") && packet.Data.TryGetValue("ID", out JToken value))
            {
                this.completedRequests.Add(value.ToString());
            }
        }
    }
}
