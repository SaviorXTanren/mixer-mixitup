using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class QueueUser
    {
        public UserViewModel user { get; set; }

        public int QueuePosition { get; set; }

        public string UserName { get { return this.user.UserName; } }

        public string PrimaryRole { get { return EnumHelper.GetEnumName(this.user.PrimaryRole); } }

        public QueueUser(UserViewModel user, int queuePosition)
        {
            this.user = user;
            this.QueuePosition = queuePosition;
        }
    }

    public class GameQueueMainControlViewModel : MainControlViewModelBase
    {
        public bool IsEnabled { get { return ChannelSession.Services.GameQueueService.IsEnabled; } }

        public string EnableDisableButtonText { get { return (this.IsEnabled) ? "Disable" : "Enable"; } }

        public bool SubPriority
        {
            get { return ChannelSession.Settings.GameQueueSubPriority; }
            set
            {
                ChannelSession.Settings.GameQueueSubPriority = value;
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<QueueUser> QueueUsers { get; private set; } = new ObservableCollection<QueueUser>();

        public ICommand EnableDisableCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ClearQueueCommand { get; private set; }

        public GameQueueMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            this.EnableDisableCommand = this.CreateCommand(async (x) =>
            {
                if (this.IsEnabled)
                {
                    await ChannelSession.Services.GameQueueService.Disable();
                }
                else
                {
                    await ChannelSession.Services.GameQueueService.Enable();
                }
                this.NotifyPropertyChanges();
            });

            this.MoveUpCommand = this.CreateCommand(async (user) =>
            {
                await ChannelSession.Services.GameQueueService.MoveUp((UserViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.MoveDownCommand = this.CreateCommand(async (user) =>
            {
                await ChannelSession.Services.GameQueueService.MoveDown((UserViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.DeleteCommand = this.CreateCommand(async (user) =>
            {
                await ChannelSession.Services.GameQueueService.Leave((UserViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.ClearQueueCommand = this.CreateCommand(async (x) =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you want to clear the Game Queue queue?"))
                {
                    await ChannelSession.Services.GameQueueService.Clear();
                    this.NotifyPropertyChanges();
                }
            });
        }

        private async void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                this.QueueUsers.Clear();
                int position = 1;
                foreach (UserViewModel user in ChannelSession.Services.GameQueueService.Queue)
                {
                    this.QueueUsers.Add(new QueueUser(user, position));
                    position++;
                }
                return Task.FromResult(0);
            });

            this.NotifyPropertyChanges();
        }

        private void NotifyPropertyChanges()
        {
            this.NotifyPropertyChanged("IsEnabled");
            this.NotifyPropertyChanged("EnableDisableButtonText");
            this.NotifyPropertyChanged("SubPriority");
            this.NotifyPropertyChanged("QueueUsers");
        }
    }
}
