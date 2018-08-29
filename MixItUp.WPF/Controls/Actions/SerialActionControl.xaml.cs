using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Model.Serial;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SerialActionControl.xaml
    /// </summary>
    public partial class SerialActionControl : ActionControlBase
    {
        private SerialAction action;

        private ObservableCollection<string> serialDevices = new ObservableCollection<string>();

        public SerialActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public SerialActionControl(ActionContainerControl containerControl, SerialAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.SerialDeviceComboBox.ItemsSource = serialDevices;
            this.serialDevices.Clear();
            foreach (SerialDeviceModel serialDevice in ChannelSession.Settings.SerialDevices)
            {
                this.serialDevices.Add(serialDevice.PortName);
            }

            if (this.action != null)
            {
                this.SerialDeviceComboBox.SelectedItem = this.action.PortName;
                this.MessageTextBox.Text = this.action.Message;
            }

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.SerialDeviceComboBox.Text) && !string.IsNullOrEmpty(this.MessageTextBox.Text))
            {
                return new SerialAction(this.SerialDeviceComboBox.Text, this.MessageTextBox.Text);
            }
            return null;
        }
    }
}
