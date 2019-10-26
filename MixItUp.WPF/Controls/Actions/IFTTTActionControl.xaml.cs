using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    public class IFTTTEventValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public IFTTTEventValue() { }

        public IFTTTEventValue(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    /// <summary>
    /// Interaction logic for IFTTTActionControl.xaml
    /// </summary>
    public partial class IFTTTActionControl : ActionControlBase
    {
        private IFTTTAction action;

        public IFTTTActionControl() : base() { InitializeComponent(); }

        public IFTTTActionControl(IFTTTAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.IFTTT == null)
            {
                this.IFTTTNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
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
                return new IFTTTAction(this.EventNameTextBox.Text, this.EventValue1TextBox.Text, this.EventValue2TextBox.Text, this.EventValue3TextBox.Text);
            }
            return null;
        }
    }
}
