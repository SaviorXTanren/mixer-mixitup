using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.ChannelPoints;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.Clips;
using MixItUp.Base.Model.Twitch.Polls;
using MixItUp.Base.Model.Twitch.Predictions;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum TwitchActionType
    {
        [Obsolete]
        Host,
        Raid,
        VIPUser,
        UnVIPUser,
        RunAd,
        Clip,
        StreamMarker,
        UpdateChannelPointReward,
        CreatePoll,
        CreatePrediction,
        [Obsolete]
        EnableEmoteOnly,
        [Obsolete]
        DisableEmoteOnly,
        [Obsolete]
        EnableFollowersOnly,
        [Obsolete]
        DisableFollowersOnly,
        [Obsolete]
        EnableSlowChat,
        [Obsolete]
        DisableSlowChat,
        [Obsolete]
        EnableSubscribersChat,
        [Obsolete]
        DisableSubscriberChat,
        SetTitle,
        SetGame,
        SetCustomTags,
        SendChatAnnouncement,
        SendShoutout,
        SetContentClassificationLabels,
        SnoozeNextAd,
        SetChatSettings,
    }

    public enum TwitchAnnouncementColor
    {
        Primary = 0,
        Blue,
        Green,
        Orange,
        Purple
    }

    [DataContract]
    public class TwitchActionModel : GroupActionModel
    {
        public const string ClipURLSpecialIdentifier = "clipurl";
        public const string PollChoiceSpecialIdentifier = "pollchoice";
        public const string PredictionOutcomeSpecialIdentifier = "predictionoutcome";

        public const int StreamMarkerMaxDescriptionLength = 140;

        private const string StartinCommercialBreakMessage = "Starting commercial break.";

        private readonly Dictionary<TwitchAnnouncementColor, string> AnnouncementColorMap = new Dictionary<TwitchAnnouncementColor, string>
        {
            { TwitchAnnouncementColor.Primary, "primary" },
            { TwitchAnnouncementColor.Blue, "blue" },
            { TwitchAnnouncementColor.Green, "green" },
            { TwitchAnnouncementColor.Orange, "orange" },
            { TwitchAnnouncementColor.Purple, "purple" },
        };

        public static readonly IEnumerable<int> SupportedAdLengths = new List<int>() { 30, 60, 90, 120, 150, 180 };

        public static TwitchActionModel CreateUserAction(TwitchActionType type, string username)
        {
            TwitchActionModel action = new TwitchActionModel(type);
            action.Username = username;
            return action;
        }

        public static TwitchActionModel CreateTextAction(TwitchActionType type, string text)
        {
            TwitchActionModel action = new TwitchActionModel(type);
            action.Text = text;
            return action;
        }

        public static TwitchActionModel CreateSetCustomTagsAction(IEnumerable<string> customTags)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.SetCustomTags);
            action.CustomTags.AddRange(customTags);
            return action;
        }

        public static TwitchActionModel CreateAdAction(int adLength)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.RunAd);
            action.AdLength = adLength;
            return action;
        }

        public static TwitchActionModel CreateClipAction(bool includeDelay, bool showInfoInChat)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.Clip);
            action.ClipIncludeDelay = includeDelay;
            action.ShowInfoInChat = showInfoInChat;
            return action;
        }

        public static TwitchActionModel CreateStreamMarkerAction(string description)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.StreamMarker);
            actionModel.StreamMarkerDescription = description;
            return actionModel;
        }

        public static TwitchActionModel CreateUpdateChannelPointReward(Guid id, string name, string description, bool state, bool paused, string backgroundColor, string cost, bool updateCooldownsAndLimits, string maxPerStream, string maxPerUser, string globalCooldown)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.UpdateChannelPointReward);
            action.ChannelPointRewardID = id;
            action.ChannelPointRewardName = name;
            action.ChannelPointRewardDescription = description;
            action.ChannelPointRewardState = state;
            action.ChannelPointRewardPaused = paused;
            action.ChannelPointRewardBackgroundColor = backgroundColor;
            action.ChannelPointRewardCostString = cost;
            action.ChannelPointRewardUpdateCooldownsAndLimits = updateCooldownsAndLimits;
            action.ChannelPointRewardMaxPerStreamString = maxPerStream;
            action.ChannelPointRewardMaxPerUserString = maxPerUser;
            action.ChannelPointRewardGlobalCooldownString = globalCooldown;
            return action;
        }

        public static TwitchActionModel CreatePollAction(string title, int duration, int channelPointCost, int bitCost, IEnumerable<string> choices, IEnumerable<ActionModelBase> actions)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.CreatePoll);
            action.PollTitle = title;
            action.PollDurationSeconds = duration;
            action.PollChannelPointsCost = channelPointCost;
            action.PollBitsCost = bitCost;
            action.PollChoices = new List<string>(choices);
            action.Actions = new List<ActionModelBase>(actions);
            return action;
        }

        public static TwitchActionModel CreatePredictionAction(string title, int duration, IEnumerable<string> outcomes, IEnumerable<ActionModelBase> actions)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.CreatePrediction);
            action.PredictionTitle = title;
            action.PredictionDurationSeconds = duration;
            action.PredictionOutcomes = new List<string>(outcomes);
            action.Actions = new List<ActionModelBase>(actions);
            return action;
        }

        public static TwitchActionModel CreateTimeAction(TwitchActionType type, string timeLength)
        {
            TwitchActionModel action = new TwitchActionModel(type);
            action.TimeLength = timeLength;
            return action;
        }

        public static TwitchActionModel CreateSendChatAnnouncementAction(string message, TwitchAnnouncementColor color, bool sendAsStreamer)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.SendChatAnnouncement);
            actionModel.Message = message;
            actionModel.Color= color;
            actionModel.SendAnnouncementAsStreamer = sendAsStreamer;
            return actionModel;
        }

        public static TwitchActionModel CreateSetContentClassificationLabelsAction(IEnumerable<ChannelContentClassificationLabelModel> ccls)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.SetContentClassificationLabels);
            actionModel.ContentClassificationLabelIDs = new List<string>(ccls.Select(l => l.id));
            return actionModel;
        }

        public static TwitchActionModel CreateSetChatSettingsAction(int? slowModeDuration, int? followerModeDuration, bool? subscriberMode, bool? emoteMode, bool? uniqueChatMode, int? nonModeratorChatDuration)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.SetChatSettings);
            actionModel.ChatSettingsSlowModeDuration = slowModeDuration;
            actionModel.ChatSettingsFollowerModeDuration = followerModeDuration;
            actionModel.ChatSettingsSubscriberMode = subscriberMode;
            actionModel.ChatSettingsEmoteMode = emoteMode;
            actionModel.ChatSettingsUniqueChatMode = uniqueChatMode;
            actionModel.ChatSettingsNonModeratorChatDuration = nonModeratorChatDuration;
            return actionModel;
        }

        public static TwitchActionModel CreateVIPUserAction(string username, DurationSpan automaticRemovalDurationSpan = null)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.VIPUser);
            actionModel.Username = username;
            actionModel.VIPUserAutomaticRemovalDurationSpan = automaticRemovalDurationSpan;
            return actionModel;
        }

        public static TwitchActionModel CreateAction(TwitchActionType type)
        {
            return new TwitchActionModel(type);
        }

        [DataMember]
        public TwitchActionType ActionType { get; set; }
        [DataMember]
        public bool ShowInfoInChat { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public List<string> CustomTags { get; set; } = new List<string>();

        [DataMember]
        public int AdLength { get; set; } = 60;

        [DataMember]
        public bool ClipIncludeDelay { get; set; }

        [DataMember]
        public string StreamMarkerDescription { get; set; }

        [DataMember]
        public Guid ChannelPointRewardID { get; set; }
        [DataMember]
        public string ChannelPointRewardName { get; set; }
        [DataMember]
        public string ChannelPointRewardDescription { get; set; }
        [DataMember]
        public bool ChannelPointRewardState { get; set; }
        [DataMember]
        public bool ChannelPointRewardPaused { get; set; }
        [DataMember]
        public string ChannelPointRewardBackgroundColor { get; set; }
        [DataMember]
        public string ChannelPointRewardCostString { get; set; }
        [DataMember]
        public bool ChannelPointRewardUpdateCooldownsAndLimits { get; set; }
        [DataMember]
        public string ChannelPointRewardMaxPerStreamString { get; set; }
        [DataMember]
        public string ChannelPointRewardMaxPerUserString { get; set; }
        [DataMember]
        public string ChannelPointRewardGlobalCooldownString { get; set; }
        [Obsolete]
        [DataMember]
        public int ChannelPointRewardCost { get; set; } = -1;
        [Obsolete]
        [DataMember]
        public int ChannelPointRewardMaxPerStream { get; set; } = -1;
        [Obsolete]
        [DataMember]
        public int ChannelPointRewardMaxPerUser { get; set; } = -1;
        [Obsolete]
        [DataMember]
        public int ChannelPointRewardGlobalCooldown { get; set; } = -1;

        [DataMember]
        public string PollTitle { get; set; }
        [DataMember]
        public int PollDurationSeconds { get; set; }
        [DataMember]
        public int PollChannelPointsCost { get; set; }
        [DataMember]
        public int PollBitsCost { get; set; }
        [DataMember]
        public List<string> PollChoices { get; set; } = new List<string>();

        [DataMember]
        public string PredictionTitle { get; set; }
        [DataMember]
        public int PredictionDurationSeconds { get; set; }
        [DataMember]
        public List<string> PredictionOutcomes { get; set; } = new List<string>();

        [DataMember]
        public string TimeLength { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public TwitchAnnouncementColor Color { get; set; }

        [DataMember]
        public bool SendAnnouncementAsStreamer { get; set; } = true;

        [DataMember]
        public List<string> ContentClassificationLabelIDs = new List<string>();

        [DataMember]
        public int? ChatSettingsSlowModeDuration = null;
        [DataMember]
        public int? ChatSettingsFollowerModeDuration = null;
        [DataMember]
        public bool? ChatSettingsSubscriberMode = null;
        [DataMember]
        public bool? ChatSettingsEmoteMode = null;
        [DataMember]
        public bool? ChatSettingsUniqueChatMode = null;
        [DataMember]
        public int? ChatSettingsNonModeratorChatDuration = null;

        [DataMember]
        public DurationSpan VIPUserAutomaticRemovalDurationSpan = null;

        private TwitchActionModel(TwitchActionType type)
            : base(ActionTypeEnum.Twitch)
        {
            this.ActionType = type;
        }

        [Obsolete]
        public TwitchActionModel() { }

        public void UpdateSetChatSettingsProperties()
        {
            #pragma warning disable CS0612 // Type or member is obsolete
            int.TryParse(this.TimeLength, out int timeLength);

            if (this.ActionType == TwitchActionType.EnableSlowChat)
            {
                this.ChatSettingsSlowModeDuration = timeLength;
            }
            else if (this.ActionType == TwitchActionType.DisableSlowChat)
            {
                this.ChatSettingsSlowModeDuration = 0;
            }
            else if (this.ActionType == TwitchActionType.EnableFollowersOnly)
            {
                this.ChatSettingsFollowerModeDuration = timeLength;
            }
            else if (this.ActionType == TwitchActionType.DisableFollowersOnly)
            {
                this.ChatSettingsFollowerModeDuration = 0;
            }
            else if (this.ActionType == TwitchActionType.EnableEmoteOnly)
            {
                this.ChatSettingsEmoteMode = true;
            }
            else if (this.ActionType == TwitchActionType.DisableEmoteOnly)
            {
                this.ChatSettingsEmoteMode = false;
            }
            else if (this.ActionType == TwitchActionType.EnableSubscribersChat)
            {
                this.ChatSettingsSubscriberMode = true;
            }
            else if (this.ActionType == TwitchActionType.DisableSubscriberChat)
            {
                this.ChatSettingsSubscriberMode = false;
            }
            else
            {
                return;
            }

            this.ActionType = TwitchActionType.SetChatSettings;

            #pragma warning restore CS0612 // Type or member is obsolete
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected)
            {
                this.UpdateSetChatSettingsProperties();

                if (this.ActionType == TwitchActionType.Raid)
                {
                    string channelName = await ReplaceStringWithSpecialModifiers(this.Username, parameters);
                    UserModel targetChannel = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIUserByLogin(channelName);
                    if (targetChannel != null)
                    {
                        await ServiceManager.Get<TwitchSession>().StreamerService.RaidChannel(ServiceManager.Get<TwitchSession>().StreamerModel, targetChannel);
                    }
                }
                else if (this.ActionType == TwitchActionType.RunAd)
                {
                    AdResponseModel response = await ServiceManager.Get<TwitchSession>().StreamerService.RunAd(ServiceManager.Get<TwitchSession>().StreamerModel, this.AdLength);
                    if (response == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TwitchAdUnableToRun, parameters);
                    }
                    else if (!string.IsNullOrEmpty(response.message) && !response.message.Contains(StartinCommercialBreakMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + response.message, parameters);
                    }
                }
                else if (this.ActionType == TwitchActionType.VIPUser || this.ActionType == TwitchActionType.UnVIPUser)
                {
                    UserV2ViewModel targetUser = null;
                    if (!string.IsNullOrEmpty(this.Username))
                    {
                        string targetUsername = await ReplaceStringWithSpecialModifiers(this.Username, parameters);
                        targetUser = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformUsername: targetUsername, performPlatformSearch: true);
                    }
                    else
                    {
                        targetUser = parameters.User;
                    }

                    if (targetUser != null)
                    {
                        TwitchUserPlatformV2Model twitchUser = targetUser.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);
                        if (this.ActionType == TwitchActionType.VIPUser)
                        {
                            targetUser.Roles.Add(User.UserRoleEnum.TwitchVIP);
                            await ServiceManager.Get<TwitchSession>().StreamerService.VIPUser(ServiceManager.Get<TwitchSession>().StreamerModel, twitchUser.ID);

                            if (this.VIPUserAutomaticRemovalDurationSpan != null)
                            {
                                ChannelSession.Settings.TwitchVIPAutomaticRemovals[twitchUser.ID] = this.VIPUserAutomaticRemovalDurationSpan.GetDateTimeOffsetFromNow();
                            }
                        }
                        else if (this.ActionType == TwitchActionType.UnVIPUser)
                        {
                            targetUser.Roles.Remove(User.UserRoleEnum.TwitchVIP);
                            await ServiceManager.Get<TwitchSession>().StreamerService.UnVIPUser(ServiceManager.Get<TwitchSession>().StreamerModel, twitchUser.ID);
                        }
                    }
                }
                else if (this.ActionType == TwitchActionType.Clip)
                {
                    ClipCreationModel clipCreation = await ServiceManager.Get<TwitchSession>().StreamerService.CreateClip(ServiceManager.Get<TwitchSession>().StreamerModel, this.ClipIncludeDelay);
                    if (clipCreation != null)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            await Task.Delay(5000);

                            ClipModel clip = await ServiceManager.Get<TwitchSession>().StreamerService.GetClip(clipCreation);
                            if (clip != null && !string.IsNullOrEmpty(clip.url))
                            {
                                if (this.ShowInfoInChat)
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.ClipCreatedMessage, clip.url), parameters);
                                }
                                parameters.SpecialIdentifiers[ClipURLSpecialIdentifier] = clip.url;
                                return;
                            }
                        }
                    }
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ClipCreationFailed, parameters);
                }
                else if (this.ActionType == TwitchActionType.StreamMarker)
                {
                    string description = await ReplaceStringWithSpecialModifiers(this.StreamMarkerDescription, parameters);
                    if (!string.IsNullOrEmpty(description) && description.Length > TwitchActionModel.StreamMarkerMaxDescriptionLength)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamMarkerDescriptionMustBe140CharactersOrLess, parameters);
                        return;
                    }

                    CreatedStreamMarkerModel streamMarker = await ServiceManager.Get<TwitchSession>().StreamerService.CreateStreamMarker(ServiceManager.Get<TwitchSession>().StreamerModel, description);
                    if (streamMarker == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamMarkerCreationFailed, parameters);
                    }
                }
                else if (this.ActionType == TwitchActionType.UpdateChannelPointReward)
                {
                    JObject jobj = new JObject()
                    {
                        { "is_enabled", this.ChannelPointRewardState },
                        { "is_paused", this.ChannelPointRewardPaused }
                    };

#pragma warning disable CS0612 // Type or member is obsolete
                    if (this.ChannelPointRewardCost >= 0)
                    {
                        this.ChannelPointRewardCostString = this.ChannelPointRewardCost.ToString();
                        this.ChannelPointRewardMaxPerStreamString = this.ChannelPointRewardMaxPerStream.ToString();
                        this.ChannelPointRewardMaxPerUserString = this.ChannelPointRewardMaxPerUser.ToString();
                        this.ChannelPointRewardGlobalCooldownString = this.ChannelPointRewardGlobalCooldown.ToString();

                        this.ChannelPointRewardCost = -1;
                        this.ChannelPointRewardMaxPerStream = -1;
                        this.ChannelPointRewardMaxPerUser = -1;
                        this.ChannelPointRewardGlobalCooldown = -1;
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    if (!string.IsNullOrEmpty(this.ChannelPointRewardName))
                    {
                        jobj["title"] = await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardName, parameters);
                    }

                    if (!string.IsNullOrEmpty(this.ChannelPointRewardDescription))
                    {
                        jobj["prompt"] = await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardDescription, parameters);
                    }

                    if (!string.IsNullOrEmpty(this.ChannelPointRewardBackgroundColor))
                    {
                        jobj["background_color"] = await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardBackgroundColor, parameters);
                    }

                    int.TryParse(await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardCostString, parameters), out int cost);
                    if (cost > 0) { jobj["cost"] = cost; }

                    if (this.ChannelPointRewardUpdateCooldownsAndLimits)
                    {
                        int.TryParse(await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardMaxPerStreamString, parameters), out int maxPerStream);
                        if (maxPerStream > 0)
                        {
                            jobj["is_max_per_stream_enabled"] = true;
                            jobj["max_per_stream"] = maxPerStream;
                        }
                        else
                        {
                            jobj["is_max_per_stream_enabled"] = false;
                            jobj["max_per_stream"] = 0;
                        }

                        int.TryParse(await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardMaxPerUserString, parameters), out int maxPerUser);
                        if (maxPerUser > 0)
                        {
                            jobj["is_max_per_user_per_stream_enabled"] = true;
                            jobj["max_per_user_per_stream"] = maxPerUser;
                        }
                        else
                        {
                            jobj["is_max_per_user_per_stream_enabled"] = false;
                            jobj["max_per_user_per_stream"] = 0;
                        }

                        int.TryParse(await ReplaceStringWithSpecialModifiers(this.ChannelPointRewardGlobalCooldownString, parameters), out int globalCooldown);
                        if (globalCooldown > 0)
                        {
                            jobj["is_global_cooldown_enabled"] = true;
                            jobj["global_cooldown_seconds"] = globalCooldown * 60;
                        }
                        else
                        {
                            jobj["is_global_cooldown_enabled"] = false;
                            jobj["global_cooldown_seconds"] = 0;
                        }
                    }

                    CustomChannelPointRewardModel reward = await ServiceManager.Get<TwitchSession>().StreamerService.UpdateCustomChannelPointReward(ServiceManager.Get<TwitchSession>().StreamerModel, this.ChannelPointRewardID, jobj);
                    if (reward == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TwitchActionChannelPointRewardCouldNotBeUpdated, parameters);
                    }
                }
                else if (this.ActionType == TwitchActionType.CreatePoll)
                {
                    List<CreatePollChoiceModel> choices = new List<CreatePollChoiceModel>();
                    foreach (string choice in this.PollChoices)
                    {
                        choices.Add(new CreatePollChoiceModel()
                        {
                            title = await ReplaceStringWithSpecialModifiers(choice, parameters)
                        });
                    }

                    PollModel poll = await ServiceManager.Get<TwitchSession>().StreamerService.CreatePoll(new CreatePollModel()
                    {
                        broadcaster_id = ServiceManager.Get<TwitchSession>().StreamerID,
                        title = await ReplaceStringWithSpecialModifiers(this.PollTitle, parameters),
                        duration = this.PollDurationSeconds,
                        channel_points_voting_enabled = this.PollChannelPointsCost > 0,
                        channel_points_per_vote = this.PollChannelPointsCost,
                        bits_voting_enabled = this.PollBitsCost > 0,
                        bits_per_vote = this.PollBitsCost,
                        choices = choices
                    });

                    if (poll == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TwitchPollFailedToCreate);
                        return;
                    }

                    if (this.Actions.Count > 0)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                        {
                            await Task.Delay(1000 * (this.PollDurationSeconds + 2));

                            for (int i = 0; i < 5; i++)
                            {
                                PollModel results = await ServiceManager.Get<TwitchSession>().StreamerService.GetPoll(ServiceManager.Get<TwitchSession>().StreamerModel, poll.id);
                                if (results != null)
                                {
                                    if (string.Equals(results.status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int maxVotes = results.choices.Max(c => c.votes);
                                        IEnumerable<PollChoiceModel> winningChoices = results.choices.Where(c => c.votes == maxVotes);
                                        parameters.SpecialIdentifiers[PollChoiceSpecialIdentifier] = string.Join(" & ", winningChoices.Select(c => c.title));

                                        await this.RunSubActions(parameters);
                                        return;
                                    }
                                    else if (!string.Equals(results.status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return;
                                    }
                                }

                                await Task.Delay(2000);
                            }

                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TwitchPollFailedToGetResults, parameters);
                        }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
                else if (this.ActionType == TwitchActionType.CreatePrediction)
                {
                    List<CreatePredictionOutcomeModel> outcomes = new List<CreatePredictionOutcomeModel>();
                    foreach (string outcome in this.PredictionOutcomes)
                    {
                        outcomes.Add(new CreatePredictionOutcomeModel()
                        {
                            title = await ReplaceStringWithSpecialModifiers(outcome, parameters)
                        });
                    }

                    PredictionModel prediction = await ServiceManager.Get<TwitchSession>().StreamerService.CreatePrediction(new CreatePredictionModel()
                    {
                        broadcaster_id = ServiceManager.Get<TwitchSession>().StreamerID,
                        title = await ReplaceStringWithSpecialModifiers(this.PredictionTitle, parameters),
                        prediction_window = this.PredictionDurationSeconds,
                        outcomes = outcomes
                    });

                    if (prediction == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TwitchPredictionFailedToCreate, parameters);
                        return;
                    }

                    if (this.Actions.Count > 0)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                        {
                            await Task.Delay(1000 * this.PredictionDurationSeconds);

                            while (true)
                            {
                                await Task.Delay(10000);

                                PredictionModel results = await ServiceManager.Get<TwitchSession>().StreamerService.GetPrediction(ServiceManager.Get<TwitchSession>().StreamerModel, prediction.id);
                                if (results != null)
                                {
                                    if (string.Equals(results.status, "RESOLVED", StringComparison.OrdinalIgnoreCase))
                                    {
                                        PredictionOutcomeModel outcome = results.outcomes.FirstOrDefault(o => string.Equals(o.id, results.winning_outcome_id));

                                        parameters.SpecialIdentifiers[PredictionOutcomeSpecialIdentifier] = outcome?.title;

                                        await this.RunSubActions(parameters);
                                        return;
                                    }
                                    else if (string.Equals(results.status, "CANCELED", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return;
                                    }
                                }
                            }
                        }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
                else if (this.ActionType == TwitchActionType.SetChatSettings)
                {
                    ChatSettingsModel settings = await ServiceManager.Get<TwitchSession>().StreamerService.GetChatSettings(ServiceManager.Get<TwitchSession>().StreamerModel);
                    if (settings != null)
                    {
                        if (this.ChatSettingsSlowModeDuration != null)
                        {
                            if (this.ChatSettingsSlowModeDuration > 0)
                            {
                                settings.slow_mode = true;
                                settings.slow_mode_wait_time = this.ChatSettingsSlowModeDuration.GetValueOrDefault();
                            }
                            else
                            {
                                settings.slow_mode = false;
                            }
                        }

                        if (this.ChatSettingsFollowerModeDuration != null)
                        {
                            if (this.ChatSettingsFollowerModeDuration > 0)
                            {
                                settings.follower_mode = true;
                                settings.follower_mode_duration = this.ChatSettingsFollowerModeDuration.GetValueOrDefault();
                            }
                            else
                            {
                                settings.follower_mode = false;
                            }
                        }

                        if (this.ChatSettingsEmoteMode != null)
                        {
                            settings.emote_mode = this.ChatSettingsEmoteMode.GetValueOrDefault();
                        }

                        if (this.ChatSettingsSubscriberMode != null)
                        {
                            settings.subscriber_mode = this.ChatSettingsSubscriberMode.GetValueOrDefault();
                        }

                        if (this.ChatSettingsUniqueChatMode != null)
                        {
                            settings.unique_chat_mode = this.ChatSettingsUniqueChatMode.GetValueOrDefault();
                        }

                        if (this.ChatSettingsNonModeratorChatDuration != null)
                        {
                            if (this.ChatSettingsNonModeratorChatDuration > 0)
                            {
                                settings.non_moderator_chat_delay = true;
                                settings.non_moderator_chat_delay_duration = this.ChatSettingsNonModeratorChatDuration.GetValueOrDefault();
                            }
                            else
                            {
                                settings.non_moderator_chat_delay = false;
                            }
                        }

                        await ServiceManager.Get<TwitchSession>().StreamerService.UpdateChatSettings(ServiceManager.Get<TwitchSession>().StreamerModel, settings);
                    }
                }
                else if (this.ActionType == TwitchActionType.SetTitle)
                {
                    string text = await ReplaceStringWithSpecialModifiers(this.Text, parameters);
                    await ServiceManager.Get<TwitchSession>().StreamerService.UpdateChannelInformation(ServiceManager.Get<TwitchSession>().StreamerModel, title: text);
                }
                else if (this.ActionType == TwitchActionType.SetGame)
                {
                    string text = await ReplaceStringWithSpecialModifiers(this.Text, parameters);
                    await ServiceManager.Get<TwitchSession>().StreamerService.SetGame(ServiceManager.Get<TwitchSession>().StreamerModel, text);
                }
                else if (this.ActionType == TwitchActionType.SetCustomTags)
                {
                    await ServiceManager.Get<TwitchSession>().StreamerService.UpdateChannelInformation(ServiceManager.Get<TwitchSession>().StreamerModel, tags: this.CustomTags);
                }
                else if (this.ActionType == TwitchActionType.SendChatAnnouncement)
                {
                    string text = await ReplaceStringWithSpecialModifiers(this.Message, parameters);

                    if (SendAnnouncementAsStreamer || ServiceManager.Get<TwitchSession>().BotModel == null)
                    {
                        await ServiceManager.Get<TwitchSession>().StreamerService.SendChatAnnouncement(ServiceManager.Get<TwitchSession>().StreamerModel, ServiceManager.Get<TwitchSession>().StreamerModel, text, AnnouncementColorMap[Color]);
                    }
                    else
                    {
                        await ServiceManager.Get<TwitchSession>().BotService.SendChatAnnouncement(ServiceManager.Get<TwitchSession>().StreamerModel, ServiceManager.Get<TwitchSession>().BotModel, text, AnnouncementColorMap[Color]);
                    }
                }
                else if (this.ActionType == TwitchActionType.SendShoutout)
                {
                    string targetUsername = null;
                    if (!string.IsNullOrEmpty(this.Username))
                    {
                        targetUsername = await ReplaceStringWithSpecialModifiers(this.Username, parameters);
                    }
                    else
                    {
                        targetUsername = parameters.User.Username;
                    }

                    if (!string.IsNullOrEmpty(targetUsername))
                    {
                        UserModel targetUser = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIUserByLogin(targetUsername);
                        if (targetUser != null)
                        {
                            await ServiceManager.Get<TwitchSession>().StreamerService.SendShoutout(ServiceManager.Get<TwitchSession>().StreamerModel, targetUser);
                        }
                    }
                }
                else if (this.ActionType == TwitchActionType.SetContentClassificationLabels)
                {
                    List<string> cclIdsToAdd = null;
                    if (this.ContentClassificationLabelIDs.Count > 0)
                    {
                        cclIdsToAdd = new List<string>(this.ContentClassificationLabelIDs);
                    }

                    List<string> cclIdsToRemove = null;
                    if (this.ContentClassificationLabelIDs.Count != ServiceManager.Get<TwitchSession>().ContentClassificationLabels.Count)
                    {
                        cclIdsToRemove = new List<string>(ServiceManager.Get<TwitchSession>().ContentClassificationLabels.Where(l => !this.ContentClassificationLabelIDs.Contains(l.id)).Select(l => l.id));
                    }

                    await ServiceManager.Get<TwitchSession>().StreamerService.UpdateChannelInformation(ServiceManager.Get<TwitchSession>().StreamerModel, cclIdsToAdd: cclIdsToAdd, cclIdsToRemove: cclIdsToRemove);
                }
                else if (this.ActionType == TwitchActionType.SnoozeNextAd)
                {
                    await ServiceManager.Get<TwitchSession>().StreamerService.SnoozeNextAd(ServiceManager.Get<TwitchSession>().StreamerModel);
                }
            }
        }
    }
}
