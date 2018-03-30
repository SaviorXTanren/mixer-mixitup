using Mixer.Base.Util;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class WindowsTextToSpeechService : ITextToSpeechService
    {
        public Task SayText(string text, TextToSpeechVoice voice, SpeechRate rate, SpeechVolume volume)
        {
            PromptStyle style = new PromptStyle();
            
            switch (rate)
            {
                case SpeechRate.ExtraFast: style.Rate = PromptRate.ExtraFast; break;
                case SpeechRate.Fast: style.Rate = PromptRate.Fast; break;
                case SpeechRate.Medium: style.Rate = PromptRate.Medium; break;
                case SpeechRate.Slow: style.Rate = PromptRate.Slow; break;
                case SpeechRate.ExtraSlow: style.Rate = PromptRate.ExtraSlow; break;
            }

            switch (volume)
            {
                case SpeechVolume.Default: style.Volume = PromptVolume.Default; break;
                case SpeechVolume.ExtraLoud: style.Volume = PromptVolume.ExtraLoud; break;
                case SpeechVolume.Loud: style.Volume = PromptVolume.Loud; break;
                case SpeechVolume.Medium: style.Volume = PromptVolume.Medium; break;
                case SpeechVolume.Soft: style.Volume = PromptVolume.Soft; break;
                case SpeechVolume.ExtraSoft: style.Volume = PromptVolume.ExtraSoft; break;
            }

            PromptBuilder prompt = new PromptBuilder();
            prompt.StartStyle(style);
            prompt.AppendText(text);
            prompt.EndStyle();

            Task.Run(async () =>
            {
                try
                {
                    using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                    {
                        if (voice == null)
                        {
                            voice = this.GetInstalledVoices().FirstOrDefault();
                        }

                        if (voice != null)
                        {
                            synthesizer.SelectVoice(voice.Name);
                        }

                        Prompt promptAsync = synthesizer.SpeakAsync(prompt);
                        while (!promptAsync.IsCompleted)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            });

            return Task.FromResult(0);
        }

        public IEnumerable<TextToSpeechVoice> GetInstalledVoices()
        {
            List<TextToSpeechVoice> voices = new List<TextToSpeechVoice>();
            try
            {
                using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                {
                    foreach (var voice in synthesizer.GetInstalledVoices())
                    {
                        if (voice.Enabled)
                        {
                            voices.Add(new TextToSpeechVoice()
                            {
                                Name = voice.VoiceInfo.Name
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return voices;
        }
    }
}
