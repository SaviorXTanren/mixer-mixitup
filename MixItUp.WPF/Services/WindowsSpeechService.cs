using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsSpeechService : ITextToSpeechService
    {
        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.WindowsTextToSpeech; } }

        public int VolumeMinimum { get { return 0; } }
        public int VolumeMaximum { get { return 100; } }
        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }
        public int PitchMaximum { get { return 0; } }
        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return -10; } }
        public int RateMaximum { get { return 10; } }
        public int RateDefault { get { return 0; } }

        public IEnumerable<TextToSpeechVoice> GetVoices()
        {
            List<TextToSpeechVoice> voices = new List<TextToSpeechVoice>();

            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                if (voice.Enabled)
                {
                    voices.Add(new TextToSpeechVoice(voice.VoiceInfo.Name));
                }
            }

            return voices;
        }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool waitForFinish)
        {
            MemoryStream stream = new MemoryStream();
            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SetOutputToAudioStream(stream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 88200, 16, 1, 16000, 2, null));
                if (!string.IsNullOrWhiteSpace(voice))
                {
                    synthesizer.SelectVoice(voice);
                }
                synthesizer.Rate = rate;
                synthesizer.Volume = volume;
                synthesizer.Speak(text);

                stream.Position = 0;
            }

            await ServiceManager.Get<IAudioService>().PlayPCM(stream, volume, outputDevice, waitForFinish);
        }
    }
}
