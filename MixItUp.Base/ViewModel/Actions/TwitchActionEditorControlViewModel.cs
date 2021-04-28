using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.ChannelPoints;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TwitchActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Twitch; } }

        public IEnumerable<TwitchActionType> ActionTypes { get { return EnumHelper.GetEnumList<TwitchActionType>(); } }

        public TwitchActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowUsernameGrid");
                this.NotifyPropertyChanged("ShowAdGrid");
                this.NotifyPropertyChanged("ShowClipsGrid");
                this.NotifyPropertyChanged("ShowStreamMarkerGrid");
                this.NotifyPropertyChanged("ShowUpdateChannelPointRewardGrid");
            }
        }
        private TwitchActionType selectedActionType;

        public bool ShowInfoInChat
        {
            get { return this.showInfoInChat; }
            set
            {
                this.showInfoInChat = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showInfoInChat;

        public bool ShowUsernameGrid
        {
            get
            {
                return this.SelectedActionType == TwitchActionType.Host || this.SelectedActionType == TwitchActionType.Raid ||
                    this.SelectedActionType == TwitchActionType.VIPUser || this.SelectedActionType == TwitchActionType.UnVIPUser;
            }
        }

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;

        public bool ShowAdGrid { get { return this.SelectedActionType == TwitchActionType.RunAd; } }

        public IEnumerable<int> AdLengths { get { return TwitchActionModel.SupportedAdLengths; } }

        public int SelectedAdLength
        {
            get { return this.selectedAdLength; }
            set
            {
                this.selectedAdLength = value;
                this.NotifyPropertyChanged();
            }
        }
        private int selectedAdLength = TwitchActionModel.SupportedAdLengths.FirstOrDefault();

        public bool ShowClipsGrid { get { return this.SelectedActionType == TwitchActionType.Clip; } }

        public bool ClipIncludeDelay
        {
            get { return this.clipIncludeDelay; }
            set
            {
                this.clipIncludeDelay = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool clipIncludeDelay;

        public bool ShowStreamMarkerGrid { get { return this.SelectedActionType == TwitchActionType.StreamMarker; } }

        public string StreamMarkerDescription
        {
            get { return this.streamMarkerDescription; }
            set
            {
                this.streamMarkerDescription = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamMarkerDescription;

        public bool ShowUpdateChannelPointRewardGrid { get { return this.SelectedActionType == TwitchActionType.UpdateChannelPointReward; } }

        public ObservableCollection<CustomChannelPointRewardModel> ChannelPointRewards { get; set; } = new ObservableCollection<CustomChannelPointRewardModel>();

        public CustomChannelPointRewardModel ChannelPointReward
        {
            get { return this.channelPointReward; }
            set
            {
                this.channelPointReward = value;
                this.NotifyPropertyChanged();

                if (this.existingChannelPointRewardID == Guid.Empty)
                {
                    this.ChannelPointRewardState = this.ChannelPointReward.is_enabled;
                    this.ChannelPointRewardCost = this.ChannelPointReward.cost.ToString();
                    this.ChannelPointRewardMaxPerStream = this.ChannelPointReward.max_per_stream_setting.max_per_stream.ToString();
                    this.ChannelPointRewardMaxPerUser = this.ChannelPointReward.max_per_user_per_stream_setting.max_per_user_per_stream.ToString();
                    this.ChannelPointRewardGlobalCooldown = (this.ChannelPointReward.global_cooldown_setting.global_cooldown_seconds / 60).ToString();
                    this.ChannelPointRewardUpdateCooldownsAndLimits = (this.ChannelPointReward.max_per_stream_setting.is_enabled || this.ChannelPointReward.max_per_user_per_stream_setting.is_enabled || this.ChannelPointReward.global_cooldown_setting.is_enabled);
                }
                this.existingChannelPointRewardID = Guid.Empty;
            }
        }
        private CustomChannelPointRewardModel channelPointReward;

        public bool ChannelPointRewardState
        {
            get { return this.channelPointRewardState; }
            set
            {
                this.channelPointRewardState = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool channelPointRewardState;

        public string ChannelPointRewardCost
        {
            get { return (this.channelPointRewardCost > 0) ? this.channelPointRewardCost.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.channelPointRewardCost = cost;
                }
                else
                {
                    this.channelPointRewardCost = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int channelPointRewardCost = 0;

        public bool ChannelPointRewardUpdateCooldownsAndLimits
        {
            get { return this.channelPointRewardUpdateCooldownsAndLimits; }
            set
            {
                this.channelPointRewardUpdateCooldownsAndLimits = value;
                this.NotifyPropertyChanged();

                if (!this.ChannelPointRewardUpdateCooldownsAndLimits)
                {
                    this.ChannelPointRewardMaxPerStream = string.Empty;
                    this.ChannelPointRewardMaxPerUser = string.Empty;
                    this.ChannelPointRewardGlobalCooldown = string.Empty;
                }
            }
        }
        private bool channelPointRewardUpdateCooldownsAndLimits = false;

        public string ChannelPointRewardMaxPerStream
        {
            get { return (this.channelPointRewardMaxPerStream > 0) ? this.channelPointRewardMaxPerStream.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.channelPointRewardMaxPerStream = cost;
                }
                else
                {
                    this.channelPointRewardMaxPerStream = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int channelPointRewardMaxPerStream = 0;

        public string ChannelPointRewardMaxPerUser
        {
            get { return (this.channelPointRewardMaxPerUser > 0) ? this.channelPointRewardMaxPerUser.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.channelPointRewardMaxPerUser = cost;
                }
                else
                {
                    this.channelPointRewardMaxPerUser = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int channelPointRewardMaxPerUser = 0;

        public string ChannelPointRewardGlobalCooldown
        {
            get { return (this.channelPointRewardGlobalCooldown > 0) ? this.channelPointRewardGlobalCooldown.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.channelPointRewardGlobalCooldown = cost;
                }
                else
                {
                    this.channelPointRewardGlobalCooldown = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int channelPointRewardGlobalCooldown = 0;

        private Guid existingChannelPointRewardID;

        public TwitchActionEditorControlViewModel(TwitchActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowUsernameGrid)
            {
                this.Username = action.Username;
            }
            else if (this.ShowAdGrid)
            {
                this.SelectedAdLength = action.AdLength;
            }
            else if (this.ShowClipsGrid)
            {
                this.ClipIncludeDelay = action.ClipIncludeDelay;
                this.ShowInfoInChat = action.ShowInfoInChat;
            }
            else if (this.ShowStreamMarkerGrid)
            {
                this.StreamMarkerDescription = action.StreamMarkerDescription;
                this.ShowInfoInChat = action.ShowInfoInChat;
            }
            else if (this.ShowUpdateChannelPointRewardGrid)
            {
                this.existingChannelPointRewardID = action.ChannelPointRewardID;
                this.ChannelPointRewardState = action.ChannelPointRewardState;
                this.channelPointRewardCost = action.ChannelPointRewardCost;
                this.ChannelPointRewardUpdateCooldownsAndLimits = action.ChannelPointRewardUpdateCooldownsAndLimits;
                this.channelPointRewardMaxPerStream = action.ChannelPointRewardMaxPerStream;
                this.channelPointRewardMaxPerUser = action.ChannelPointRewardMaxPerUser;
                this.channelPointRewardGlobalCooldown = action.ChannelPointRewardGlobalCooldown;
            }
        }

        public TwitchActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowStreamMarkerGrid)
            {
                if (!string.IsNullOrEmpty(this.StreamMarkerDescription) && this.StreamMarkerDescription.Length > TwitchActionModel.StreamMarkerMaxDescriptionLength)
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.StreamMarkerDescriptionMustBe140CharactersOrLess));
                }
            }
            else if (this.ShowUpdateChannelPointRewardGrid)
            {
                if (this.ChannelPointReward == null)
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.ChannelPointRewardMissing));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override async Task OnLoadedInternal()
        {
            foreach (CustomChannelPointRewardModel channelPoint in (await ServiceManager.Get<TwitchSessionService>().UserConnection.GetCustomChannelPointRewards(ServiceManager.Get<TwitchSessionService>().UserNewAPI)).OrderBy(c => c.title))
            {
                this.ChannelPointRewards.Add(channelPoint);
            }

            if (this.ShowUpdateChannelPointRewardGrid)
            {
                this.ChannelPointReward = this.ChannelPointRewards.FirstOrDefault(c => c.id.Equals(this.existingChannelPointRewardID));
            }

            await base.OnLoadedInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowUsernameGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateUserAction(this.SelectedActionType, this.Username));
            }
            else if (this.ShowAdGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateAdAction(this.SelectedAdLength));
            }
            else if (this.ShowClipsGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateClipAction(this.ClipIncludeDelay, this.ShowInfoInChat));
            }
            else if (this.ShowStreamMarkerGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateStreamMarkerAction(this.StreamMarkerDescription, this.ShowInfoInChat));
            }
            else if (this.ShowUpdateChannelPointRewardGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateUpdateChannelPointReward(this.ChannelPointReward.id, this.ChannelPointRewardState, this.channelPointRewardCost,
                    this.ChannelPointRewardUpdateCooldownsAndLimits, this.channelPointRewardMaxPerStream, this.channelPointRewardMaxPerUser, this.channelPointRewardGlobalCooldown));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
