using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    public class HotKeyUI
    {
        public HotKeyConfiguration HotKey { get; set; }

        public CommandBase Command { get; set; }

        public HotKeyUI(HotKeyConfiguration hotKey, CommandBase command)
        {
            this.HotKey = hotKey;
            this.Command = command;
        }
    }

    /// <summary>
    /// Interaction logic for HotKeysSettingsControl.xaml
    /// </summary>
    public partial class HotKeysSettingsControl : SettingsControlBase
    {
        private const string PreMadeCommandType = "Pre-Made";

        private ObservableCollection<HotKeyUI> hotKeys = new ObservableCollection<HotKeyUI>();

        public HotKeysSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.HotKeysDataGrid.ItemsSource = this.hotKeys;

            this.RefreshList();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            this.CommandTypeComboBox.ItemsSource = EnumHelper.GetEnumNames(ChannelSession.AllCommands.Select(c => c.Type).Distinct()).OrderBy(s => s);
            this.KeyComboBox.ItemsSource = EnumHelper.GetEnumNames<InputKeyEnum>().OrderBy(s => s);

            this.RefreshList();

            await this.InitializeInternal();
        }

        private void CommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandTypeComboBox.SelectedIndex >= 0)
            {
                string typeString = (string)this.CommandTypeComboBox.SelectedItem;
                CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeString);
                if (typeString.Equals(PreMadeCommandType))
                {
                    type = CommandTypeEnum.Chat;
                }

                IEnumerable<CommandBase> commands = ChannelSession.AllCommands.Where(c => c.Type == type).OrderBy(c => c.Name);
                if (type == CommandTypeEnum.Chat)
                {
                    if (typeString.Equals(PreMadeCommandType))
                    {
                        commands = commands.Where(c => c is PreMadeChatCommand);
                    }
                    else
                    {
                        commands = commands.Where(c => !(c is PreMadeChatCommand));
                    }
                }
                this.CommandNameComboBox.ItemsSource = commands;
            }
            else
            {
                this.CommandNameComboBox.ItemsSource = null;
            }
        }

        private async void AddHotKeyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.CommandNameComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A command must be selected");
                }

                CommandBase command = (CommandBase)this.CommandNameComboBox.SelectedItem;

                if (this.KeyComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A hot key configuration must be set");
                }

                HotKeyModifiersEnum modifiers = HotKeyModifiersEnum.None;
                if (this.ShiftCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Shift; }
                if (this.ControlCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Control; }
                if (this.AltCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Alt; }
                HotKeyConfiguration hotKey = new HotKeyConfiguration(modifiers, EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.KeyComboBox.SelectedItem), command.ID);

                ChannelSession.Settings.HotKeys[hotKey.ToString()] = hotKey;

                ChannelSession.Services.InputService.RegisterHotKey(hotKey.Modifiers, hotKey.Key);

                this.RefreshList();
            });
        }

        private void DeleteHotKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            HotKeyUI hotKey = (HotKeyUI)button.DataContext;
            if (hotKey != null)
            {
                ChannelSession.Settings.HotKeys.Remove(hotKey.ToString());
                ChannelSession.Services.InputService.UnregisterHotKey(hotKey.HotKey.Modifiers, hotKey.HotKey.Key);
            }
            this.RefreshList();
        }

        private void RefreshList()
        {
            this.CommandTypeComboBox.SelectedIndex = -1;
            this.ShiftCheckBox.IsChecked = false;
            this.ControlCheckBox.IsChecked = false;
            this.AltCheckBox.IsChecked = false;
            this.KeyComboBox.SelectedIndex = -1;

            this.hotKeys.Clear();
            foreach (HotKeyConfiguration hotKey in ChannelSession.Settings.HotKeys.Values.OrderBy(hotKey => hotKey.Key))
            {
                CommandBase command = ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(hotKey.CommandID));
                if (command != null)
                {
                    this.hotKeys.Add(new HotKeyUI(hotKey, command));
                }
            }
        }
    }
}
