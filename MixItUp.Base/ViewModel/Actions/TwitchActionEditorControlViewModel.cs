using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Twitch;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.ChannelPoints;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TwitchActionEditorControlViewModel : SubActionContainerControlViewModel
    {
        private const int PredictionTitleMaxLength = 45;

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
                this.NotifyPropertyChanged("ShowTextGrid");
                this.NotifyPropertyChanged("ShowSetCustomTagsGrid");
                this.NotifyPropertyChanged("ShowAdGrid");
                this.NotifyPropertyChanged("ShowClipsGrid");
                this.NotifyPropertyChanged("ShowStreamMarkerGrid");
                this.NotifyPropertyChanged("ShowUpdateChannelPointRewardGrid");
                this.NotifyPropertyChanged("ShowPollGrid");
                this.NotifyPropertyChanged("ShowPredictionGrid");
                this.NotifyPropertyChanged("ShowSubActions");
                this.NotifyPropertyChanged("ShowFollowersGrid");
                this.NotifyPropertyChanged("ShowSlowChatGrid");
                this.NotifyPropertyChanged("ShowSendAnnouncementGrid");
                this.NotifyPropertyChanged("ShowSetContentClassificationLabelsGrid");
            }
        }
        private TwitchActionType selectedActionType;

        public bool ShowSubActions { get { return this.SelectedActionType == TwitchActionType.CreatePoll || this.SelectedActionType == TwitchActionType.CreatePrediction; } }

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
                return this.SelectedActionType == TwitchActionType.Raid ||
                    this.SelectedActionType == TwitchActionType.VIPUser || this.SelectedActionType == TwitchActionType.UnVIPUser ||
                    this.SelectedActionType == TwitchActionType.SendShoutout;
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

        public bool ShowTextGrid { get { return this.SelectedActionType == TwitchActionType.SetTitle || this.SelectedActionType == TwitchActionType.SetGame; } }

        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.NotifyPropertyChanged();
            }
        }
        private string text;

        public bool ShowSetCustomTagsGrid { get { return this.SelectedActionType == TwitchActionType.SetCustomTags; } }

        public TwitchTagEditorViewModel TagEditor { get; set; } = new TwitchTagEditorViewModel();

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
                    this.ChannelPointRewardPaused = this.ChannelPointReward.is_paused;
                    this.ChannelPointRewardName = this.ChannelPointReward.title;
                    this.ChannelPointRewardDescription = this.ChannelPointReward.prompt;
                    this.ChannelPointRewardBackgroundColor = this.ChannelPointReward.background_color;
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

        public bool ChannelPointRewardPaused
        {
            get { return this.channelPointRewardPaused; }
            set
            {
                this.channelPointRewardPaused = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool channelPointRewardPaused;

        public string ChannelPointRewardName
        {
            get { return this.channelPointRewardName; }
            set
            {
                this.channelPointRewardName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardName;

        public string ChannelPointRewardDescription
        {
            get { return this.channelPointRewardDescription; }
            set
            {
                this.channelPointRewardDescription = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardDescription;

        public string ChannelPointRewardBackgroundColor
        {
            get { return this.channelPointRewardBackgroundColor; }
            set
            {
                this.channelPointRewardBackgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardBackgroundColor;

        public string ChannelPointRewardCost
        {
            get { return this.channelPointRewardCost; }
            set
            {
                this.channelPointRewardCost = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardCost;

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
            get { return this.channelPointRewardMaxPerStream; }
            set
            {
                this.channelPointRewardMaxPerStream = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardMaxPerStream;

        public string ChannelPointRewardMaxPerUser
        {
            get { return this.channelPointRewardMaxPerUser; }
            set
            {
                this.channelPointRewardMaxPerUser = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardMaxPerUser;

        public string ChannelPointRewardGlobalCooldown
        {
            get { return this.channelPointRewardGlobalCooldown; }
            set
            {
                this.channelPointRewardGlobalCooldown = value;
                this.NotifyPropertyChanged();
            }
        }
        private string channelPointRewardGlobalCooldown;

        private Guid existingChannelPointRewardID;

        public bool ShowPollGrid { get { return this.SelectedActionType == TwitchActionType.CreatePoll; } }

        public string PollTitle
        {
            get { return this.pollTitle; }
            set
            {
                this.pollTitle = value;
                this.NotifyPropertyChanged();
            }
        }
        private string pollTitle;

        public int PollDurationSeconds
        {
            get { return this.pollDurationSeconds; }
            set
            {
                this.pollDurationSeconds = value;
                this.NotifyPropertyChanged();
            }
        }
        private int pollDurationSeconds = 60;

        public string PollChannelPointsCost
        {
            get { return (this.pollChannelPointsCost > 0) ? this.pollChannelPointsCost.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.pollChannelPointsCost = cost;
                }
                else
                {
                    this.pollChannelPointsCost = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int pollChannelPointsCost = 0;

        public string PollBitsCost
        {
            get { return (this.pollBitsCost > 0) ? this.pollBitsCost.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int cost) && cost > 0)
                {
                    this.pollBitsCost = cost;
                }
                else
                {
                    this.pollBitsCost = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int pollBitsCost = 0;

        public string PollChoice1
        {
            get { return this.pollChoice1; }
            set
            {
                this.pollChoice1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string pollChoice1;

        public string PollChoice2
        {
            get { return this.pollChoice2; }
            set
            {
                this.pollChoice2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string pollChoice2;

        public string PollChoice3
        {
            get { return this.pollChoice3; }
            set
            {
                this.pollChoice3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string pollChoice3;

        public string PollChoice4
        {
            get { return this.pollChoice4; }
            set
            {
                this.pollChoice4 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string pollChoice4;

        public bool ShowPredictionGrid { get { return this.SelectedActionType == TwitchActionType.CreatePrediction; } }

        public string PredictionTitle
        {
            get { return this.predictionTitle; }
            set
            {
                this.predictionTitle = value;
                this.NotifyPropertyChanged();
            }
        }
        private string predictionTitle;

        public int PredictionDurationSeconds
        {
            get { return this.predictionDurationSeconds; }
            set
            {
                this.predictionDurationSeconds = value;
                this.NotifyPropertyChanged();
            }
        }
        private int predictionDurationSeconds = 60;

        public string PredictionOutcome1
        {
            get { return this.predictionOutcome1; }
            set
            {
                this.predictionOutcome1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string predictionOutcome1;

        public string PredictionOutcome2
        {
            get { return this.predictionOutcome2; }
            set
            {
                this.predictionOutcome2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string predictionOutcome2;

        public bool ShowFollowersGrid { get { return this.SelectedActionType == TwitchActionType.EnableFollowersOnly; } }

        public bool ShowSlowChatGrid { get { return this.SelectedActionType == TwitchActionType.EnableSlowChat; } }
        public bool ShowSendAnnouncementGrid { get { return this.SelectedActionType == TwitchActionType.SendChatAnnouncement; } }

        public string TimeLength
        {
            get { return this.timeLength; }
            set
            {
                this.timeLength = value;
                this.NotifyPropertyChanged();
            }
        }
        private string timeLength;

        public string Message
        {
            get { return this.message; }
            set
            {
                this.message = value;
                this.NotifyPropertyChanged();
            }
        }
        private string message;

        public IEnumerable<TwitchAnnouncementColor> AnnouncementColors { get { return EnumHelper.GetEnumList<TwitchAnnouncementColor>(); } }

        public TwitchAnnouncementColor SelectedAnnouncementColor
        {
            get { return this.selectedAnnouncementColor; }
            set
            {
                this.selectedAnnouncementColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private TwitchAnnouncementColor selectedAnnouncementColor = TwitchAnnouncementColor.Primary;

        public bool SendAnnouncementAsStreamer
        {
            get { return this.sendAnnouncementAsStreamer; }
            set
            {
                this.sendAnnouncementAsStreamer = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool sendAnnouncementAsStreamer = true;

        public bool ShowSetContentClassificationLabelsGrid { get { return this.SelectedActionType == TwitchActionType.SetContentClassificationLabels; } }

        public TwitchContentClassificationLabelsEditorViewModel ContentClassificationLabelsEditor { get; set; } = new TwitchContentClassificationLabelsEditorViewModel();

        private IEnumerable<string> existingTags = null;
        private IEnumerable<string> existingContentClassificationLabelIDs = null;

        public TwitchActionEditorControlViewModel(TwitchActionModel action)
            : base(action, action.Actions)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowUsernameGrid)
            {
                this.Username = action.Username;
            }
            else if (this.ShowTextGrid)
            {
                this.Text = action.Text;
            }
            else if (this.ShowSetCustomTagsGrid)
            {
                this.existingTags = action.CustomTags;
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
#pragma warning disable CS0612 // Type or member is obsolete
                if (action.ChannelPointRewardCost >= 0)
                {
                    action.ChannelPointRewardCostString = action.ChannelPointRewardCost.ToString();
                    action.ChannelPointRewardMaxPerStreamString = action.ChannelPointRewardMaxPerStream.ToString();
                    action.ChannelPointRewardMaxPerUserString = action.ChannelPointRewardMaxPerUser.ToString();
                    action.ChannelPointRewardGlobalCooldownString = action.ChannelPointRewardGlobalCooldown.ToString();

                    action.ChannelPointRewardCost = -1;
                    action.ChannelPointRewardMaxPerStream = -1;
                    action.ChannelPointRewardMaxPerUser = -1;
                    action.ChannelPointRewardGlobalCooldown = -1;
                }
#pragma warning restore CS0612 // Type or member is obsolete

                this.existingChannelPointRewardID = action.ChannelPointRewardID;
                this.ChannelPointRewardState = action.ChannelPointRewardState;
                this.ChannelPointRewardPaused = action.ChannelPointRewardPaused;
                this.ChannelPointRewardName = action.ChannelPointRewardName;
                this.ChannelPointRewardDescription = action.ChannelPointRewardDescription;
                this.ChannelPointRewardBackgroundColor = action.ChannelPointRewardBackgroundColor;
                this.ChannelPointRewardCost = action.ChannelPointRewardCostString;
                this.ChannelPointRewardUpdateCooldownsAndLimits = action.ChannelPointRewardUpdateCooldownsAndLimits;
                this.ChannelPointRewardMaxPerStream = action.ChannelPointRewardMaxPerStreamString;
                this.ChannelPointRewardMaxPerUser = action.ChannelPointRewardMaxPerUserString;
                this.ChannelPointRewardGlobalCooldown = action.ChannelPointRewardGlobalCooldownString;
            }
            else if (this.ShowPollGrid)
            {
                this.PollTitle = action.PollTitle;
                this.PollDurationSeconds = action.PollDurationSeconds;
                this.pollChannelPointsCost = action.PollChannelPointsCost;
                this.pollBitsCost = action.PollBitsCost;
                if (action.PollChoices.Count > 0)
                {
                    this.PollChoice1 = action.PollChoices[0];
                }
                if (action.PollChoices.Count > 1)
                {
                    this.PollChoice2 = action.PollChoices[1];
                }
                if (action.PollChoices.Count > 2)
                {
                    this.PollChoice3 = action.PollChoices[2];
                }
                if (action.PollChoices.Count > 3)
                {
                    this.PollChoice4 = action.PollChoices[3];
                }
            }
            else if (this.ShowPredictionGrid)
            {
                this.PredictionTitle = action.PredictionTitle;
                this.PredictionDurationSeconds = action.PredictionDurationSeconds;
                this.PredictionOutcome1 = action.PredictionOutcomes[0];
                this.PredictionOutcome2 = action.PredictionOutcomes[1];
            }
            else if (this.ShowFollowersGrid || this.ShowSlowChatGrid)
            {
                this.TimeLength = action.TimeLength;
            }
            else if (this.ShowSendAnnouncementGrid)
            {
                this.Message = action.Message;
                this.SelectedAnnouncementColor = action.Color;
                this.SendAnnouncementAsStreamer = action.SendAnnouncementAsStreamer;
            }
            else if (this.ShowSetContentClassificationLabelsGrid)
            {
                this.existingContentClassificationLabelIDs = action.ContentClassificationLabelIDs;
            }
        }

        public TwitchActionEditorControlViewModel() : base() { }

        public override async Task<Result> Validate()
        {
            if (this.ShowUsernameGrid)
            {
                if (string.IsNullOrEmpty(this.Username))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionUsernameMissing);
                }
            }
            else if (this.ShowTextGrid)
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionNameMissing);
                }
            }
            else if (this.ShowStreamMarkerGrid)
            {
                if (!string.IsNullOrEmpty(this.StreamMarkerDescription) && this.StreamMarkerDescription.Length > TwitchActionModel.StreamMarkerMaxDescriptionLength)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionStreamMarkerDescriptionMustBe140CharactersOrLess);
                }
            }
            else if (this.ShowUpdateChannelPointRewardGrid)
            {
                if (this.ChannelPointReward == null)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionChannelPointRewardMissing);
                }
            }
            else if (this.ShowPollGrid)
            {
                if (string.IsNullOrEmpty(this.PollTitle))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePollMissingTitle);
                }

                if (this.PollTitle.Length > 60)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionPollTitleTooLong);
                }

                if (this.PollDurationSeconds <= 0)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePollInvalidDuration);
                }

                if (string.IsNullOrEmpty(this.PollChoice1) || string.IsNullOrEmpty(this.PollChoice2))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePollTwoOrMoreChoices);
                }

                if ((!string.IsNullOrEmpty(this.PollChoice1) && this.PollChoice1.Length > 25) ||
                    (!string.IsNullOrEmpty(this.PollChoice2) && this.PollChoice2.Length > 25) ||
                    (!string.IsNullOrEmpty(this.PollChoice3) && this.PollChoice3.Length > 25) ||
                    (!string.IsNullOrEmpty(this.PollChoice4) && this.PollChoice4.Length > 25))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionPollChoicesTooLong);
                }
            }
            else if (this.ShowPredictionGrid)
            {
                if (string.IsNullOrEmpty(this.PredictionTitle))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePredictionMissingTitle);
                }

                if (this.PredictionTitle.Length > 45)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionPredictionTitleTooLong);
                }

                if (this.PredictionDurationSeconds <= 0)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePredictionInvalidDuration);
                }

                if (string.IsNullOrEmpty(this.PredictionOutcome1) || string.IsNullOrEmpty(this.PredictionOutcome2))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionCreatePredictionTwoChoices);
                }

                if (this.PredictionOutcome1.Length > 25 || this.PredictionOutcome2.Length > 25)
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionPredictionOutcomesTooLong);
                }
            }
            else if (this.ShowFollowersGrid || this.ShowSlowChatGrid)
            {
                if (string.IsNullOrEmpty(this.TimeLength))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionTimeLengthMissing);
                }
            }
            else if (this.ShowSendAnnouncementGrid)
            {
                if (string.IsNullOrEmpty(this.Message))
                {
                    return new Result(MixItUp.Base.Resources.TwitchActionMessageMissing);
                }
            }
            return await base.Validate();
        }

        protected override async Task OnOpenInternal()
        {
            await this.TagEditor.OnOpen();
            await this.ContentClassificationLabelsEditor.OnOpen();

            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                foreach (CustomChannelPointRewardModel channelPoint in (await ServiceManager.Get<TwitchSessionService>().UserConnection.GetCustomChannelPointRewards(ServiceManager.Get<TwitchSessionService>().User, managableRewardsOnly: true)).OrderBy(c => c.title))
                {
                    this.ChannelPointRewards.Add(channelPoint);
                }

                if (this.ShowUpdateChannelPointRewardGrid)
                {
                    this.ChannelPointReward = this.ChannelPointRewards.FirstOrDefault(c => c.id.Equals(this.existingChannelPointRewardID));
                }

                if (this.existingTags != null)
                {
                    foreach (string tag in this.existingTags)
                    {
                        await this.TagEditor.AddCustomTag(tag);
                    }
                }
                else if (ServiceManager.Get<TwitchSessionService>().Channel?.tags != null)
                {
                    foreach (string tag in ServiceManager.Get<TwitchSessionService>().Channel.tags)
                    {
                        await this.TagEditor.AddCustomTag(tag);
                    }
                }

                if (this.existingContentClassificationLabelIDs != null)
                {
                    foreach (string ccl in this.existingContentClassificationLabelIDs)
                    {
                        this.ContentClassificationLabelsEditor.AddLabel(ccl);
                    }
                }
            }
            await base.OnOpenInternal();
        }

        protected override async Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowUsernameGrid)
            {
                return TwitchActionModel.CreateUserAction(this.SelectedActionType, this.Username);
            }
            else if (this.ShowTextGrid)
            {
                return TwitchActionModel.CreateTextAction(this.SelectedActionType, this.Text);
            }
            else if (this.ShowSetCustomTagsGrid)
            {
                return TwitchActionModel.CreateSetCustomTagsAction(this.TagEditor.CustomTags.Select(t => t.Tag));
            }
            else if (this.ShowAdGrid)
            {
                return TwitchActionModel.CreateAdAction(this.SelectedAdLength);
            }
            else if (this.ShowClipsGrid)
            {
                return TwitchActionModel.CreateClipAction(this.ClipIncludeDelay, this.ShowInfoInChat);
            }
            else if (this.ShowStreamMarkerGrid)
            {
                return TwitchActionModel.CreateStreamMarkerAction(this.StreamMarkerDescription);
            }
            else if (this.ShowUpdateChannelPointRewardGrid)
            {
                return TwitchActionModel.CreateUpdateChannelPointReward(this.ChannelPointReward.id, this.ChannelPointRewardName, this.ChannelPointRewardDescription, this.ChannelPointRewardState, this.ChannelPointRewardPaused,
                    this.ChannelPointRewardBackgroundColor, this.ChannelPointRewardCost, this.ChannelPointRewardUpdateCooldownsAndLimits, this.ChannelPointRewardMaxPerStream, this.ChannelPointRewardMaxPerUser, this.ChannelPointRewardGlobalCooldown);
            }
            else if (this.ShowPollGrid)
            {
                List<string> choices = new List<string>();
                if (!string.IsNullOrEmpty(this.PollChoice1)) { choices.Add(this.PollChoice1); }
                if (!string.IsNullOrEmpty(this.PollChoice2)) { choices.Add(this.PollChoice2); }
                if (!string.IsNullOrEmpty(this.PollChoice3)) { choices.Add(this.PollChoice3); }
                if (!string.IsNullOrEmpty(this.PollChoice4)) { choices.Add(this.PollChoice4); }
                return TwitchActionModel.CreatePollAction(this.PollTitle, this.PollDurationSeconds, this.pollChannelPointsCost, this.pollBitsCost, choices, await this.ActionEditorList.GetActions());
            }
            else if (this.ShowPredictionGrid)
            {
                return TwitchActionModel.CreatePredictionAction(this.PredictionTitle, this.PredictionDurationSeconds, new List<string>() { this.PredictionOutcome1, this.PredictionOutcome2 }, await this.ActionEditorList.GetActions());
            }
            else if (this.ShowFollowersGrid)
            {
                return TwitchActionModel.CreateTimeAction(TwitchActionType.EnableFollowersOnly, this.TimeLength);
            }
            else if (this.ShowSlowChatGrid)
            {
                return TwitchActionModel.CreateTimeAction(TwitchActionType.EnableSlowChat, this.TimeLength);
            }
            else if (this.ShowSendAnnouncementGrid)
            {
                return TwitchActionModel.CreateSendChatAnnouncementAction(this.Message, this.SelectedAnnouncementColor, this.SendAnnouncementAsStreamer);
            }
            else if (this.ShowSetContentClassificationLabelsGrid)
            {
                return TwitchActionModel.CreateSetContentClassificationLabelsAction(this.ContentClassificationLabelsEditor.Labels.Select(l => l.Label));
            }
            else
            {
                return TwitchActionModel.CreateAction(this.SelectedActionType);
            }
        }
    }
}
