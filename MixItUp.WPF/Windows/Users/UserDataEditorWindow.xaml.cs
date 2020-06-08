using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Currency;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataEditorWindow.xaml
    /// </summary>
    public partial class UserDataEditorWindow : LoadingWindowBase
    {
        private UserViewModel user;
        private UserDataEditorWindowViewModel viewModel;

        public UserDataEditorWindow(UserDataModel userData)
        {
            this.user = new UserViewModel(userData);
            this.DataContext = this.viewModel = new UserDataEditorWindowViewModel(userData);

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.Closed += UserDataEditorWindow_Closed;
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.Load();

            this.CurrencyRankStackPanel.Children.Clear();
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.ToList())
            {
                this.CurrencyRankStackPanel.Children.Add(new UserCurrencyIndividualEditorControl(this.user.Data, currency));
            }

            this.InventoryStackPanel.Children.Clear();
            foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.ToList())
            {
                this.InventoryStackPanel.Children.Add(new UserInventoryEditorControl(this.user.Data, inventory));
            }
        }

        private void AddUserOnlyCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(autoAddToChatCommands: false));
            window.CommandSaveSuccessfully += NewUserOnlyCommandWindow_CommandSaveSuccessfully;
            window.Show();
        }

        private void NewUserOnlyCommandWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.AddUserOnlyChatCommand((ChatCommand)e);
        }

        private void UserOnlyChatCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command, autoAddToChatCommands: false));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void UserOnlyChatCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    this.viewModel.RemoveUserOnlyChatCommand(command);
                    await ChannelSession.SaveSettings();
                }
            });
        }

        private void ExistingEntranceCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private async void ExistingEntranceCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
                if (command != null)
                {
                    this.viewModel.EntranceCommand = null;
                    await ChannelSession.SaveSettings();
                }
            });
        }

        private void NewEntranceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(UserDataEditorWindowViewModel.UserEntranceCommandName)));
            window.CommandSaveSuccessfully += NewEntranceCommandWindow_CommandSaveSuccessfully;
            window.Show();
        }

        private void NewEntranceCommandWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.EntranceCommand = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshUserOnlyChatCommands();
        }

        private void UserDataEditorWindow_Closed(object sender, EventArgs e)
        {
            ChannelSession.Settings.UserData.ManualValueChanged(this.user.ID);
        }
    }
}
