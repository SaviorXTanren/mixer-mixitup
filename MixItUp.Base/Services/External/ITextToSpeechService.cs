using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public enum TextToSpeechProviderType
    {
        WindowsTextToSpeech,
        ResponsiveVoice,
        AmazonPolly,
    }

    public class TextToSpeechVoice
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public TextToSpeechVoice(string name) : this(name, name) { }

        public TextToSpeechVoice(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }

    public interface ITextToSpeechService
    {
        TextToSpeechProviderType ProviderType { get; }

        int VolumeMinimum { get; }
        int VolumeMaximum { get; }
        int VolumeDefault { get; }

        int PitchMinimum { get; }
        int PitchMaximum { get; }
        int PitchDefault { get; }

        int RateMinimum { get; }
        int RateMaximum { get; }
        int RateDefault { get; }

        IEnumerable<TextToSpeechVoice> GetVoices();

        Task Speak(Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool waitForFinish);
    }
}
