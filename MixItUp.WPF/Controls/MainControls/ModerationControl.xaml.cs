using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
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
            this.MaxPunctuationSymbolsTypeComboBox.ItemsSource = ModerationControl.ChatTextModerationSliderTypes;
            this.MaxEmotesTypeComboBox.ItemsSource = ModerationControl.ChatTextModerationSliderTypes;

            this.FilteredWordsExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.ChatTextModerationExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.BlockLinksExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.ModerationTimeoutExemptComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;

            this.CommunityBannedWordsToggleButton.IsChecked = ChannelSession.Settings.ModerationUseCommunityFilteredWords;

            this.FilteredWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.FilteredWords);
            this.BannedWordsTextBox.Text = this.ConvertFilteredWordListToText(ChannelSession.Settings.BannedWords);
            this.FilteredWordsExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationFilteredWordsExcempt);

            this.MaxCapsSlider.Value = ChannelSession.Settings.ModerationCapsBlockCount;
            this.MaxCapsTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationCapsBlockIsPercentage ? 0 : 1;
            this.MaxPunctuationSymbolsSlider.Value = ChannelSession.Settings.ModerationPunctuationBlockCount;
            this.MaxPunctuationSymbolsTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationPunctuationBlockIsPercentage ? 0 : 1;
            this.MaxEmotesSlider.Value = ChannelSession.Settings.ModerationEmoteBlockCount;
            this.MaxEmotesTypeComboBox.SelectedIndex = ChannelSession.Settings.ModerationEmoteBlockIsPercentage ? 0 : 1;
            this.ChatTextModerationExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationChatTextExcempt);

            this.BlockLinksToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinks;
            this.BlockLinksExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationBlockLinksExcempt);

            this.ModerationTimeout1MinAfterSlider.Value = ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount;
            this.ModerationTimeout5MinAfterSlider.Value = ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount;
            this.ModerationTimeoutExemptComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.ModerationChatTextExcempt);

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

                    ChannelSession.Settings.ModerationCapsBlockCount = (int)this.MaxCapsSlider.Value;
                    ChannelSession.Settings.ModerationCapsBlockIsPercentage = (this.MaxCapsTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationPunctuationBlockCount = (int)this.MaxPunctuationSymbolsSlider.Value;
                    ChannelSession.Settings.ModerationPunctuationBlockIsPercentage = (this.MaxPunctuationSymbolsTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationEmoteBlockCount = (int)this.MaxEmotesSlider.Value;
                    ChannelSession.Settings.ModerationEmoteBlockIsPercentage = (this.MaxEmotesTypeComboBox.SelectedIndex == 0);
                    ChannelSession.Settings.ModerationChatTextExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.ChatTextModerationExemptComboBox.SelectedItem);

                    ChannelSession.Settings.ModerationBlockLinks = this.BlockLinksToggleButton.IsChecked.GetValueOrDefault();
                    ChannelSession.Settings.ModerationBlockLinksExcempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.BlockLinksExemptComboBox.SelectedItem);

                    ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount = (int)this.ModerationTimeout1MinAfterSlider.Value;
                    ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount = (int)this.ModerationTimeout5MinAfterSlider.Value;
                    ChannelSession.Settings.ModerationTimeoutExempt = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.ModerationTimeoutExemptComboBox.SelectedItem);

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
    }
}
