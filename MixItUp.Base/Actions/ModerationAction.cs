using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum ModerationActionTypeEnum
    {
        [Name("Chat Timeout")]
        ChatTimeout,
        [Name("Purge User")]
        PurgeUser,
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
    }

    [DataContract]
    public class ModerationAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ModerationAction.asyncSemaphore; } }

        [DataMember]
        public ModerationActionTypeEnum ModerationType { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string TimeAmount { get; set; }

        [DataMember]
        public string ModerationReason { get; set; }

        public ModerationAction() : base(ActionTypeEnum.Moderation) { }

        public ModerationAction(ModerationActionTypeEnum moderationType, string username, string timeAmount, string moderationReason)
            : this()
        {
            this.ModerationType = moderationType;
            this.UserName = username;
            this.TimeAmount = timeAmount;
            this.ModerationReason = moderationReason;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                UserViewModel targetUser = null;
                if (!string.IsNullOrEmpty(this.UserName))
                {
                    string username = await this.ReplaceStringWithSpecialModifiers(this.UserName, user, arguments);
                    UserModel targetUserModel = await ChannelSession.MixerStreamerConnection.GetUser(username);
                    if (targetUser == null)
                    {
                        targetUser = new UserViewModel(targetUserModel);
                    }
                }
                else
                {
                    targetUser = user;
                }

                if (this.ModerationType == ModerationActionTypeEnum.ClearChat)
                {
                    await ChannelSession.Chat.ClearMessages();
                }
                else if (targetUser != null)
                {
                    if (this.ModerationType == ModerationActionTypeEnum.PurgeUser)
                    {
                        await ChannelSession.Chat.PurgeUser(targetUser.UserName);
                    }
                    else if (this.ModerationType == ModerationActionTypeEnum.BanUser)
                    {
                        await ChannelSession.Chat.BanUser(targetUser);
                    }
                    else if (this.ModerationType == ModerationActionTypeEnum.UnbanUser)
                    {
                        await ChannelSession.Chat.UnBanUser(targetUser);
                    }
                    else if (this.ModerationType == ModerationActionTypeEnum.AddModerationStrike)
                    {
                        string moderationReason = "Manual Moderation Strike";
                        if (!string.IsNullOrEmpty(this.ModerationReason))
                        {
                            moderationReason = await this.ReplaceStringWithSpecialModifiers(this.ModerationReason, user, arguments);
                        }
                        await targetUser.AddModerationStrike(moderationReason);
                    }
                    else if (this.ModerationType == ModerationActionTypeEnum.RemoveModerationStrike)
                    {
                        await targetUser.RemoveModerationStrike();
                    }
                    else if (!string.IsNullOrEmpty(this.TimeAmount))
                    {
                        string timeAmountString = await this.ReplaceStringWithSpecialModifiers(this.TimeAmount, user, arguments);
                        if (uint.TryParse(timeAmountString, out uint timeAmount))
                        {
                            if (this.ModerationType == ModerationActionTypeEnum.ChatTimeout)
                            {
                                await ChannelSession.Chat.TimeoutUser(targetUser.UserName, timeAmount);
                            }
                            else if (this.ModerationType == ModerationActionTypeEnum.InteractiveTimeout)
                            {
                                if (targetUser != null && ChannelSession.Interactive != null)
                                {
                                    await ChannelSession.Interactive.TimeoutUser(targetUser, (int)timeAmount);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
