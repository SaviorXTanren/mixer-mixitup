using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class QueueUser
    {
        public UserV2ViewModel user { get; set; }

        public int QueuePosition { get; set; }

        public string Username { get { return this.user.FullDisplayName; } }

        public string Platform { get { return EnumLocalizationHelper.GetLocalizedName(this.user.Platform); } }

        public string PrimaryRole { get { return EnumHelper.GetEnumName(this.user.PrimaryRole); } }

        public QueueUser(UserV2ViewModel user, int queuePosition)
        {
            this.user = user;
            this.QueuePosition = queuePosition;
        }
    }

    public class GameQueueMainControlViewModel : WindowControlViewModelBase
    {
        public bool IsEnabled { get { return ServiceManager.Get<GameQueueService>().IsEnabled; } }

        public string EnableDisableButtonText { get { return (this.IsEnabled) ? MixItUp.Base.Resources.Disable : MixItUp.Base.Resources.Enable; } }

        public bool SubPriority
        {
            get { return ChannelSession.Settings.GameQueueSubPriority; }
            set
            {
                ChannelSession.Settings.GameQueueSubPriority = value;
                this.NotifyPropertyChanged();
            }
        }

        public ThreadSafeObservableCollection<QueueUser> QueueUsers { get; private set; } = new ThreadSafeObservableCollection<QueueUser>();

        public CommandModelBase GameQueueUserJoinedCommand
        {
            get { return this.gameQueueUserJoinedCommand; }
            set
            {
                this.gameQueueUserJoinedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase gameQueueUserJoinedCommand;
        public CommandModelBase GameQueueUserSelectedCommand
        {
            get { return this.gameQueueUserSelectedCommand; }
            set
            {
                this.gameQueueUserSelectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase gameQueueUserSelectedCommand;

        public ICommand EnableDisableCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ClearQueueCommand { get; private set; }

        public GameQueueMainControlViewModel(UIViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            this.GameQueueUserJoinedCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.GameQueueUserJoinedCommandID);
            this.GameQueueUserSelectedCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.GameQueueUserSelectedCommandID);

            this.EnableDisableCommand = this.CreateCommand(async () =>
            {
                if (this.IsEnabled)
                {
                    await ServiceManager.Get<GameQueueService>().Disable();
                }
                else
                {
                    await ServiceManager.Get<GameQueueService>().Enable();
                }
                this.NotifyPropertyChanges();
            });

            this.MoveUpCommand = this.CreateCommand(async (user) =>
            {
                await ServiceManager.Get<GameQueueService>().MoveUp((UserV2ViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.MoveDownCommand = this.CreateCommand(async (user) =>
            {
                await ServiceManager.Get<GameQueueService>().MoveDown((UserV2ViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.DeleteCommand = this.CreateCommand(async (user) =>
            {
                await ServiceManager.Get<GameQueueService>().Leave((UserV2ViewModel)user);
                this.NotifyPropertyChanges();
            });

            this.ClearQueueCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.ClearGameQueuePrompt))
                {
                    await ServiceManager.Get<GameQueueService>().Clear();
                    this.NotifyPropertyChanges();
                }
            });
        }

        private void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            List<QueueUser> queue = new List<QueueUser>();
            int position = 1;
            foreach (UserV2ViewModel user in ServiceManager.Get<GameQueueService>().Queue)
            {
                queue.Add(new QueueUser(user, position));
                position++;
            }
            this.QueueUsers.ClearAndAddRange(queue);

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
