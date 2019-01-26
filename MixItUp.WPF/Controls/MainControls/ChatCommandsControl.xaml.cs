using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : MainControlBase
    {
        private ObservableCollection<CommandGroupControlViewModel> customChatCommands = new ObservableCollection<CommandGroupControlViewModel>();

        private int nameOrder = 1;
        private int commandsOrder = 0;
        private int permissionsOrder = 0;
        private int cooldownOrder = 0;

        public ChatCommandsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            foreach (PreMadeChatCommand command in ChannelSession.PreMadeChatCommands.OrderBy(c => c.Name))
            {
                PreMadeChatCommandControl control = new PreMadeChatCommandControl();
                control.Margin = new Thickness(0, 5, 0, 5);
                control.Initialize(this.Window, command);
                this.PreMadeChatCommandsStackPanel.Children.Add(control);
            }

            this.CustomCommandsItemsControl.ItemsSource = this.customChatCommands;

            this.RefreshList();

            if (this.customChatCommands.Count > 0)
            {
                this.PreMadeCommandsButton.IsEnabled = true;
                this.CustomCommandsGrid.Visibility = Visibility.Visible;
                this.CommandNameFilterGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.CustomCommandsButton.IsEnabled = true;
                this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
                this.CommandNameFilterGrid.Visibility = Visibility.Collapsed;
            }

            return base.InitializeInternal();
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshList();
            return Task.FromResult(0);
        }

        private void RefreshList()
        {
            string filter = this.CommandNameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.customChatCommands.Clear();

            this.NameSortingIcon.Visibility = Visibility.Collapsed;
            if (nameOrder == 1)
            {
                this.NameSortingIcon.Visibility = Visibility.Visible;
                this.NameSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowDown;
            }
            else if (nameOrder == -1)
            {
                this.NameSortingIcon.Visibility = Visibility.Visible;
                this.NameSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowUp;
            }

            this.CommandsSortingIcon.Visibility = Visibility.Collapsed;
            if (commandsOrder == 1)
            {
                this.CommandsSortingIcon.Visibility = Visibility.Visible;
                this.CommandsSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowDown;
            }
            else if (commandsOrder == -1)
            {
                this.CommandsSortingIcon.Visibility = Visibility.Visible;
                this.CommandsSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowUp;
            }

            this.PermissionsSortingIcon.Visibility = Visibility.Collapsed;
            if (permissionsOrder == 1)
            {
                this.PermissionsSortingIcon.Visibility = Visibility.Visible;
                this.PermissionsSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowDown;
            }
            else if (permissionsOrder == -1)
            {
                this.PermissionsSortingIcon.Visibility = Visibility.Visible;
                this.PermissionsSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowUp;
            }

            this.CooldownSortingIcon.Visibility = Visibility.Collapsed;
            if (cooldownOrder == 1)
            {
                this.CooldownSortingIcon.Visibility = Visibility.Visible;
                this.CooldownSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowDown;
            }
            else if (cooldownOrder == -1)
            {
                this.CooldownSortingIcon.Visibility = Visibility.Visible;
                this.CooldownSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowUp;
            }

            IEnumerable<ChatCommand> commands = ChannelSession.Settings.ChatCommands.ToList();
            foreach (var group in commands.Where(c => string.IsNullOrEmpty(filter) || c.Name.ToLower().Contains(filter)).GroupBy(c => c.GroupName).OrderByDescending(g => !string.IsNullOrEmpty(g.Key)).ThenBy(g => g.Key))
            {
                IEnumerable<CommandBase> cmds = group;
                if (nameOrder != 0)
                {
                    cmds = (nameOrder > 0) ? group.OrderBy(c => c.Name) : group.OrderByDescending(c => c.Name);
                }
                else if (commandsOrder != 0)
                {
                    cmds = (commandsOrder > 0) ? group.OrderBy(c => c.CommandsString) : group.OrderByDescending(c => c.CommandsString);
                }
                else if (permissionsOrder != 0)
                {
                    cmds = (permissionsOrder > 0) ? group.OrderBy(c => c.Requirements.Role.RoleNameString) : group.OrderByDescending(c => c.Requirements.Role.RoleNameString);
                }
                else if (cooldownOrder != 0)
                {
                    cmds = (cooldownOrder > 0) ? group.OrderBy(c => c.Requirements.Cooldown.CooldownAmount) : group.OrderByDescending(c => c.Requirements.Cooldown.CooldownAmount);
                }
                CommandGroupSettings groupSettings = null;
                if (!string.IsNullOrEmpty(cmds.First().GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(cmds.First().GroupName))
                {
                    groupSettings = ChannelSession.Settings.CommandGroups[cmds.First().GroupName];
                }
                this.customChatCommands.Add(new CommandGroupControlViewModel(groupSettings, cmds));
            }
        }

        private void CommandNameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshList();
        }

        private void PreMadeCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = false;
            this.CustomCommandsButton.IsEnabled = true;
            this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
            this.CustomCommandsGrid.Visibility = Visibility.Collapsed;
            this.CommandNameFilterGrid.Visibility = Visibility.Collapsed;
        }

        private void CustomCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = true;
            this.CustomCommandsButton.IsEnabled = false;
            this.PreMadeCommandsGrid.Visibility = Visibility.Collapsed;
            this.CustomCommandsGrid.Visibility = Visibility.Visible;
            this.CommandNameFilterGrid.Visibility = Visibility.Visible;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.ChatCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void GroupCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RefreshList();
        }

        private void Name_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            commandsOrder = 0;
            permissionsOrder = 0;
            cooldownOrder = 0;
            if (nameOrder == 0)
            {
                nameOrder = 1;
            }
            else
            {
                nameOrder *= -1;
            }
            this.RefreshList();
        }

        private void Commands_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nameOrder = 0;
            permissionsOrder = 0;
            cooldownOrder = 0;
            if (commandsOrder == 0)
            {
                commandsOrder = 1;
            }
            else
            {
                commandsOrder *= -1;
            }
            this.RefreshList();
        }

        private void Permissions_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nameOrder = 0;
            commandsOrder = 0;
            cooldownOrder = 0;
            if (permissionsOrder == 0)
            {
                permissionsOrder = 1;
            }
            else
            {
                permissionsOrder *= -1;
            }
            this.RefreshList();
        }

        private void Cooldown_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nameOrder = 0;
            commandsOrder = 0;
            permissionsOrder = 0;
            if (cooldownOrder == 0)
            {
                cooldownOrder = 1;
            }
            else
            {
                cooldownOrder *= -1;
            }
            this.RefreshList();
        }

        private void AccordianGroupBoxControl_Minimized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = true;
                    }
                }
            }
        }

        private void AccordianGroupBoxControl_Maximized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = false;
                    }
                }
            }
        }
    }
}
