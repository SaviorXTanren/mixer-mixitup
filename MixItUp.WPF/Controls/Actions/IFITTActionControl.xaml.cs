using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    public class IFITTEventValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public IFITTEventValue() { }

        public IFITTEventValue(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    /// <summary>
    /// Interaction logic for IFITTActionControl.xaml
    /// </summary>
    public partial class IFITTActionControl : ActionControlBase
    {
        private IFITTAction action;

        public IFITTActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public IFITTActionControl(ActionContainerControl containerControl, IFITTAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.IFITT == null)
            {
                this.IFITTNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
            }

            if (this.action != null)
            {
                this.EventNameTextBox.Text = this.action.EventName;
                this.EventValue1TextBox.Text = this.action.EventValue1;
                this.EventValue2TextBox.Text = this.action.EventValue2;
                this.EventValue3TextBox.Text = this.action.EventValue3;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.EventNameTextBox.Text))
            {
                return new IFITTAction(this.EventNameTextBox.Text, this.EventValue1TextBox.Text, this.EventValue2TextBox.Text, this.EventValue3TextBox.Text);
            }
            return null;
        }
    }
}
