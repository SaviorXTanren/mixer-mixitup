using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ModerationActionTypeEnum
    {
        TimeoutUser,
        PurgeUser,
        ClearChat,
        BanUser,
        UnbanUser,
        ModUser,
        UnmodUser,
        AddModerationStrike,
        RemoveModerationStrike,
    }

    [DataContract]
    public class ModerationActionModel : ActionModelBase
    {
        [DataMember]
        public ModerationActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TargetUsername { get; set; }

        [DataMember]
        public string TimeoutAmount { get; set; }

        [DataMember]
        public string ModerationReason { get; set; }

        public ModerationActionModel(ModerationActionTypeEnum actionType, string targetUsername = null, string timeoutAmount = null, string moderationReason = null)
            : base(ActionTypeEnum.Moderation)
        {
            this.ActionType = actionType;
            this.TargetUsername = targetUsername;
            this.TimeoutAmount = timeoutAmount;
            this.ModerationReason = moderationReason;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal ModerationActionModel(MixItUp.Base.Actions.ModerationAction action)
            : base(ActionTypeEnum.Moderation)
        {
            if (action.ModerationType != Base.Actions.ModerationActionTypeEnum.VIPUser && action.ModerationType != Base.Actions.ModerationActionTypeEnum.UnVIPUser)
            {
                switch (action.ModerationType)
                {
                    case Base.Actions.ModerationActionTypeEnum.ChatTimeout: this.ActionType = ModerationActionTypeEnum.TimeoutUser; break;
                    case Base.Actions.ModerationActionTypeEnum.PurgeUser: this.ActionType = ModerationActionTypeEnum.PurgeUser; break;
                    case Base.Actions.ModerationActionTypeEnum.ClearChat: this.ActionType = ModerationActionTypeEnum.ClearChat; break;
                    case Base.Actions.ModerationActionTypeEnum.BanUser: this.ActionType = ModerationActionTypeEnum.BanUser; break;
                    case Base.Actions.ModerationActionTypeEnum.UnbanUser: this.ActionType = ModerationActionTypeEnum.UnbanUser; break;
                    case Base.Actions.ModerationActionTypeEnum.ModUser: this.ActionType = ModerationActionTypeEnum.ModUser; break;
                    case Base.Actions.ModerationActionTypeEnum.UnmodUser: this.ActionType = ModerationActionTypeEnum.UnmodUser; break;
                    case Base.Actions.ModerationActionTypeEnum.AddModerationStrike: this.ActionType = ModerationActionTypeEnum.AddModerationStrike; break;
                    case Base.Actions.ModerationActionTypeEnum.RemoveModerationStrike: this.ActionType = ModerationActionTypeEnum.RemoveModerationStrike; break;
                }
                this.TargetUsername = action.UserName;
                this.TimeoutAmount = action.TimeAmount;
                this.ModerationReason = action.ModerationReason;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private ModerationActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == ModerationActionTypeEnum.ClearChat)
            {
                await ChannelSession.Services.Chat.ClearMessages();
            }
            else
            {
                UserViewModel targetUser = null;
                if (!string.IsNullOrEmpty(this.TargetUsername))
                {
                    string username = await ReplaceStringWithSpecialModifiers(this.TargetUsername, parameters);
                    targetUser = ChannelSession.Services.User.GetActiveUserByUsername(username, parameters.Platform);
                }
                else
                {
                    targetUser = parameters.User;
                }

                if (targetUser != null)
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
                            moderationReason = await ReplaceStringWithSpecialModifiers(this.ModerationReason, parameters);
                        }
                        await targetUser.AddModerationStrike(moderationReason);
                    }
                    else if (this.ActionType == ModerationActionTypeEnum.RemoveModerationStrike)
                    {
                        await targetUser.RemoveModerationStrike();
                    }
                    else if (this.ActionType == ModerationActionTypeEnum.TimeoutUser)
                    {
                        if (!string.IsNullOrEmpty(this.TimeoutAmount))
                        {
                            string timeAmountString = await ReplaceStringWithSpecialModifiers(this.TimeoutAmount, parameters);
                            if (uint.TryParse(timeAmountString, out uint timeAmount))
                            {
                                await ChannelSession.Services.Chat.TimeoutUser(targetUser, timeAmount);
                            }
                        }
                    }
                }
            }
        }
    }
}
