using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
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
        private static readonly List<string> ChatTextModerationSliderTypes = new List<string>() { "%", "Min" };
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

            this.FilteredWordsExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.ChatTextModerationExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.BlockLinksExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.ChatInteractiveParticipationExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;

            this.CommunityBannedWordsToggleButton.IsChecked = ChannelSession.Settings.ModerationUseCommunityFilteredWords;
            this.FilteredWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.FilteredWords);
            this.BannedWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.BannedWords);
            this.FilteredWordsExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationFilteredWordsExcempt);
            this.FilteredWordsApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationFilteredWordsApplyStrikes;

            this.MaxCapsSlider.Value = ChannelSession.Settings.ModerationCapsBlockCount;
            this.MaxCapsTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationCapsBlockIsPercentage ? 0 : 1;
            this.MaxPunctuationSymbolsEmotesSlider.Value = ChannelSession.Settings.ModerationPunctuationBlockCount;
            this.MaxPunctuationSymbolsEmotesTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationPunctuationBlockIsPercentage ? 0 : 1;
            this.ChatTextModerationExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationChatTextExcempt);
            this.ChatTextApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationChatTextApplyStrikes;

            this.BlockLinksToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinks;
            this.BlockLinksExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationBlockLinksExcempt);
            this.BlockLinksApplyStrikesToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinksApplyStrikes;

            this.ChatInteractiveParticipationComboBox.ItemsSource = EnumHelper.GetEnumNames<ModerationChatInteractiveParticipationEnum>();
            this.ChatInteractiveParticipationComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationChatInteractiveParticipation);
            this.ChatInteractiveParticipationExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt);

            this.ResetStrikesOnLaunchToggleButton.IsChecked = ChannelSession.Settings.ModerationResetStrikesOnLaunch;
            this.Strike1Command.DataContext = ChannelSession.Settings.ModerationStrike1Command;
            this.Strike2Command.DataContext = ChannelSession.Settings.ModerationStrike2Command;
            this.Strike3Command.DataContext = ChannelSession.Settings.ModerationStrike3Command;

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
                    ChannelSession.Settings.ModerationFilteredWordsExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.FilteredWordsExemptComboBox.SelectedItem);
                    ChannelSession.Settings.ModerationFilteredWordsApplyStrikes = this.FilteredWordsApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationCapsBlockCount = (int)this.MaxCapsSlider.Value;
                    ChannelSession.Settings.ModerationCapsBlockIsPercentage = (this.MaxCapsTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationPunctuationBlockCount = (int)this.MaxPunctuationSymbolsEmotesSlider.Value;
                    ChannelSession.Settings.ModerationPunctuationBlockIsPercentage = (this.MaxPunctuationSymbolsEmotesTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationChatTextExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.ChatTextModerationExemptComboBox.SelectedItem);
                    ChannelSession.Settings.ModerationChatTextApplyStrikes = this.ChatTextApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationBlockLinks = this.BlockLinksToggleButton.IsChecked.GetValueOrDefault();
                    ChannelSession.Settings.ModerationBlockLinksExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.BlockLinksExemptComboBox.SelectedItem);
                    ChannelSession.Settings.ModerationBlockLinksApplyStrikes = this.BlockLinksApplyStrikesToggleButton.IsChecked.GetValueOrDefault();

                    ChannelSession.Settings.ModerationChatInteractiveParticipation = EnumHelper.GetEnumValueFromString<ModerationChatInteractiveParticipationEnum>((string)this.ChatInteractiveParticipationComboBox.SelectedItem);
                    ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.ChatInteractiveParticipationExemptComboBox.SelectedItem);

                    ChannelSession.Settings.ModerationResetStrikesOnLaunch = this.ResetStrikesOnLaunchToggleButton.IsChecked.GetValueOrDefault();

                    await ChannelSession.SaveSettings();
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
            text = text.Replace(ModerationHelper.BannedWordWildcardRegexFormat, "*");
            return text;
        }

        private void ConvertFilteredTextToWordList(string text, LockedList<string> list)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "";
            }
            text = text.Replace("*", ModerationHelper.BannedWordWildcardRegexFormat);

            list.Clear();
            foreach (string split in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(split);
            }
        }

        private void StrikeCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }
    }
}
