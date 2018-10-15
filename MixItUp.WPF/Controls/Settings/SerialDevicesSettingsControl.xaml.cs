using MixItUp.Base;
using MixItUp.Base.Model.Serial;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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
            foreach (string portName in await ChannelSession.Services.SerialService.GetCurrentPortNames())
            {
                this.portNames.Add(portName);
            }

            this.SerialDevicesListView.ItemsSource = this.serialDevices;
            this.serialDevices.Clear();
            foreach (SerialDeviceModel serialDevice in ChannelSession.Settings.SerialDevices)
            {
                this.serialDevices.Add(serialDevice);
            }

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private async void AddSerialDeviceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                if (!string.IsNullOrEmpty(this.PortNameComboBox.Text) && !string.IsNullOrEmpty(this.BaudRateTextBox.Text) && int.TryParse(this.BaudRateTextBox.Text, out int baudRate) && baudRate >= 0)
                {
                    if (!ChannelSession.Settings.SerialDevices.Any(p => p.PortName.Equals(this.PortNameComboBox.Text)))
                    {
                        SerialDeviceModel serialDevice = new SerialDeviceModel(this.PortNameComboBox.Text, baudRate, this.DTREnabledCheckBox.IsChecked.GetValueOrDefault(), this.RTSEnabledCheckBox.IsChecked.GetValueOrDefault());
                        ChannelSession.Settings.SerialDevices.Add(serialDevice);
                        this.serialDevices.Add(serialDevice);
                    }
                }
                this.PortNameComboBox.Text = string.Empty;
                this.BaudRateTextBox.Text = string.Empty;
                this.DTREnabledCheckBox.IsChecked = false;
                this.RTSEnabledCheckBox.IsChecked = false;

                return Task.FromResult(0);
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
