using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using Mixer.Base.ViewModel;
using MixItUp.Base.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CooldownActionTypeEnum
    {
        Global,
        Individual
    }

    [DataContract]
    public class CooldownAction : ActionBase
    {
        [DataMember]
        public CooldownActionTypeEnum CooldownType { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public CooldownAction() { }

        public CooldownAction(CooldownActionTypeEnum cooldownType, int amount)
            : base(ActionTypeEnum.Cooldown)
        {
            this.CooldownType = cooldownType;
            this.Amount = amount;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (MixerAPIHandler.InteractiveClient != null && arguments.Count() == 1 && ChannelSession.ConnectedScene != null)
            {
                List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();
                if (this.CooldownType == CooldownActionTypeEnum.Global)
                {
                    buttons.AddRange(ChannelSession.ConnectedScene.buttons);
                }
                else if (this.CooldownType == CooldownActionTypeEnum.Individual)
                {
                    string controlID = arguments.ElementAt(0);
                    buttons.Add(ChannelSession.ConnectedScene.buttons.FirstOrDefault(b => b.controlID.Equals(controlID)));
                }

                long cooldownEnd = DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.Amount));
                foreach (InteractiveConnectedButtonControlModel button in buttons)
                {
                    button.cooldown = cooldownEnd;
                }

                await MixerAPIHandler.InteractiveClient.UpdateControls(ChannelSession.ConnectedScene, buttons);
            }
        }
    }
}
