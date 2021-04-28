using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Ads;
using Twitch.Base.Models.NewAPI.ChannelPoints;
using Twitch.Base.Models.NewAPI.Clips;
using Twitch.Base.Models.NewAPI.Streams;

namespace MixItUp.Base.Model.Actions
{
    public enum TwitchActionType
    {
        Host,
        Raid,
        VIPUser,
        UnVIPUser,
        RunAd,
        Clip,
        StreamMarker,
        UpdateChannelPointReward,
    }

    [DataContract]
    public class TwitchActionModel : ActionModelBase
    {
        public const string ClipURLSpecialIdentifier = "clipurl";
        public const string StreamMarkerURLSpecialIdentifier = "streammarkerurl";

        public const int StreamMarkerMaxDescriptionLength = 140;

        private const string StartinCommercialBreakMessage = "Starting commercial break.";

        public static readonly IEnumerable<int> SupportedAdLengths = new List<int>() { 30, 60, 90, 120, 150, 180 };

        public static TwitchActionModel CreateUserAction(TwitchActionType type, string username)
        {
            TwitchActionModel action = new TwitchActionModel(type);
            action.Username = username;
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

        public static TwitchActionModel CreateStreamMarkerAction(string description, bool showInfoInChat)
        {
            TwitchActionModel actionModel = new TwitchActionModel(TwitchActionType.StreamMarker);
            actionModel.StreamMarkerDescription = description;
            actionModel.ShowInfoInChat = showInfoInChat;
            return actionModel;
        }

        public static TwitchActionModel CreateUpdateChannelPointReward(Guid id, bool state, int cost, bool updateCooldownsAndLimits, int maxPerStream, int maxPerUser, int globalCooldown)
        {
            TwitchActionModel action = new TwitchActionModel(TwitchActionType.UpdateChannelPointReward);
            action.ChannelPointRewardID = id;
            action.ChannelPointRewardState = state;
            action.ChannelPointRewardCost = cost;
            action.ChannelPointRewardUpdateCooldownsAndLimits = updateCooldownsAndLimits;
            action.ChannelPointRewardMaxPerStream = maxPerStream;
            action.ChannelPointRewardMaxPerUser = maxPerUser;
            action.ChannelPointRewardGlobalCooldown = globalCooldown;
            return action;
        }

        [DataMember]
        public TwitchActionType ActionType { get; set; }
        [DataMember]
        public bool ShowInfoInChat { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int AdLength { get; set; } = 60;

        [DataMember]
        public bool ClipIncludeDelay { get; set; }

        [DataMember]
        public string StreamMarkerDescription { get; set; }

        [DataMember]
        public Guid ChannelPointRewardID { get; set; }
        [DataMember]
        public bool ChannelPointRewardState { get; set; }
        [DataMember]
        public int ChannelPointRewardCost { get; set; }
        [DataMember]
        public bool ChannelPointRewardUpdateCooldownsAndLimits { get; set; }
        [DataMember]
        public int ChannelPointRewardMaxPerStream { get; set; }
        [DataMember]
        public int ChannelPointRewardMaxPerUser { get; set; }
        [DataMember]
        public int ChannelPointRewardGlobalCooldown { get; set; }

        private TwitchActionModel(TwitchActionType type)
            : base(ActionTypeEnum.Twitch)
        {
            this.ActionType = type;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal TwitchActionModel(MixItUp.Base.Actions.StreamingPlatformAction action)
            : base(ActionTypeEnum.Twitch)
        {
            if (action.ActionType == Base.Actions.StreamingPlatformActionType.Host)
            {
                this.ActionType = TwitchActionType.Host;
                this.Username = action.HostChannelName;
            }
            else if (action.ActionType == Base.Actions.StreamingPlatformActionType.Raid)
            {
                this.ActionType = TwitchActionType.Raid;
                this.Username = action.HostChannelName;
            }
            else if (action.ActionType == Base.Actions.StreamingPlatformActionType.RunAd)
            {
                this.ActionType = TwitchActionType.RunAd;
                this.AdLength = action.AdLength;
            }
        }

        internal TwitchActionModel(MixItUp.Base.Actions.ClipsAction action)
            : base(ActionTypeEnum.Twitch)
        {
            this.ActionType = TwitchActionType.Clip;
            this.ClipIncludeDelay = action.IncludeDelay;
            this.ShowInfoInChat = action.ShowClipInfoInChat;
        }

        internal TwitchActionModel(MixItUp.Base.Actions.ModerationAction action)
            : base(ActionTypeEnum.Twitch)
        {
            if (action.ModerationType == Base.Actions.ModerationActionTypeEnum.VIPUser)
            {
                this.ActionType = TwitchActionType.VIPUser;
                this.Username = action.UserName;
            }
            else if (action.ModerationType == Base.Actions.ModerationActionTypeEnum.UnVIPUser)
            {
                this.ActionType = TwitchActionType.UnVIPUser;
                this.Username = action.UserName;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private TwitchActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                if (this.ActionType == TwitchActionType.Host)
                {
                    string channelName = await this.ReplaceStringWithSpecialModifiers(this.Username, parameters);
                    await ServiceManager.Get<ChatService>().SendMessage("/host @" + channelName, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
                }
                else if (this.ActionType == TwitchActionType.Raid)
                {
                    string channelName = await this.ReplaceStringWithSpecialModifiers(this.Username, parameters);
                    await ServiceManager.Get<ChatService>().SendMessage("/raid @" + channelName, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
                }
                else if (this.ActionType == TwitchActionType.RunAd)
                {
                    AdResponseModel response = await ServiceManager.Get<TwitchSessionService>().UserConnection.RunAd(ServiceManager.Get<TwitchSessionService>().UserNewAPI, this.AdLength);
                    if (response == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage("ERROR: We were unable to run an ad, please try again later", StreamingPlatformTypeEnum.Twitch);
                    }
                    else if (!string.IsNullOrEmpty(response.message) && !response.message.Contains(StartinCommercialBreakMessage, System.StringComparison.OrdinalIgnoreCase))
                    {
                        await ServiceManager.Get<ChatService>().SendMessage("ERROR: " + response.message, StreamingPlatformTypeEnum.Twitch);
                    }
                }
                else if (this.ActionType == TwitchActionType.VIPUser || this.ActionType == TwitchActionType.UnVIPUser)
                {
                    UserViewModel targetUser = null;
                    if (!string.IsNullOrEmpty(this.Username))
                    {
                        string username = await this.ReplaceStringWithSpecialModifiers(this.Username, parameters);
                        targetUser = ServiceManager.Get<UserService>().GetUserByUsername(username, StreamingPlatformTypeEnum.Twitch);
                    }
                    else
                    {
                        targetUser = parameters.User;
                    }

                    if (targetUser != null)
                    {
                        if (this.ActionType == TwitchActionType.VIPUser)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage("/vip @" + targetUser.TwitchUsername, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
                        }
                        else if (this.ActionType == TwitchActionType.UnVIPUser)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage("/unvip @" + targetUser.TwitchUsername, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
                        }
                    }
                }
                else if (this.ActionType == TwitchActionType.Clip)
                {
                    ClipCreationModel clipCreation = await ServiceManager.Get<TwitchSessionService>().UserConnection.CreateClip(ServiceManager.Get<TwitchSessionService>().UserNewAPI, this.ClipIncludeDelay);
                    if (clipCreation != null)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            await Task.Delay(5000);

                            ClipModel clip = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClip(clipCreation);
                            if (clip != null && !string.IsNullOrEmpty(clip.url))
                            {
                                if (this.ShowInfoInChat)
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.ClipCreatedMessage, clip.url), parameters.Platform);
                                }
                                parameters.SpecialIdentifiers[ClipURLSpecialIdentifier] = clip.url;

                                GlobalEvents.TwitchClipCreated(clip);
                                return;
                            }
                        }
                    }
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ClipCreationFailed, parameters.Platform);
                }
                else if (this.ActionType == TwitchActionType.StreamMarker)
                {
                    string description = await this.ReplaceStringWithSpecialModifiers(this.StreamMarkerDescription, parameters);
                    if (!string.IsNullOrEmpty(description) && description.Length > TwitchActionModel.StreamMarkerMaxDescriptionLength)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamMarkerDescriptionMustBe140CharactersOrLess, parameters.Platform);
                        return;
                    }

                    CreatedStreamMarkerModel streamMarker = await ServiceManager.Get<TwitchSessionService>().UserConnection.CreateStreamMarker(ServiceManager.Get<TwitchSessionService>().UserNewAPI, description);
                    if (streamMarker != null)
                    {
                        if (this.ShowInfoInChat)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.StreamMarkerCreatedMessage, streamMarker.URL), parameters.Platform);
                        }
                        parameters.SpecialIdentifiers[StreamMarkerURLSpecialIdentifier] = streamMarker.URL;
                        return;
                    }
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamMarkerCreationFailed, parameters.Platform);
                }
            }
            else if (this.ActionType == TwitchActionType.UpdateChannelPointReward)
            {
                JObject jobj = new JObject()
                {
                    { "is_enabled", this.ChannelPointRewardState },
                };

                if (this.ChannelPointRewardCost > 0) { jobj["cost"] = this.ChannelPointRewardCost; }

                if (this.ChannelPointRewardUpdateCooldownsAndLimits)
                {
                    if (this.ChannelPointRewardMaxPerStream > 0)
                    {
                        jobj["max_per_stream_setting"] = new JObject()
                        {
                            { "is_enabled", true },
                            { "max_per_stream", this.ChannelPointRewardMaxPerStream }
                        };
                    }
                    else
                    {
                        jobj["max_per_stream_setting"] = new JObject()
                        {
                            { "is_enabled", false },
                        };
                    }

                    if (this.ChannelPointRewardMaxPerUser > 0)
                    {
                        jobj["max_per_user_per_stream_setting"] = new JObject()
                        {
                            { "is_enabled", true },
                            { "max_per_user_per_stream", this.ChannelPointRewardMaxPerUser }
                        };
                    }
                    else
                    {
                        jobj["max_per_user_per_stream_setting"] = new JObject()
                        {
                            { "is_enabled", false },
                        };
                    }

                    if (this.ChannelPointRewardGlobalCooldown > 0)
                    {
                        jobj["global_cooldown_setting"] = new JObject()
                        {
                            { "is_enabled", true },
                            { "global_cooldown_seconds", this.ChannelPointRewardGlobalCooldown * 60 }
                        };
                    }
                    else
                    {
                        jobj["global_cooldown_setting"] = new JObject()
                        {
                            { "is_enabled", false },
                        };
                    }
                }

                CustomChannelPointRewardModel reward = await ChannelSession.TwitchUserConnection.UpdateCustomChannelPointReward(ChannelSession.TwitchUserNewAPI, this.ChannelPointRewardID, jobj);
                if (reward == null)
                {
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.TwitchActionChannelPointRewardCouldNotBeUpdated);
                }
            }
        }
    }
}
