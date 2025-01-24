using MixItUp.Base.Model;
using MixItUp.Base.Services.Twitch;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class TikTokTTSService : ITextToSpeechService
    {
        public static readonly IEnumerable<TextToSpeechVoice> AvailableVoices = new List<TextToSpeechVoice>()
        {
            new TextToSpeechVoice("en_us_001", "English US - Female"),
            new TextToSpeechVoice("en_us_006", "English US - Male 1"),
            new TextToSpeechVoice("en_us_007", "English US - Male 2"),
            new TextToSpeechVoice("en_us_009", "English US - Male 3"),
            new TextToSpeechVoice("en_us_010", "English US - Male 4"),

            new TextToSpeechVoice("en_uk_001", "English UK - Male 1"),
            new TextToSpeechVoice("en_uk_003", "English UK - Male 2"),

            new TextToSpeechVoice("en_au_001", "English AU - Female"),
            new TextToSpeechVoice("en_au_002", "English AU - Male"),

            new TextToSpeechVoice("fr_001", "French - Male 1"),
            new TextToSpeechVoice("fr_001", "French - Male 2"),

            new TextToSpeechVoice("de_001", "German - Female"),
            new TextToSpeechVoice("de_002", "German - Male"),

            new TextToSpeechVoice("es_002", "Spanish - Male"),

            new TextToSpeechVoice("es_mx_002", "Spanish MX - Male 1"),
            new TextToSpeechVoice("es_male_m3", "Spanish MX - Male 2"),
            new TextToSpeechVoice("es_female_f6", "Spanish MX - Female 1"),
            new TextToSpeechVoice("es_female_fp1", "Spanish MX - Female 2"),
            new TextToSpeechVoice("es_mx_female_supermom", "Spanish MX - Female 3"),

            new TextToSpeechVoice("br_003", "Portuguese BR - Female 2"),
            new TextToSpeechVoice("br_004", "Portuguese BR - Female 3"),
            new TextToSpeechVoice("br_005", "Portuguese BR - Male"),

            new TextToSpeechVoice("id_001", "Indonesian - Female"),

            new TextToSpeechVoice("jp_001", "Japanese - Female 1"),
            new TextToSpeechVoice("jp_003", "Japanese - Female 2"),
            new TextToSpeechVoice("jp_005", "Japanese - Female 3"),
            new TextToSpeechVoice("jp_006", "Japanese - Male"),

            new TextToSpeechVoice("kr_002", "Korean - Male 1"),
            new TextToSpeechVoice("kr_004", "Korean - Male 2"),
            new TextToSpeechVoice("kr_003", "Korean - Female"),

            new TextToSpeechVoice("en_female_f08_salut_damour", "Alto"),
            new TextToSpeechVoice("en_male_m2_xhxs_m03_silly", "Chipmunk"),
            new TextToSpeechVoice("en_female_ht_f08_wonderful_world", "Dramatic"),
            new TextToSpeechVoice("en_female_ht_f08_glorious", "Glorious"),
            new TextToSpeechVoice("en_male_sing_funny_it_goes_up", "It Goes Up"),
            new TextToSpeechVoice("en_male_m03_sunshine_soon", "Sunshine Soon"),
            new TextToSpeechVoice("en_male_m03_lobby", "Tenor"),
            new TextToSpeechVoice("en_female_f08_warmy_breeze", "Warmy Breeze"),

            new TextToSpeechVoice("en_us_c3po", "C3PO (Star Wars)"),
            new TextToSpeechVoice("en_us_chewbacca", "Chewbacca (Star Wars)"),
            new TextToSpeechVoice("en_us_ghostface", "Ghostface (Scream)"),
            new TextToSpeechVoice("en_us_rocket", "Rocket (Guardians of the Galaxy)"),
            new TextToSpeechVoice("en_us_stitch", "Stitch (Lilo & Stitch)"),
            new TextToSpeechVoice("en_us_stormtrooper", "Stormtrooper (Star Wars)"),
        };

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.TikTokTTS; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return TikTokTTSService.AvailableVoices; }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                client.DefaultRequestHeaders.Add("User-Agent", $"MixItUp/{Assembly.GetEntryAssembly().GetName().Version.ToString()} (Web call from Mix It Up; https://mixitupapp.com; support@mixitupapp.com)");

                JObject body = new JObject();
                body["text"] = text;
                body["voice"] = voice;

                HttpResponseMessage response = await client.PostAsync("https://tiktok-tts.weilbyte.dev/api/generate", AdvancedHttpClient.CreateContentFromObject(body));
                if (response.IsSuccessStatusCode)
                {
                    MemoryStream stream = new MemoryStream();
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        responseStream.CopyTo(stream);
                        stream.Position = 0;
                    }

                    await ServiceManager.Get<IAudioService>().PlayMP3Stream(stream, volume, outputDevice, waitForFinish: waitForFinish);
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Logger.Log(LogLevel.Error, "TikTok TTS Error: " + content);
                    await ServiceManager.Get<ChatService>().SendMessage("TikTok TTS Error: " + content, StreamingPlatformTypeEnum.All);
                }
            }
        }
    }
}
