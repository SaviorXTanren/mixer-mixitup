using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for PreMadeChatCommandControl.xaml
    /// </summary>
    public partial class PreMadeChatCommandControl : UserControl
    {
        private LoadingWindowBase window;
        private PreMadeChatCommand command;
        private PreMadeChatCommandSettings setting;

        public PreMadeChatCommandControl()
        {
            InitializeComponent();

            this.PermissionsComboBox.ItemsSource = ChatCommand.PermissionsAllowedValues;
        }

        public void Initialize(LoadingWindowBase window, PreMadeChatCommand command)
        {
            this.window = window;
            this.DataContext = this.command = command;
            this.PermissionsComboBox.SelectedItem = this.command.PermissionsString;

            this.setting = ChannelSession.Settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals(this.command.Name));
            if (this.setting == null)
            {
                this.setting = new PreMadeChatCommandSettings(this.command);
                ChannelSession.Settings.PreMadeChatCommandSettings.Add(this.setting);
            }
        }

        private void PermissionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            PreMadeChatCommand command = (PreMadeChatCommand)combobox.DataContext;

            command.Permissions = EnumHelper.GetEnumValueFromString<UserRole>((string)combobox.SelectedItem);

            this.UpdateSetting();
        }

        private void CooldownTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            PreMadeChatCommand command = (PreMadeChatCommand)textbox.DataContext;

            int cooldown = 0;
            if (!string.IsNullOrEmpty(textbox.Text) && int.TryParse(textbox.Text, out cooldown))
            {
                cooldown = Math.Max(cooldown, 0);
            }
            command.Cooldown = cooldown;
            textbox.Text = command.Cooldown.ToString();

            this.UpdateSetting();
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            await this.window.RunAsyncOperation(async () =>
            {
                await command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
            });
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = true;

            this.UpdateSetting();
        }

        private void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = false;

            this.UpdateSetting();
        }

        private void UpdateSetting()
        {
            if (this.setting != null)
            {
                this.setting.Permissions = command.Permissions;
                this.setting.Cooldown = command.Cooldown;
                this.setting.IsEnabled = command.IsEnabled;
            }
        }
    }
}
