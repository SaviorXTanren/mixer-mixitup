using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum TrovoActionType
    {
        Host,
        EnableSlowMode,
        DisableSlowMode,
        EnableFollowerMode,
        DisableFollowerMode,
        AddUserRole,
        RemoveUserRole,
        [Obsolete]
        FastClip90Seconds,
        SetTitle,
        SetGame,
        EnableSubscriberMode,
        DisableSubscriberMode,
    }

    [DataContract]
    public class TrovoActionModel : ActionModelBase
    {
        public static TrovoActionModel CreateHostAction(string username)
        {
            TrovoActionModel action = new TrovoActionModel(TrovoActionType.Host);
            action.Username = username;
            return action;
        }

        public static TrovoActionModel CreateTextAction(TrovoActionType actionType, string text)
        {
            TrovoActionModel action = new TrovoActionModel(actionType);
            action.Text = text;
            return action;
        }

        public static TrovoActionModel CreateEnableSlowModeAction(int amount)
        {
            TrovoActionModel action = new TrovoActionModel(TrovoActionType.EnableSlowMode);
            action.Amount = amount;
            return action;
        }

        public static TrovoActionModel CreateUserRoleAction(TrovoActionType actionType, string username, string roleName)
        {
            TrovoActionModel action = new TrovoActionModel(actionType);
            action.Username = username;
            action.RoleName = roleName;
            return action;
        }

        public static TrovoActionModel CreateBasicAction(TrovoActionType actionType)
        {
            return new TrovoActionModel(actionType);
        }

        [DataMember]
        public TrovoActionType ActionType { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string RoleName { get; set; }

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        private TrovoActionModel(TrovoActionType type)
            : base(ActionTypeEnum.Trovo)
        {
            this.ActionType = type;
        }

        [Obsolete]
        public TrovoActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TrovoSessionService>().IsConnected && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                string username = null;
                if (!string.IsNullOrEmpty(this.Username))
                {
                    username = await ReplaceStringWithSpecialModifiers(this.Username, parameters);
                }
                else
                {
                    username = parameters.User?.Username;
                }

                string text = await ReplaceStringWithSpecialModifiers(this.Text, parameters);
                string roleName = await ReplaceStringWithSpecialModifiers(this.RoleName, parameters);

                if (this.ActionType == TrovoActionType.Host)
                {
                    await ServiceManager.Get<TrovoChatEventService>().HostUser(username);
                }
                else if (this.ActionType == TrovoActionType.EnableSlowMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().SlowMode(this.Amount);
                }
                else if (this.ActionType == TrovoActionType.DisableSlowMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().SlowMode(0);
                }
                else if (this.ActionType == TrovoActionType.EnableFollowerMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().FollowersMode(enable: true);
                }
                else if (this.ActionType == TrovoActionType.DisableFollowerMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().FollowersMode(enable: false);
                }
                else if (this.ActionType == TrovoActionType.AddUserRole)
                {
                    await ServiceManager.Get<TrovoChatEventService>().AddRole(username, roleName);
                }
                else if (this.ActionType == TrovoActionType.RemoveUserRole)
                {
                    await ServiceManager.Get<TrovoChatEventService>().RemoveRole(username, roleName);
                }
                else if (this.ActionType == TrovoActionType.FastClip90Seconds)
                {
                    await ServiceManager.Get<TrovoChatEventService>().FastClip();
                }
                else if (this.ActionType == TrovoActionType.SetTitle)
                {
                    await ServiceManager.Get<TrovoSessionService>().SetTitle(text);
                }
                else if (this.ActionType == TrovoActionType.SetGame)
                {
                    await ServiceManager.Get<TrovoSessionService>().SetGame(text);
                }
                else if (this.ActionType == TrovoActionType.EnableSubscriberMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().SubscriberMode(enable: true);
                }
                else if (this.ActionType == TrovoActionType.DisableSubscriberMode)
                {
                    await ServiceManager.Get<TrovoChatEventService>().SubscriberMode(enable: false);
                }
            }
        }
    }
}