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

    public interface IVoicemodService : IOAuthExternalService
    {
        Task VoiceChangerOnOff(bool state);
        Task<IEnumerable<VoicemodVoiceModel>> GetVoices();
    }

    public class VoicemodEmptyService : OAuthExternalServiceBase, IVoicemodService
    {
        public override string Name { get { return "Voicemod"; } }

        public VoicemodEmptyService() : base(string.Empty) { }

        public override Task<Result> Connect() { return Task.FromResult(new Result(success: false)); }

        public override Task Disconnect() { return Task.CompletedTask; }

        public Task VoiceChangerOnOff(bool state) { return Task.CompletedTask; }

        public Task<IEnumerable<VoicemodVoiceModel>> GetVoices() { return Task.FromResult<IEnumerable<VoicemodVoiceModel>>(new List<VoicemodVoiceModel>()); }

        protected override Task<Result> InitializeInternal() { return Task.FromResult(new Result(success: false)); }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }
    }
}
