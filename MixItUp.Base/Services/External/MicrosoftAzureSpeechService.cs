using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class MicrosoftAzureSpeechService : ITextToSpeechService
    {
        private const string BaseURL = "https://eastus.api.cognitive.microsoft.com/";

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.MicrosoftAzureSpeech; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        public IEnumerable<TextToSpeechVoice> GetVoices()
        {
            throw new NotImplementedException();
        }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool waitForFinish)
        {
            voice = "en-US-JennyNeural";

            using (AdvancedHttpClient client = new AdvancedHttpClient(MicrosoftAzureSpeechService.BaseURL))
            {
                client.AddHeader("Ocp-Apim-Subscription-Key", ServiceManager.Get<SecretsService>().GetSecret("AzureSpeechServiceSecret"));
                client.AddHeader("Content-Type", "application/ssml+xml");
                client.AddHeader("User-Agent", "Mix It Up Desktop");
                client.AddHeader("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3");
                HttpResponseMessage response = await client.PostAsync("cognitiveservices/v1", AdvancedHttpClient.CreateContentFromString($"<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='Female' name='{voice}'>{text}</voice></speak>"));

                MemoryStream stream = new MemoryStream();
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    responseStream.CopyTo(stream);
                    stream.Position = 0;
                }
                await ServiceManager.Get<IAudioService>().PlayMP3(stream, volume, outputDevice, waitForFinish: waitForFinish);
            }
        }

        private async Task GenerateVoicesList()
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                client.AddHeader("Ocp-Apim-Subscription-Key", ServiceManager.Get<SecretsService>().GetSecret("AzureSpeechServiceSecret"));
                string content = await client.GetStringAsync("https://eastus.tts.speech.microsoft.com/cognitiveservices/voices/list");
                await ServiceManager.Get<IFileService>().SaveFile("S:\\voices.txt", content);
            }
        }
    }
}
