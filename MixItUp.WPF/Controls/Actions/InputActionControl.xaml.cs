using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for InputActionControl.xaml
    /// </summary>
    public partial class InputActionControl : ActionControlBase
    {
        private InputAction action;

        public InputActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InputActionControl(ActionContainerControl containerControl, InputAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.InputButtonComboBox.ItemsSource = EnumHelper.GetEnumNames<InputTypeEnum>();
            if (this.action != null)
            {
                this.InputButtonComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Inputs.First());
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.InputButtonComboBox.SelectedIndex >= 0)
            {
                return new InputAction(new List<InputTypeEnum>() { EnumHelper.GetEnumValueFromString<InputTypeEnum>((string)this.InputButtonComboBox.SelectedItem) });
            }
            return null;
        }
    }
}
