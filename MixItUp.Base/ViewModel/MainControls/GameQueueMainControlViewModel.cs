using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class QueueUser
    {
        public CommandParametersModel parameters { get; set; }

        public int QueuePosition { get; set; }

        public string Username { get { return this.parameters.User.FullDisplayName; } }

        public string Platform { get { return EnumLocalizationHelper.GetLocalizedName(this.parameters.User.Platform); } }

        public string PrimaryRole { get { return EnumLocalizationHelper.GetLocalizedName(this.parameters.User.PrimaryRole); } }

        public QueueUser(CommandParametersModel parameters, int queuePosition)
        {
            this.parameters = parameters;
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
            GameQueueService.OnGameQueueUpdated += GameQueueService_OnGameQueueUpdated;

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
                await ServiceManager.Get<GameQueueService>().MoveUp(((QueueUser)user).parameters);
                this.NotifyPropertyChanges();
            });

            this.MoveDownCommand = this.CreateCommand(async (user) =>
            {
                await ServiceManager.Get<GameQueueService>().MoveDown(((QueueUser)user).parameters);
                this.NotifyPropertyChanges();
            });

            this.DeleteCommand = this.CreateCommand(async (user) =>
            {
                await ServiceManager.Get<GameQueueService>().Leave(((QueueUser)user).parameters);
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

        private void GameQueueService_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            List<QueueUser> queue = new List<QueueUser>();
            int position = 1;
            foreach (CommandParametersModel parameters in ServiceManager.Get<GameQueueService>().Queue)
            {
                queue.Add(new QueueUser(parameters, position));
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
