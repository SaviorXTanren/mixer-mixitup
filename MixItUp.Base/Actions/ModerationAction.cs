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
        [Name("Interactive Timeout")]
        InteractiveTimeout,
        [Name("Ban User")]
        BanUser,
        [Name("Clear Chat")]
        ClearChat,
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

        public ModerationAction() : base(ActionTypeEnum.Moderation) { }

        public ModerationAction(ModerationActionTypeEnum moderationType, string username, string timeAmount)
            : this()
        {
            this.ModerationType = moderationType;
            this.UserName = username;
            this.TimeAmount = timeAmount;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                if (this.ModerationType == ModerationActionTypeEnum.ClearChat)
                {
                    await ChannelSession.Chat.ClearMessages();
                }
                else if (!string.IsNullOrEmpty(this.UserName))
                {
                    string username = await this.ReplaceStringWithSpecialModifiers(this.UserName, user, arguments);
                    UserModel targetUserModel = await ChannelSession.Connection.GetUser(username);
                    if (targetUserModel != null)
                    {
                        if (this.ModerationType == ModerationActionTypeEnum.PurgeUser)
                        {
                            await ChannelSession.Chat.PurgeUser(targetUserModel.username);
                        }
                        else if (this.ModerationType == ModerationActionTypeEnum.BanUser)
                        {
                            await ChannelSession.Chat.BanUser(new UserViewModel(targetUserModel));
                        }
                        else if (!string.IsNullOrEmpty(this.TimeAmount))
                        {
                            string timeAmountString = await this.ReplaceStringWithSpecialModifiers(this.TimeAmount, user, arguments);
                            if (uint.TryParse(timeAmountString, out uint timeAmount))
                            {
                                if (this.ModerationType == ModerationActionTypeEnum.ChatTimeout)
                                {
                                    await ChannelSession.Chat.TimeoutUser(targetUserModel.username, timeAmount);
                                }
                                else if (this.ModerationType == ModerationActionTypeEnum.InteractiveTimeout)
                                {
                                    UserViewModel targetUser = await ChannelSession.ActiveUsers.GetUserByID(targetUserModel.id);
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
}
