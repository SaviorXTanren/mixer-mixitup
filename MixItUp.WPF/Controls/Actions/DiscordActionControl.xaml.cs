using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for DiscordActionControl.xaml
    /// </summary>
    public partial class DiscordActionControl : ActionControlBase
    {
        private ObservableCollection<DiscordChannel> channels = new ObservableCollection<DiscordChannel>();

        private DiscordAction action;

        public DiscordActionControl() : base() { InitializeComponent(); }

        public DiscordActionControl(DiscordAction action) : this() { this.action = action; }

        public override async Task OnLoaded()
        {
            this.channels.Clear();
            this.SendMessageChannelComboBox.ItemsSource = this.channels;

            if (ChannelSession.Services.Discord == null)
            {
                this.DiscordNotEnabledWarningTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                foreach (DiscordChannel channel in await ChannelSession.Services.Discord.GetServerChannels(ChannelSession.Services.Discord.Server))
                {
                    if (channel.Type == DiscordChannel.DiscordChannelTypeEnum.Text)
                    {
                        this.channels.Add(channel);
                    }
                }
            }

            this.MuteDeafenOptionCheckBox.IsChecked = true;

            this.DiscordActionTypeComboBox.ItemsSource = Enum.GetValues(typeof(DiscordActionTypeEnum));
            if (this.action != null)
            {
                this.DiscordActionTypeComboBox.SelectedItem = action.DiscordType;

                this.SendMessageChannelComboBox.SelectedItem = action.SendMessageChannel;
                this.SendMessageTextBox.Text = action.SendMessageText;
                this.FilePath.Text = action.FilePath;

                this.MuteDeafenOptionCheckBox.IsChecked = action.ShouldMuteDeafen;
            }
        }

        public override ActionBase GetAction()
        {
            if (this.DiscordActionTypeComboBox.SelectedIndex >= 0)
            {
                DiscordActionTypeEnum actionType = (DiscordActionTypeEnum)this.DiscordActionTypeComboBox.SelectedItem;
                if (actionType == DiscordActionTypeEnum.SendMessage)
                {
                    if (this.SendMessageChannelComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.SendMessageTextBox.Text))
                    {
                        DiscordChannel channel = (DiscordChannel)this.SendMessageChannelComboBox.SelectedItem;
                        return DiscordAction.CreateForChatMessage(channel, this.SendMessageTextBox.Text, this.FilePath.Text);
                    }
                }
                else if (actionType == DiscordActionTypeEnum.MuteSelf)
                {
                    return DiscordAction.CreateForMuteSelf(this.MuteDeafenOptionCheckBox.IsChecked.GetValueOrDefault());
                }
                else if (actionType == DiscordActionTypeEnum.DeafenSelf)
                {
                    return DiscordAction.CreateForDeafenSelf(this.MuteDeafenOptionCheckBox.IsChecked.GetValueOrDefault());
                }
            }
            return null;
        }

        private void DiscordActionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SendMessageGrid.Visibility = Visibility.Collapsed;
            this.MuteDeafenOptionGrid.Visibility = Visibility.Collapsed;
            if (this.DiscordActionTypeComboBox.SelectedIndex >= 0)
            {
                DiscordActionTypeEnum actionType = (DiscordActionTypeEnum)this.DiscordActionTypeComboBox.SelectedItem;
                if (actionType == DiscordActionTypeEnum.SendMessage)
                {
                    this.SendMessageGrid.Visibility = Visibility.Visible;
                }
                else if (actionType == DiscordActionTypeEnum.MuteSelf)
                {
                    this.MuteDeafenOptionGrid.Visibility = Visibility.Visible;
                    this.MuteDeafenOptionTextBlock.Text = MixItUp.Base.Resources.Mute;
                }
                else if (actionType == DiscordActionTypeEnum.DeafenSelf)
                {
                    this.MuteDeafenOptionGrid.Visibility = Visibility.Visible;
                    this.MuteDeafenOptionTextBlock.Text = MixItUp.Base.Resources.Deafen;
                }
            }
        }

        private void FilePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.FilePath.Text = filePath;
            }
        }
    }
}
