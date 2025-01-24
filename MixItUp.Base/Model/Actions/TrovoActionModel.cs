using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo.New;
using System;
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

        private TrovoActionModel(TrovoActionType type)
            : base(ActionTypeEnum.Trovo)
        {
            this.ActionType = type;
        }

        [Obsolete]
        public TrovoActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TrovoSession>().IsConnected)
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
                    await ServiceManager.Get<TrovoSession>().StreamerService.HostUser(ServiceManager.Get<TrovoSession>().ChannelID, username);
                }
                else if (this.ActionType == TrovoActionType.EnableSlowMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.SlowMode(ServiceManager.Get<TrovoSession>().ChannelID, this.Amount);
                }
                else if (this.ActionType == TrovoActionType.DisableSlowMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.SlowMode(ServiceManager.Get<TrovoSession>().ChannelID, 0);
                }
                else if (this.ActionType == TrovoActionType.EnableFollowerMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.FollowersMode(ServiceManager.Get<TrovoSession>().ChannelID, enable: true);
                }
                else if (this.ActionType == TrovoActionType.DisableFollowerMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.FollowersMode(ServiceManager.Get<TrovoSession>().ChannelID, enable: false);
                }
                else if (this.ActionType == TrovoActionType.AddUserRole)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.AddRole(ServiceManager.Get<TrovoSession>().ChannelID, username, roleName);
                }
                else if (this.ActionType == TrovoActionType.RemoveUserRole)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.RemoveRole(ServiceManager.Get<TrovoSession>().ChannelID, username, roleName);
                }
                else if (this.ActionType == TrovoActionType.FastClip90Seconds)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.FastClip(ServiceManager.Get<TrovoSession>().ChannelID);
                }
                else if (this.ActionType == TrovoActionType.SetTitle)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.UpdateChannel(ServiceManager.Get<TrovoSession>().ChannelID, title: text);
                }
                else if (this.ActionType == TrovoActionType.SetGame)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.SetGame(ServiceManager.Get<TrovoSession>().ChannelID, text);
                }
                else if (this.ActionType == TrovoActionType.EnableSubscriberMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.SubscriberMode(ServiceManager.Get<TrovoSession>().ChannelID, enable: true);
                }
                else if (this.ActionType == TrovoActionType.DisableSubscriberMode)
                {
                    await ServiceManager.Get<TrovoSession>().StreamerService.SubscriberMode(ServiceManager.Get<TrovoSession>().ChannelID, enable: false);
                }
            }
        }
    }
}