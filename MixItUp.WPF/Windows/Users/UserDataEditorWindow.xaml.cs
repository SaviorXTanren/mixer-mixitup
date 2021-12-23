using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataEditorWindow.xaml
    /// </summary>
    public partial class UserDataEditorWindow : LoadingWindowBase
    {
        private UserV2ViewModel user;
        private UserDataEditorWindowViewModel viewModel;

        public UserDataEditorWindow(UserV2Model userData)
        {
            this.user = new UserV2ViewModel(userData);
            this.DataContext = this.viewModel = new UserDataEditorWindowViewModel(userData);

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.Closed += UserDataEditorWindow_Closed;
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.Load();
        }

        private void AddUserOnlyCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.UserOnlyChat, this.viewModel.User.ID);
            window.CommandSaved += (object s, CommandModelBase command) =>
            {
                this.viewModel.AddUserOnlyChatCommand((UserOnlyChatCommandModel)command);
                this.viewModel.RefreshUserOnlyChatCommands();
            };
            window.ForceShow();
        }

        private void UserOnlyChatCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<UserOnlyChatCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.RefreshUserOnlyChatCommands(); };
            window.ForceShow();
        }

        private async void UserOnlyChatCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.viewModel.RemoveUserOnlyChatCommand(FrameworkElementHelpers.GetDataContext<UserOnlyChatCommandModel>(sender));
                await ChannelSession.SaveSettings();
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            });
        }

        private void UserOnlyChatCommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
        }

        private void NewEntranceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, UserDataEditorWindowViewModel.UserEntranceCommandName);
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.EntranceCommand = command; };
            window.ForceShow();
        }

        private void ExistingEntranceCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CommandModelBase>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.EntranceCommand = command; };
            window.ForceShow();
        }

        private async void ExistingEntranceCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.viewModel.EntranceCommand = null;
                await ChannelSession.SaveSettings();
            });
        }

        private void UserDataEditorWindow_Closed(object sender, EventArgs e)
        {
            ChannelSession.Settings.Users.ManualValueChanged(this.user.ID);
        }
    }
}
