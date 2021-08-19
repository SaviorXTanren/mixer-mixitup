using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class VoicemodActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Voicemod; } }

        public bool VoicemodConnected { get { return ChannelSession.Services.Voicemod.IsConnected; } }
        public bool VoicemodNotConnected { get { return !this.VoicemodConnected; } }

        public IEnumerable<VoicemodActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<VoicemodActionTypeEnum>(); } }

        public VoicemodActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowStateGrid");
            }
        }
        private VoicemodActionTypeEnum selectedActionType;

        public bool ShowStateGrid { get { return this.SelectedActionType == VoicemodActionTypeEnum.VoiceChangerOnOff; } }

        public bool State
        {
            get { return this.state; }
            set
            {
                this.state = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool state;

        public VoicemodActionEditorControlViewModel(VoicemodActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowStateGrid)
            {
                this.State = action.State;
            }
        }

        public VoicemodActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == VoicemodActionTypeEnum.VoiceChangerOnOff)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForVoiceChangerOnOff(this.State));
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        protected override async Task OnLoadedInternal()
        {
            if (this.VoicemodConnected)
            {

            }
            await base.OnLoadedInternal();
        }
    }
}