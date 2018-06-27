using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for TranslationActionControl.xaml
    /// </summary>
    public partial class TranslationActionControl : ActionControlBase
    {
        private TranslationAction action;

        public TranslationActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public TranslationActionControl(ActionContainerControl containerControl, TranslationAction action) : this(containerControl) { this.action = action; }

        public override async Task OnLoaded()
        {
            this.TranslationLanguageComboBox.ItemsSource = await ChannelSession.Services.TranslationService.GetAvailableLanguages();
            this.ResponseActionComboBox.ItemsSource = EnumHelper.GetEnumNames<TranslationResponseActionTypeEnum>();
            this.CommandResponseComboBox.ItemsSource = ChannelSession.Settings.ChatCommands.OrderBy(c => c.Name);

            if (this.action != null)
            {
                this.TranslationLanguageComboBox.SelectedItem = this.action.Culture;
                this.AllowProfanityCheckBox.IsChecked = this.action.AllowProfanity;
                this.TranslationTextTextBox.Text = this.action.Text;

                this.ResponseActionComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ResponseAction);
                if (this.action.ResponseAction == TranslationResponseActionTypeEnum.Chat)
                {
                    this.ChatResponseTextBox.Text = this.action.ResponseChatText;
                }
                else if (this.action.ResponseAction == TranslationResponseActionTypeEnum.Command)
                {
                    this.CommandResponseComboBox.SelectedItem = ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.action.ResponseCommandID));
                    this.CommandResponseArgumentsTextBox.Text = this.action.ResponseCommandArgumentsText;
                }
                else if (this.action.ResponseAction == TranslationResponseActionTypeEnum.SpecialIdentifier)
                {
                    this.SpecialIdentifierResponseTextBox.Text = this.action.SpecialIdentifierName;
                }
            }
        }

        public override ActionBase GetAction()
        {
            if (this.TranslationLanguageComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.TranslationTextTextBox.Text))
            {
                CultureInfo culture = (CultureInfo)this.TranslationLanguageComboBox.SelectedItem;
                TranslationResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<TranslationResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);
                if (responseType == TranslationResponseActionTypeEnum.Chat)
                {
                    if (!string.IsNullOrEmpty(this.ChatResponseTextBox.Text))
                    {
                        return TranslationAction.CreateForChat(culture, this.TranslationTextTextBox.Text, this.AllowProfanityCheckBox.IsChecked.GetValueOrDefault(), this.ChatResponseTextBox.Text);
                    }
                }
                else if (responseType == TranslationResponseActionTypeEnum.Command)
                {
                    if (this.CommandResponseComboBox.SelectedIndex >= 0)
                    {
                        return TranslationAction.CreateForCommand(culture, this.TranslationTextTextBox.Text, this.AllowProfanityCheckBox.IsChecked.GetValueOrDefault(),
                            (CommandBase)this.CommandResponseComboBox.SelectedItem, this.CommandResponseArgumentsTextBox.Text);
                    }
                }
                else if (responseType == TranslationResponseActionTypeEnum.SpecialIdentifier)
                {
                    if (SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.SpecialIdentifierResponseTextBox.Text))
                    {
                        return TranslationAction.CreateForSpecialIdentifier(culture, this.TranslationTextTextBox.Text, this.AllowProfanityCheckBox.IsChecked.GetValueOrDefault(),
                            this.SpecialIdentifierResponseTextBox.Text);
                    }
                }
            }
            return null;
        }

        private void ResponseActionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ResponseActionComboBox.SelectedIndex >= 0)
            {
                TranslationResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<TranslationResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);

                this.ChatResponseActionGrid.Visibility = (responseType == TranslationResponseActionTypeEnum.Chat) ? Visibility.Visible : Visibility.Collapsed;
                this.CommandResponseActionGrid.Visibility = (responseType == TranslationResponseActionTypeEnum.Command) ? Visibility.Visible : Visibility.Collapsed;
                this.SpecialIdentifierResponseActionGrid.Visibility = (responseType == TranslationResponseActionTypeEnum.SpecialIdentifier) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
