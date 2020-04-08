using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamingPlatformActionControl.xaml
    /// </summary>
    public partial class StreamingPlatformActionControl : ActionControlBase
    {
        private StreamingPlatformAction action;

        public StreamingPlatformActionControl() : base() { InitializeComponent(); }

        public StreamingPlatformActionControl(StreamingPlatformAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.ActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingPlatformActionType>().OrderBy(s => s);
            this.PollCommandTypeComboBox.ItemsSource = EnumHelper.GetEnumNames(ChannelSession.AllCommands.Select(c => c.Type).Distinct().OrderBy(s => s));
            if (this.action != null)
            {
                this.ActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ActionType);
                
                if (this.action.ActionType == StreamingPlatformActionType.Host)
                {
                    this.HostChannelNameTextBox.Text = this.action.HostChannelName;
                }
                else if (this.action.ActionType == StreamingPlatformActionType.Poll)
                {
                    this.PollQuestionTextBox.Text = this.action.PollQuestion;
                    this.PollLengthTextBox.Text = this.action.PollLength.ToString();
                    if (this.action.PollAnswers.Count > 0) { this.PollAnswer1TextBox.Text = this.action.PollAnswers[0]; }
                    if (this.action.PollAnswers.Count > 1) { this.PollAnswer2TextBox.Text = this.action.PollAnswers[1]; }
                    if (this.action.PollAnswers.Count > 2) { this.PollAnswer3TextBox.Text = this.action.PollAnswers[2]; }
                    if (this.action.PollAnswers.Count > 3) { this.PollAnswer4TextBox.Text = this.action.PollAnswers[3]; }

                    CommandBase command = this.action.Command;
                    if (command != null)
                    {
                        this.PollCommandTypeComboBox.SelectedItem = EnumHelper.GetEnumName(command.Type);
                        this.PollCommandNameComboBox.SelectedItem = command;
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.ActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamingPlatformActionType actionType = EnumHelper.GetEnumValueFromString<StreamingPlatformActionType>((string)this.ActionTypeComboBox.SelectedItem);
                if (actionType == StreamingPlatformActionType.Host)
                {
                    if (!string.IsNullOrEmpty(this.HostChannelNameTextBox.Text))
                    {
                        return StreamingPlatformAction.CreateHostAction(this.HostChannelNameTextBox.Text);
                    }
                }
                else if (actionType == StreamingPlatformActionType.Poll)
                {
                    if (!string.IsNullOrEmpty(this.PollQuestionTextBox.Text) && !string.IsNullOrEmpty(this.PollLengthTextBox.Text) &&
                        uint.TryParse(this.PollLengthTextBox.Text, out uint length) && length > 0)
                    {
                        List<string> answers = new List<string>();
                        answers.Add(this.PollAnswer1TextBox.Text);
                        answers.Add(this.PollAnswer2TextBox.Text);
                        answers.Add(this.PollAnswer3TextBox.Text);
                        answers.Add(this.PollAnswer4TextBox.Text);

                        IEnumerable<string> validAnswers = answers.Where(a => !string.IsNullOrEmpty(a));
                        if (validAnswers.Count() >= 2)
                        {
                            CommandBase command = null;
                            if (this.PollCommandNameComboBox.SelectedIndex >= 0)
                            {
                                command = (CommandBase)this.PollCommandNameComboBox.SelectedItem;
                            }

                            return StreamingPlatformAction.CreatePollAction(this.PollQuestionTextBox.Text, length, validAnswers, command);
                        }
                    }
                }
                else if (actionType == StreamingPlatformActionType.RunAd)
                {
                    return StreamingPlatformAction.CreateRunAdAction();
                }
            }
            return null;
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ActionTypeComboBox.SelectedIndex >= 0)
            {
                this.HostGrid.Visibility = Visibility.Collapsed;
                this.PollGrid.Visibility = Visibility.Collapsed;

                StreamingPlatformActionType actionType = EnumHelper.GetEnumValueFromString<StreamingPlatformActionType>((string)this.ActionTypeComboBox.SelectedItem);
                if (actionType == StreamingPlatformActionType.Host)
                {
                    this.HostGrid.Visibility = Visibility.Visible;
                }
                else if (actionType == StreamingPlatformActionType.Poll)
                {
                    this.PollGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void PollCommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PollCommandTypeComboBox.SelectedIndex >= 0)
            {
                string typeString = (string)this.PollCommandTypeComboBox.SelectedItem;
                CommandTypeEnum type = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(typeString);
                IEnumerable<CommandBase> commands = ChannelSession.AllCommands.Where(c => c.Type == type).OrderBy(c => c.Name);
                if (type == CommandTypeEnum.Chat)
                {
                    commands = commands.Where(c => !(c is PreMadeChatCommand));
                }
                this.PollCommandNameComboBox.ItemsSource = commands;
            }
        }
    }
}
