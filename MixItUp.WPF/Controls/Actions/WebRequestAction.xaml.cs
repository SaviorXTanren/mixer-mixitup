using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for WebRequestAction.xaml
    /// </summary>
    public partial class WebRequestActionControl : ActionControlBase
    {
        private WebRequestAction action;

        public WebRequestActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public WebRequestActionControl(ActionContainerControl containerControl, WebRequestAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.ResponseActionComboBox.ItemsSource = EnumHelper.GetEnumNames<WebRequestResponseActionTypeEnum>();
            
            List<string> commandNames = ChannelSession.Settings.ChatCommands.Select(c => c.Name).ToList();
            CommandBase command = this.containerControl.EditorControl.GetExistingCommand();
            if (command != null && command is ChatCommand)
            {
                commandNames.Remove(command.Name);
            }
            this.CommandResponseComboBox.ItemsSource = commandNames;

            if (this.action != null)
            {
                this.WebRequestURLTextBox.Text = this.action.Url;
                this.ResponseActionComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ResponseAction);
                if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.Chat)
                {
                    this.ChatResponseTextBox.Text = this.action.ResponseChatText;
                }
                else if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.Command)
                {
                    this.CommandResponseComboBox.SelectedItem = this.action.ResponseCommandName;
                    this.CommandResponseArgumentsTextBox.Text = this.action.ResponseCommandArgumentsText;
                }
                else if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.SpecialIdentifier)
                {
                    this.SpecialIdentifierResponseTextBox.Text = this.action.SpecialIdentifierName;
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.WebRequestURLTextBox.Text))
            {
                WebRequestResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<WebRequestResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);
                if (responseType == WebRequestResponseActionTypeEnum.Chat)
                {
                    if (!string.IsNullOrEmpty(this.ChatResponseTextBox.Text))
                    {
                        return new WebRequestAction(this.WebRequestURLTextBox.Text, this.ChatResponseTextBox.Text);
                    }
                }
                else if (responseType == WebRequestResponseActionTypeEnum.Command)
                {
                    if (this.CommandResponseComboBox.SelectedIndex >= 0)
                    {
                        return new WebRequestAction(this.WebRequestURLTextBox.Text, (string)this.CommandResponseComboBox.SelectedItem, this.CommandResponseArgumentsTextBox.Text);
                    }
                }
                else if (responseType == WebRequestResponseActionTypeEnum.SpecialIdentifier)
                {
                    if (!string.IsNullOrEmpty(this.SpecialIdentifierResponseTextBox.Text) && this.SpecialIdentifierResponseTextBox.Text.All(c => Char.IsLetterOrDigit(c)))
                    {
                        return new WebRequestAction(this.WebRequestURLTextBox.Text, WebRequestResponseActionTypeEnum.SpecialIdentifier) { SpecialIdentifierName = this.SpecialIdentifierResponseTextBox.Text };
                    }
                }
                else
                {
                    return new WebRequestAction(this.WebRequestURLTextBox.Text);
                }
            }
            return null;
        }

        private void ResponseActionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ResponseActionComboBox.SelectedIndex >= 0)
            {
                WebRequestResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<WebRequestResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);

                this.ChatResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.Chat) ? Visibility.Visible : Visibility.Collapsed;
                this.CommandResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.Command) ? Visibility.Visible : Visibility.Collapsed;
                this.SpecialIdentifierResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.SpecialIdentifier) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
