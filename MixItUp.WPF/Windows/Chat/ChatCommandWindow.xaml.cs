using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Chat
{
    /// <summary>
    /// Interaction logic for ChatCommandWindow.xaml
    /// </summary>
    public partial class ChatCommandWindow : LoadingWindowBase
    {
        private ChatCommand command;

        private ObservableCollection<ActionControl> actionControls;

        private List<ActionTypeEnum> allowedActions = new List<ActionTypeEnum>()
        {
            ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Giveaway,
            ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
        };

        public ChatCommandWindow() : this(null) { }

        public ChatCommandWindow(ChatCommand command)
        {
            this.command = command;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.ActionsListView.ItemsSource = this.actionControls;

            List<string> roles = EnumHelper.GetEnumNames<UserRole>().ToList();
            roles.Remove(EnumHelper.GetEnumName<UserRole>(UserRole.Banned));
            this.LowestRoleAllowedComboBox.ItemsSource = roles;
            this.LowestRoleAllowedComboBox.SelectedIndex = 0;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.ChatCommandTextBox.Text = this.command.CommandsString;
                this.LowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.LowestAllowedRole);

                foreach (ActionBase action in this.command.Actions)
                {
                    this.actionControls.Add(new ActionControl(allowedActions, action));
                }
            }

            return base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.actionControls.Add(new ActionControl(allowedActions));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                MessageBoxHelper.ShowError("Required command information is missing");
                return;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                MessageBoxHelper.ShowError("Required chat command information is missing");
                return;
            }

            if (this.actionControls.Count == 0)
            {
                MessageBoxHelper.ShowError("At least one action must be created");
                return;
            }

            List<ActionBase> newActions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowError("Required action information is missing");
                    return;
                }
                newActions.Add(action);
            }

            List<string> commands = new List<string>(this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            UserRole lowestRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.LowestRoleAllowedComboBox.SelectedItem);
            if (this.command == null)
            {
                
                this.command = new ChatCommand(this.NameTextBox.Text, commands, lowestRole);
                ChannelSession.Settings.ChatCommands.Add(this.command);
            }
            else
            {
                this.command.Name = this.NameTextBox.Text;
                this.command.Commands = commands;
                this.command.LowestAllowedRole = lowestRole;
            }

            this.command.Actions.Clear();
            this.command.Actions = newActions;

            this.Close();
        }
    }
}
