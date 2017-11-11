using MixItUp.Base.Services;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class WindowsTextToSpeechService : ITextToSpeechService
    {
        public async Task SayText(string text, SpeechRate rate, SpeechVolume volume)
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

            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            Prompt promptAsync = synthesizer.SpeakAsync(prompt);
            while (!promptAsync.IsCompleted)
            {
                await Task.Delay(1000);
            }
        }
    }
}
