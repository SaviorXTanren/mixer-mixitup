using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class PixelChatActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.PixelChat; } }

        public IEnumerable<PixelChatActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<PixelChatActionTypeEnum>(); } }

        public PixelChatActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowOverlays");
                this.NotifyPropertyChanged("ShowTargetUsernameGrid");
                this.NotifyPropertyChanged("ShowTimeAmountGrid");

                this.SelectedOverlay = null;
                this.Overlays.Clear();

                if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout)
                {
                    this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.ShoutoutOverlayType)));
                }
                else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup)
                {
                    this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.TimerOverlayType)));
                }
                else if (this.SelectedActionType == PixelChatActionTypeEnum.StartStreamathon || this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime)
                {
                    this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.StreamathonOverlayType)));
                }
                else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerCredits)
                {
                    this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.CreditsOverlayType)));
                }
                else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerGiveaway)
                {
                    this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.GiveawayOverlayType)));
                }
            }
        }
        private PixelChatActionTypeEnum selectedActionType;

        public bool ShowOverlays
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown ||
                    this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup || this.SelectedActionType == PixelChatActionTypeEnum.StartStreamathon ||
                    this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCredits ||
                    this.SelectedActionType == PixelChatActionTypeEnum.TriggerGiveaway;
            }
        }

        public ThreadSafeObservableCollection<PixelChatOverlayModel> Overlays { get; set; } = new ThreadSafeObservableCollection<PixelChatOverlayModel>();

        public PixelChatOverlayModel SelectedOverlay
        {
            get { return this.selectedOverlay; }
            set
            {
                this.selectedOverlay = value;
                this.NotifyPropertyChanged();
            }
        }
        private PixelChatOverlayModel selectedOverlay;

        public bool ShowTargetUsernameGrid
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout;
            }
        }

        public string TargetUsername
        {
            get { return this.targetUsername; }
            set
            {
                this.targetUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string targetUsername;

        public bool ShowTimeAmountGrid
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup ||
                    this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime;
            }
        }

        public string TimeAmount
        {
            get { return this.timeAmount; }
            set
            {
                this.timeAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string timeAmount;

        private List<PixelChatOverlayModel> allOverlays = new List<PixelChatOverlayModel>();

        public PixelChatActionEditorControlViewModel(PixelChatActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowOverlays)
            {
                this.SelectedOverlay = this.allOverlays.FirstOrDefault(o => o.id.Equals(action.OverlayID));
                if (this.ShowTargetUsernameGrid)
                {
                    this.TargetUsername = action.TargetUsername;
                }
                else if (this.ShowTimeAmountGrid)
                {
                    this.TimeAmount = action.TimeAmount;
                }
            }
        }

        public PixelChatActionEditorControlViewModel()
            : base()
        {
            this.SelectedActionType = PixelChatActionTypeEnum.TriggerGiveaway;
        }

        public override async Task<Result> Validate()
        {
            if (this.ShowOverlays)
            {
                if (this.SelectedOverlay == null)
                {
                    return new Result(MixItUp.Base.Resources.PixelChatActionMissingOverlay);
                }

                if (this.ShowTimeAmountGrid)
                {
                    if (string.IsNullOrEmpty(this.TimeAmount))
                    {
                        return new Result(MixItUp.Base.Resources.PixelChatActionMissingTimeAmount);
                    }
                }
            }
            return await base.Validate();
        }

        protected override async Task OnLoadedInternal()
        {
            if (ChannelSession.Services.PixelChat.IsConnected)
            {
                foreach (PixelChatOverlayModel overlay in (await ChannelSession.Services.PixelChat.GetOverlays()).OrderBy(o => o.Name))
                {
                    this.allOverlays.Add(overlay);
                }
            }
            await base.OnLoadedInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowOverlays)
            {
                if (this.ShowTargetUsernameGrid)
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateOverlayTargetUser(this.SelectedActionType, this.SelectedOverlay.id, this.TargetUsername));
                }
                else if (this.ShowTimeAmountGrid)
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateOverlayTimeAmount(this.SelectedActionType, this.SelectedOverlay.id, this.TimeAmount));
                }
                else
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateBasicOverlay(this.SelectedActionType, this.SelectedOverlay.id));
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
