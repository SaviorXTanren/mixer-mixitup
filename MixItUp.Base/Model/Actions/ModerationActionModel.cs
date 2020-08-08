using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ModerationActionTypeEnum
    {
        [Name("Chat Timeout")]
        ChatTimeout,
        [Name("Purge User")]
        PurgeUser,
        [Obsolete]
        [Name("MixPlay Timeout")]
        InteractiveTimeout,
        [Name("Ban User")]
        BanUser,
        [Name("Clear Chat")]
        ClearChat,
        [Name("Add Moderation Strike")]
        AddModerationStrike,
        [Name("Remove Moderation Strike")]
        RemoveModerationStrike,
        [Name("Unban User")]
        UnbanUser,
        [Name("Mod User")]
        ModUser,
        [Name("Unmod User")]
        UnmodUser
    }

    [DataContract]
    public class ModerationActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ModerationActionModel.asyncSemaphore; } }

        [DataMember]
        public ModerationActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string TimeAmount { get; set; }

        [DataMember]
        public string ModerationReason { get; set; }

        public ModerationActionModel(ModerationActionTypeEnum actionType, string username, string timeAmount, string moderationReason)
            : base(ActionTypeEnum.Moderation)
        {
            this.ActionType = actionType;
            this.UserName = username;
            this.TimeAmount = timeAmount;
            this.ModerationReason = moderationReason;
        }

        internal ModerationActionModel(MixItUp.Base.Actions.ModerationAction action)
            : base(ActionTypeEnum.Moderation)
        {
            if (action.ModerationType != Base.Actions.ModerationActionTypeEnum.VIPUser && action.ModerationType != Base.Actions.ModerationActionTypeEnum.UnVIPUser)
            {
                this.ActionType = (ModerationActionTypeEnum)(int)action.ModerationType;
                this.UserName = action.UserName;
                this.TimeAmount = action.TimeAmount;
                this.ModerationReason = action.ModerationReason;
            }
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            UserViewModel targetUser = null;
            if (!string.IsNullOrEmpty(this.UserName))
            {
                string username = await this.ReplaceStringWithSpecialModifiers(this.UserName, user, platform, arguments, specialIdentifiers);
                targetUser = ChannelSession.Services.User.GetUserByUsername(username, platform);
            }
            else
            {
                targetUser = user;
            }

            if (this.ActionType == ModerationActionTypeEnum.ClearChat)
            {
                await ChannelSession.Services.Chat.ClearMessages();
            }
            else if (targetUser != null)
            {
                if (this.ActionType == ModerationActionTypeEnum.PurgeUser)
                {
                    await ChannelSession.Services.Chat.PurgeUser(targetUser);
                }
                else if (this.ActionType == ModerationActionTypeEnum.BanUser)
                {
                    await ChannelSession.Services.Chat.BanUser(targetUser);
                }
                else if (this.ActionType == ModerationActionTypeEnum.UnbanUser)
                {
                    await ChannelSession.Services.Chat.UnbanUser(targetUser);
                }
                else if (this.ActionType == ModerationActionTypeEnum.ModUser)
                {
                    await ChannelSession.Services.Chat.ModUser(targetUser);
                }
                else if (this.ActionType == ModerationActionTypeEnum.UnmodUser)
                {
                    await ChannelSession.Services.Chat.UnmodUser(targetUser);
                }
                else if (this.ActionType == ModerationActionTypeEnum.AddModerationStrike)
                {
                    string moderationReason = "Manual Moderation Strike";
                    if (!string.IsNullOrEmpty(this.ModerationReason))
                    {
                        moderationReason = await this.ReplaceStringWithSpecialModifiers(this.ModerationReason, user, platform, arguments, specialIdentifiers);
                    }
                    await targetUser.AddModerationStrike(moderationReason);
                }
                else if (this.ActionType == ModerationActionTypeEnum.RemoveModerationStrike)
                {
                    await targetUser.RemoveModerationStrike();
                }
                else if (!string.IsNullOrEmpty(this.TimeAmount))
                {
                    string timeAmountString = await this.ReplaceStringWithSpecialModifiers(this.TimeAmount, user, platform, arguments, specialIdentifiers);
                    if (uint.TryParse(timeAmountString, out uint timeAmount))
                    {
                        if (this.ActionType == ModerationActionTypeEnum.ChatTimeout)
                        {
                            await ChannelSession.Services.Chat.TimeoutUser(targetUser, timeAmount);
                        }
                    }
                }
            }
        }
    }
}
