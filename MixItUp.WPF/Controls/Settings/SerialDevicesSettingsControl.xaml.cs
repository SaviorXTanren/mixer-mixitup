using MixItUp.Base;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for SerialDevicesSettingsControl.xaml
    /// </summary>
    public partial class SerialDevicesSettingsControl : SettingsControlBase
    {
        private ObservableCollection<string> portNames = new ObservableCollection<string>();
        private ObservableCollection<SerialDeviceModel> serialDevices = new ObservableCollection<SerialDeviceModel>();

        public SerialDevicesSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.PortNameComboBox.ItemsSource = this.portNames;
            this.portNames.Clear();
            this.portNames.AddRange(await ServiceManager.Get<SerialService>().GetCurrentPortNames());

            this.SerialDevicesListView.ItemsSource = this.serialDevices;
            this.serialDevices.Clear();
            this.serialDevices.AddRange(ChannelSession.Settings.SerialDevices);

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private async void AddSerialDeviceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (!string.IsNullOrEmpty(this.PortNameComboBox.Text) && !string.IsNullOrEmpty(this.BaudRateTextBox.Text) && int.TryParse(this.BaudRateTextBox.Text, out int baudRate) && baudRate >= 0)
                {
                    if (ChannelSession.Settings.SerialDevices.Any(p => p.PortName.Equals(this.PortNameComboBox.Text)))
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.SerialDevicesAlreadyExistDeviceWithPortName);
                        return;
                    }

                    SerialDeviceModel serialDevice = new SerialDeviceModel(this.PortNameComboBox.Text, baudRate, this.DTREnabledCheckBox.IsChecked.GetValueOrDefault(), this.RTSEnabledCheckBox.IsChecked.GetValueOrDefault());
                    ChannelSession.Settings.SerialDevices.Add(serialDevice);
                    this.serialDevices.Add(serialDevice);

                    this.PortNameComboBox.Text = string.Empty;
                    this.BaudRateTextBox.Text = string.Empty;
                    this.DTREnabledCheckBox.IsChecked = false;
                    this.RTSEnabledCheckBox.IsChecked = false;
                }
            });
        }

        private void DeleteSerialDeviceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SerialDeviceModel serialDevice = (SerialDeviceModel)button.DataContext;
            ChannelSession.Settings.SerialDevices.Remove(serialDevice);
            this.serialDevices.Remove(serialDevice);
        }
    }
}
