using Mixer.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
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

    public class TextToSpeechVoice : IEquatable<TextToSpeechVoice>
    {
        public string Name { get; set; }

        public override bool Equals(object other)
        {
            if (other is TextToSpeechVoice)
            {
                return this.Equals((TextToSpeechVoice)other);
            }
            return false;
        }

        public bool Equals(TextToSpeechVoice other) { return this.Name.Equals(other.Name); }

        public override int GetHashCode() { return this.Name.GetHashCode(); }
    }

    public interface ITextToSpeechService
    {
        Task SayText(string text, TextToSpeechVoice voice, SpeechRate rate, SpeechVolume volume);

        IEnumerable<TextToSpeechVoice> GetInstalledVoices();
    }
}
