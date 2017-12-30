using Mixer.Base.Util;
using MixItUp.Base;
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

        private IEnumerable<InputTypeEnum> inputs;

        public InputActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InputActionControl(ActionContainerControl containerControl, InputAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.inputs = action.Inputs;
                this.SetRecordKeysText();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.inputs != null && this.inputs.Count() > 0)
            {
                return new InputAction(this.inputs);
            }
            return null;
        }

        private async void RecordKeysButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.containerControl.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.InitializeInputService();

                object buttonText = this.RecordKeysButton.Content;
                
                for (int i = 20; i > 0; i--)
                {
                    this.RecordKeysButton.Content = (i / 5).ToString();

                    this.inputs = await ChannelSession.Services.InputService.GetCurrentInputs();
                    this.SetRecordKeysText();

                    await Task.Delay(250);
                }

                this.RecordKeysButton.Content = buttonText;
            });
        }

        private void SetRecordKeysText()
        {
            this.RecordedKeysTextBlock.Text = string.Join(", ", EnumHelper.GetEnumNames(this.inputs));
        }
    }
}
