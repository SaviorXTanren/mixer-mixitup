using Mixer.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SpeechRate
    {
        [Name("Extra Fast")]
        ExtraFast = 0,
        Fast = 1,
        Medium = 2,
        Slow = 3,
        [Name("Extra Slow")]
        ExtraSlow = 4
    }

    public enum SpeechVolume
    {
        Default = 0,
        [Name("Extra Soft")]
        ExtraSoft = 1,
        Soft = 2,
        Medium = 3,
        Loud = 4,
        [Name("Extra Loud")]
        ExtraLoud = 5,
    }

    public interface ITextToSpeechService
    {
        Task SayText(string text, SpeechRate rate, SpeechVolume volume);
    }
}
