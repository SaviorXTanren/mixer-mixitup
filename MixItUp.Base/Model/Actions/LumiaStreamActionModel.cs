using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum LumiaStreamActionTypeEnum
    {
        TriggerCommand,
        SetLightsColor,
    }

    public enum LumiaStreamActionCommandTypeEnum
    {
        ChatCommand,
        TwitchPoint,
        TwitchExtension,
        TrovoSpell,
    }

    [DataContract]
    public class LumiaStreamActionModel : ActionModelBase
    {
        [DataMember]
        public LumiaStreamActionTypeEnum ActionType { get; set; }

        [DataMember]
        public LumiaStreamActionCommandTypeEnum CommandType { get; set; }
        [DataMember]
        public string CommandName { get; set; }

        [DataMember]
        public string ColorHex { get; set; }
        [DataMember]
        public int ColorBrightness { get; set; }
        [DataMember]
        public double ColorTransition { get; set; }
        [DataMember]
        public double ColorDuration { get; set; }
        [DataMember]
        public bool ColorHold { get; set; }

        public LumiaStreamActionModel(LumiaStreamActionTypeEnum actionType, LumiaStreamActionCommandTypeEnum commandType, string commandName)
            : base(ActionTypeEnum.LumiaStream)
        {
            this.ActionType = actionType;
            this.CommandType = commandType;
            this.CommandName = commandName;
        }

        public LumiaStreamActionModel(LumiaStreamActionTypeEnum actionType, string colorHex, int brightness, double transtion, double duration, bool colorHold)
            : base(ActionTypeEnum.LumiaStream)
        {
            this.ActionType = actionType;
            this.ColorHex = colorHex;
            this.ColorBrightness = brightness;
            this.ColorTransition = transtion;
            this.ColorDuration = duration;
            this.ColorHold = colorHold;
        }

        [Obsolete]
        private LumiaStreamActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<LumiaStreamService>().IsEnabled && !ServiceManager.Get<LumiaStreamService>().IsConnected)
            {
                await ServiceManager.Get<LumiaStreamService>().Connect();
            }

            if (ServiceManager.Get<LumiaStreamService>().IsConnected)
            {
                if (this.ActionType == LumiaStreamActionTypeEnum.TriggerCommand)
                {
                    string commandType = null;
                    switch (this.CommandType)
                    {
                        case LumiaStreamActionCommandTypeEnum.ChatCommand:
                            commandType = "chat-command";
                            break;
                        case LumiaStreamActionCommandTypeEnum.TwitchPoint:
                            commandType = "twitch-points";
                            break;
                        case LumiaStreamActionCommandTypeEnum.TwitchExtension:
                            commandType = "twitch-extension";
                            break;
                        case LumiaStreamActionCommandTypeEnum.TrovoSpell:
                            commandType = "trovo-spells";
                            break;
                    }

                    if (!string.IsNullOrEmpty(commandType))
                    {
                        await ServiceManager.Get<LumiaStreamService>().TriggerCommand(commandType, this.CommandName);
                    }
                }
                else if (this.ActionType == LumiaStreamActionTypeEnum.SetLightsColor)
                {
                    await ServiceManager.Get<LumiaStreamService>().SetColorAndBrightness(this.ColorHex, this.ColorBrightness, (int)(this.ColorTransition * 1000), (int)(this.ColorDuration * 1000), this.ColorHold);
                }
            }
        }
    }
}
