using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Commands;

namespace MixItUp.Base.ViewModel.MixPlay
{
    public class MixPlayControlViewModel
    {
        public MixPlayControlModel Control { get; set; }
        public MixPlayButtonControlModel Button { get; set; }
        public MixPlayJoystickControlModel Joystick { get; set; }
        public MixPlayTextBoxControlModel TextBox { get; set; }

        public MixPlayCommand Command { get; set; }

        public MixPlayControlViewModel(MixPlayGameModel game, MixPlayButtonControlModel button) : this(game, (MixPlayControlModel)button) { this.Button = button; }
        public MixPlayControlViewModel(MixPlayGameModel game, MixPlayJoystickControlModel joystick) : this(game, (MixPlayControlModel)joystick) { this.Joystick = joystick; }
        public MixPlayControlViewModel(MixPlayGameModel game, MixPlayTextBoxControlModel textBox) : this(game, (MixPlayControlModel)textBox) { this.TextBox = textBox; }

        private MixPlayControlViewModel(MixPlayGameModel game, MixPlayControlModel control)
        {
            this.Control = control;
            this.Command = ChannelSession.Services.MixPlay.GetInteractiveCommandForControl(game.id, this.Control.controlID);
        }

        public string Name { get { return this.Control.controlID; } }

        public string Type
        {
            get
            {
                if (this.Control is MixPlayButtonControlModel) { return "Button"; }
                if (this.Control is MixPlayJoystickControlModel) { return "Joystick"; }
                if (this.Control is MixPlayTextBoxControlModel) { return "Text Box"; }
                return "Unknown";
            }
        }

        public string SparkCost
        {
            get
            {
                if (this.Control is MixPlayButtonControlModel) { return this.Button.cost.ToString(); }
                if (this.Control is MixPlayTextBoxControlModel) { return this.TextBox.cost.ToString(); }
                return string.Empty;
            }
        }

        public string Cooldown
        {
            get
            {
                if (this.Command != null && this.Command.Requirements.Cooldown != null)
                {
                    if (this.Command.Requirements.Cooldown.IsGroup)
                    {
                        return this.Command.Requirements.Cooldown.GroupName;
                    }
                    return this.Command.Requirements.Cooldown.CooldownAmount.ToString();
                }
                return string.Empty;
            }
        }

        public string EventTypeString { get { return (this.Command != null) ? this.Command.EventTypeString : string.Empty; } }

        public bool IsNewCommandButton { get { return (this.Command == null); } }
        public bool IsExistingCommandButton { get { return !this.IsNewCommandButton; } }
    }
}
