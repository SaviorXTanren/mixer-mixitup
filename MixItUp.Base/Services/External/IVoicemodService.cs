using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class VoicemodVoiceModel
    {
        public string voiceID { get; set; }
        public string friendlyName { get; set; }
    }

    public class VoicemodMemeModel
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
    }

    public enum VoicemodRandomVoiceType
    {
        AllVoices,
        FavoriteVoices,
        CustomVoices,
    }

    public interface IVoicemodService : IExternalService
    {
        Task<IEnumerable<VoicemodVoiceModel>> GetVoices();
        Task VoiceChangerOnOff(bool state);
        Task SelectVoice(string voiceID);
        Task RandomVoice(VoicemodRandomVoiceType voiceType);

        Task BeepSoundOnOff(bool state);
        Task HearMyselfOnOff(bool state);
        Task MuteOnOff(bool state);

        Task<IEnumerable<VoicemodMemeModel>> GetMemeSounds();
        Task PlayMemeSound(string fileName);
        Task StopAllMemeSounds();
    }

    public class VoicemodEmptyService : IVoicemodService
    {
        public string Name { get { return "Voicemod"; } }

        public bool IsConnected { get { return false; } }

        public VoicemodEmptyService() { }

        public Task<Result> Connect() { return Task.FromResult(new Result(success: false)); }

        public Task Disconnect() { return Task.CompletedTask; }

        public Task<IEnumerable<VoicemodVoiceModel>> GetVoices() { return Task.FromResult<IEnumerable<VoicemodVoiceModel>>(new List<VoicemodVoiceModel>()); }

        public Task VoiceChangerOnOff(bool state) { return Task.CompletedTask; }

        public Task SelectVoice(string voiceID) { return Task.CompletedTask; }

        public Task RandomVoice(VoicemodRandomVoiceType voiceType) { return Task.CompletedTask; }

        public Task BeepSoundOnOff(bool state) { return Task.CompletedTask; }

        public Task HearMyselfOnOff(bool state) { return Task.CompletedTask; }

        public Task MuteOnOff(bool state) { return Task.CompletedTask; }

        public Task<IEnumerable<VoicemodMemeModel>> GetMemeSounds() { return Task.FromResult<IEnumerable<VoicemodMemeModel>>(new List<VoicemodMemeModel>()); }

        public Task PlayMemeSound(string fileName) { return Task.CompletedTask; }

        public Task StopAllMemeSounds() { return Task.CompletedTask; }
    }
}
