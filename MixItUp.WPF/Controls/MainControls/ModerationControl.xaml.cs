using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ModerationControl.xaml
    /// </summary>
    public partial class ModerationControl : MainControlBase
    {
        private static readonly List<string> ChatTextModerationSliderTypes = new List<string>() { "Percent", "Min" };
        private bool isLoaded = false;

        public ModerationControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.isLoaded = false;

            this.MaxCapsTypeComboBox.ItemsSource = ModerationControl.ChatTextModerationSliderTypes;
            this.MaxPunctuationSymbolsEmotesTypeComboBox.ItemsSource = ModerationControl.ChatTextModerationSliderTypes;

            this.FilteredWordsExemptComboBox.ItemsSource = UserRoles.All;
            this.ChatTextModerationExemptComboBox.ItemsSource = UserRoles.All;
            this.BlockLinksExemptComboBox.ItemsSource = UserRoles.All;
            this.ChatParticipationExemptComboBox.ItemsSource = UserRoles.All;

            this.CommunityBannedWordsToggleButton.IsChecked = ChannelSession.Settings.ModerationUseCommunityFilteredWords;
            this.FilteredWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.FilteredWords);
            this.BannedWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.BannedWords);
            this.FilteredWordsExemptComboBox.SelectedItem = ChannelSession.Settings.ModerationFilteredWordsExcemptUserRole;
            this.FilteredWordsApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationFilteredWordsApplyStrikes;

            this.MaxCapsSlider.Value = ChannelSession.Settings.ModerationCapsBlockCount;
            this.MaxCapsTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationCapsBlockIsPercentage ? 0 : 1;
            this.MaxPunctuationSymbolsEmotesSlider.Value = ChannelSession.Settings.ModerationPunctuationBlockCount;
            this.MaxPunctuationSymbolsEmotesTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationPunctuationBlockIsPercentage ? 0 : 1;
            this.ChatTextModerationExemptComboBox.SelectedItem = ChannelSession.Settings.ModerationChatTextExcemptUserRole;
            this.ChatTextApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationChatTextApplyStrikes;

            this.BlockLinksToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinks;
            this.BlockLinksExemptComboBox.SelectedItem = ChannelSession.Settings.ModerationBlockLinksExcemptUserRole;
            this.BlockLinksApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinksApplyStrikes;

            this.ChatInteractiveParticipationComboBox.ItemsSource = EnumHelper.GetEnumList<ModerationChatInteractiveParticipationEnum>();
            this.ChatInteractiveParticipationComboBox.SelectedItem = ChannelSession.Settings.ModerationChatInteractiveParticipation;
            this.ChatParticipationExemptComboBox.SelectedItem = ChannelSession.Settings.ModerationChatInteractiveParticipationExcemptUserRole;

            this.FollowEventModerationToggleButton.IsChecked = ChannelSession.Settings.ModerationFollowEvent;
            this.FollowEventModerationMaxQueueTextBox.Text = ChannelSession.Settings.ModerationFollowEventMaxInQueue.ToString();

            this.ResetStrikesOnLaunchToggleButton.IsChecked = ChannelSession.Settings.ModerationResetStrikesOnLaunch;
            this.Strike1Command.DataContext = ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike1CommandID);
            this.Strike2Command.DataContext = ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike2CommandID);
            this.Strike3Command.DataContext = ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike3CommandID);

            this.isLoaded = true;

            return base.InitializeInternal();
        }

        private async Task SaveSettings()
        {
            if (this.isLoaded)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    ChannelSession.Settings.ModerationUseCommunityFilteredWords = this.CommunityBannedWordsToggleButton.IsChecked.GetValueOrDefault();

                    this.ConvertFilteredTextToWordList(this.FilteredWordsTextBox.Text, ChannelSession.Settings.FilteredWords);
                    this.ConvertFilteredTextToWordList(this.BannedWordsTextBox.Text, ChannelSession.Settings.BannedWords);
                    ChannelSession.Settings.ModerationFilteredWordsExcemptUserRole = (UserRoleEnum)this.FilteredWordsExemptComboBox.SelectedItem;
                    ChannelSession.Settings.ModerationFilteredWordsApplyStrikes = this.FilteredWordsApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationCapsBlockCount = (int)this.MaxCapsSlider.Value;
                    ChannelSession.Settings.ModerationCapsBlockIsPercentage = (this.MaxCapsTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationPunctuationBlockCount = (int)this.MaxPunctuationSymbolsEmotesSlider.Value;
                    ChannelSession.Settings.ModerationPunctuationBlockIsPercentage = (this.MaxPunctuationSymbolsEmotesTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationChatTextExcemptUserRole = (UserRoleEnum)this.ChatTextModerationExemptComboBox.SelectedItem;
                    ChannelSession.Settings.ModerationChatTextApplyStrikes = this.ChatTextApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationBlockLinks = this.BlockLinksToggleButton.IsChecked.GetValueOrDefault();
                    ChannelSession.Settings.ModerationBlockLinksExcemptUserRole = (UserRoleEnum)this.BlockLinksExemptComboBox.SelectedItem;
                    ChannelSession.Settings.ModerationBlockLinksApplyStrikes = this.BlockLinksApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationChatInteractiveParticipation = (ModerationChatInteractiveParticipationEnum)this.ChatInteractiveParticipationComboBox.SelectedItem;
                    ChannelSession.Settings.ModerationChatInteractiveParticipationExcemptUserRole = (UserRoleEnum)this.ChatParticipationExemptComboBox.SelectedItem;

                    if (ChannelSession.Settings.ModerationFollowEvent != this.FollowEventModerationToggleButton.IsChecked.GetValueOrDefault())
                    {
                        EventCommandModel.FollowEventsInQueue = 0;
                    }
                    ChannelSession.Settings.ModerationFollowEvent = this.FollowEventModerationToggleButton.IsChecked.GetValueOrDefault();
                    int.TryParse(this.FollowEventModerationMaxQueueTextBox.Text, out int followEventModerationMaxQueue);
                    ChannelSession.Settings.ModerationFollowEventMaxInQueue = Math.Max(followEventModerationMaxQueue, 0);

                    ChannelSession.Settings.ModerationResetStrikesOnLaunch = this.ResetStrikesOnLaunchToggleButton.IsChecked.GetValueOrDefault();

                    await ChannelSession.SaveSettings();

                    ServiceManager.Get<ModerationService>().RebuildCache();
                });
            }
        }

        private async void TextBoxes_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.SaveSettings();
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await this.SaveSettings();
        }

        private async void Slider_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.SaveSettings();
        }

        private async void ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.SaveSettings();
        }

        private string ConvertFilteredWordListToText(IEnumerable<string> words)
        {
            string text = string.Join(Environment.NewLine, words);
            text = text.Replace(ModerationService.WordWildcardRegex, "*");
            return text;
        }

        private void ConvertFilteredTextToWordList(string text, List<string> list)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "";
            }
            text = text.Trim();
            text = text.Replace("*", ModerationService.WordWildcardRegex);

            list.Clear();
            foreach (string split in text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(split);
            }
        }

        private void Strike1Command_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) =>
            {
                this.Strike1Command.DataContext = null;
                this.Strike1Command.DataContext = command;
            };
            window.ForceShow();
        }

        private void Strike2Command_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) =>
            {
                this.Strike2Command.DataContext = null;
                this.Strike2Command.DataContext = command;
            };
            window.ForceShow();
        }

        private void Strike3Command_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) =>
            {
                this.Strike3Command.DataContext = null;
                this.Strike3Command.DataContext = command;
            };
            window.ForceShow();
        }
    }
}
