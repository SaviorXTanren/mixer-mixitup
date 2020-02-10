using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CounterActionControl.xaml
    /// </summary>
    public partial class CounterActionControl : ActionControlBase
    {
        private CounterAction action;

        public CounterActionControl() : base() { InitializeComponent(); }

        public CounterActionControl(CounterAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.CounterActionTypeComboBox.ItemsSource = new List<string>() { "Update", "Set", "Reset" };

            if (this.action != null)
            {
                this.CounterNameTextBox.Text = this.action.CounterName;
                if (ChannelSession.Settings.Counters.ContainsKey(this.action.CounterName))
                {
                    this.SaveToFileToggleButton.IsChecked = ChannelSession.Settings.Counters[this.action.CounterName].SaveToFile;
                    this.ResetOnLoadToggleButton.IsChecked = ChannelSession.Settings.Counters[this.action.CounterName].ResetOnLoad;
                }

                if (this.action.UpdateAmount)
                {
                    this.CounterActionTypeComboBox.SelectedIndex = 0;
                    this.CounterAmountTextBox.Text = this.action.Amount;
                }
                else if (this.action.SetAmount)
                {
                    this.CounterActionTypeComboBox.SelectedIndex = 1;
                    this.CounterAmountTextBox.Text = this.action.Amount;
                }
                else if (this.action.ResetAmount)
                {
                    this.CounterActionTypeComboBox.SelectedIndex = 2;
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.CounterNameTextBox.Text))
            {
                if (!ChannelSession.Settings.Counters.ContainsKey(this.CounterNameTextBox.Text))
                {
                    ChannelSession.Settings.Counters[this.CounterNameTextBox.Text] = new CounterModel(this.CounterNameTextBox.Text);
                }
                ChannelSession.Settings.Counters[this.CounterNameTextBox.Text].SaveToFile = this.SaveToFileToggleButton.IsChecked.GetValueOrDefault();
                ChannelSession.Settings.Counters[this.CounterNameTextBox.Text].ResetOnLoad = this.ResetOnLoadToggleButton.IsChecked.GetValueOrDefault();

                if (this.CounterActionTypeComboBox.SelectedIndex == 0)
                {
                    return new CounterAction(this.CounterNameTextBox.Text, this.CounterAmountTextBox.Text, false);
                }
                else if (this.CounterActionTypeComboBox.SelectedIndex == 1)
                {
                    return new CounterAction(this.CounterNameTextBox.Text, this.CounterAmountTextBox.Text, true);
                }
                else if (this.CounterActionTypeComboBox.SelectedIndex == 2)
                {
                    return new CounterAction(this.CounterNameTextBox.Text);
                }
            }
            return null;
        }

        private void SaveToFileToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ResetOnLoadToggleButton.IsEnabled = this.SaveToFileToggleButton.IsChecked.GetValueOrDefault();
            if (!this.ResetOnLoadToggleButton.IsEnabled)
            {
                this.ResetOnLoadToggleButton.IsChecked = true;
            }
        }

        private void CounterActionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CounterActionTypeComboBox.SelectedIndex == 0)
            {
                this.CounterAmountTextBox.IsEnabled = true;
            }
            else if (this.CounterActionTypeComboBox.SelectedIndex == 1)
            {
                this.CounterAmountTextBox.IsEnabled = true;
            }
            else if (this.CounterActionTypeComboBox.SelectedIndex == 2)
            {
                this.CounterAmountTextBox.IsEnabled = false;
                this.CounterAmountTextBox.Clear();
            }
        }

        private async void CountersFolderHyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string counterFolderPath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), CounterModel.CounterFolderName);
            if (!Directory.Exists(counterFolderPath))
            {
                await ChannelSession.Services.FileService.CreateDirectory(counterFolderPath);
            }
            ProcessHelper.LaunchFolder(counterFolderPath);
        }
    }
}
