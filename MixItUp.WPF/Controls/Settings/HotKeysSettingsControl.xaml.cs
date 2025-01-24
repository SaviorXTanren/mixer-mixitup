using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    public class HotKeyUI : IEquatable<HotKeyUI>, IComparable, IComparable<HotKeyUI>
    {
        public HotKeyConfiguration HotKey { get; set; }

        public CommandModelBase Command { get; set; }

        public string CommandName { get { return this.Command?.Name ?? Resources.Unknown; } }

        public HotKeyUI(HotKeyConfiguration hotKey, CommandModelBase command)
        {
            this.HotKey = hotKey;
            this.Command = command;
        }

        public int CompareTo(object obj)
        {
            if (obj is HotKeyUI)
            {
                return this.CompareTo((HotKeyUI)obj);
            }
            return -1;
        }

        public int CompareTo(HotKeyUI other) { return this.HotKey.VirtualKey.CompareTo(other.HotKey.VirtualKey); }

        public override bool Equals(object obj)
        {
            if (obj is HotKeyUI)
            {
                return this.Equals((HotKeyUI)obj);
            }
            return false;
        }

        public bool Equals(HotKeyUI other) { return this.HotKey.Equals(other.HotKey); }

        public override int GetHashCode() { return this.HotKey.GetHashCode(); }
    }

    /// <summary>
    /// Interaction logic for HotKeysSettingsControl.xaml
    /// </summary>
    public partial class HotKeysSettingsControl : SettingsControlBase
    {
        private static IEnumerable<VirtualKeyEnum> keyboardKeys = EnumHelper.GetEnumList<VirtualKeyEnum>().OrderBy(k => EnumLocalizationHelper.GetLocalizedName(k));

        private const string PreMadeCommandType = "Pre-Made";

        private ObservableCollection<HotKeyUI> hotKeys = new ObservableCollection<HotKeyUI>();

        public HotKeysSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.KeyComboBox.ItemsSource = keyboardKeys;
            this.HotKeysDataGrid.ItemsSource = this.hotKeys;

            this.RefreshList();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            this.CommandTypeComboBox.ItemsSource = EnumHelper.GetEnumNames(CommandModelBase.GetSelectableCommandTypes());

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

                IEnumerable<CommandModelBase> commands = ServiceManager.Get<CommandService>().AllCommands.Where(c => c.Type == type).OrderBy(c => c.Name);
                if (type == CommandTypeEnum.Chat)
                {
                    if (typeString.Equals(PreMadeCommandType))
                    {
                        commands = commands.Where(c => c is PreMadeChatCommandModelBase);
                    }
                    else
                    {
                        commands = commands.Where(c => !(c is PreMadeChatCommandModelBase));
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
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommandRequired);
                    return;
                }

                CommandModelBase command = (CommandModelBase)this.CommandNameComboBox.SelectedItem;

                if (this.KeyComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.HotKeyRequired);
                    return;
                }

                HotKeyModifiersEnum modifiers = HotKeyModifiersEnum.None;
                if (this.ShiftCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Shift; }
                if (this.ControlCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Control; }
                if (this.AltCheckBox.IsChecked.GetValueOrDefault()) { modifiers |= HotKeyModifiersEnum.Alt; }
                HotKeyConfiguration hotKey = new HotKeyConfiguration(modifiers, (VirtualKeyEnum)this.KeyComboBox.SelectedItem, command.ID);

                ChannelSession.Settings.HotKeys[hotKey.ToString()] = hotKey;

                ServiceManager.Get<IInputService>().RegisterHotKey(hotKey.Modifiers, hotKey.VirtualKey);

                this.RefreshList();
            });
        }

        private void DeleteHotKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            HotKeyUI hotKey = (HotKeyUI)button.DataContext;
            if (hotKey != null)
            {
                ChannelSession.Settings.HotKeys.Remove(hotKey.HotKey.ToString());
                ServiceManager.Get<IInputService>().UnregisterHotKey(hotKey.HotKey.Modifiers, hotKey.HotKey.VirtualKey);
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
            foreach (HotKeyConfiguration hotKey in ChannelSession.Settings.HotKeys.Values.OrderBy(hotKey => EnumLocalizationHelper.GetLocalizedName(hotKey.VirtualKey)))
            {
                this.hotKeys.Add(new HotKeyUI(hotKey, ChannelSession.Settings.GetCommand(hotKey.CommandID)));
            }
        }
    }
}
