using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Commands
{
    public class CommandInstanceViewModel : UIViewModelBase
    {
        public CommandInstanceModel Model { get { return this.model; } }
        private CommandInstanceModel model;

        public string CommandName { get { return this.model.ToString(); } }

        public CommandTypeEnum CommandType
        {
            get
            {
                CommandModelBase command = this.model.Command;
                if (command != null)
                {
                    switch (command.Type)
                    {
                        case CommandTypeEnum.PreMade: return CommandTypeEnum.Chat;
                        case CommandTypeEnum.UserOnlyChat: return CommandTypeEnum.Chat;
                        default: return command.Type;
                    }
                }
                return CommandTypeEnum.Custom;
            }
        }

        public CommandInstanceStateEnum State { get { return this.model.State; } }

        public string Username { get { return this.model.Parameters?.User?.Username ?? MixItUp.Base.Resources.Unknown; } }

        public DateTimeOffset DateTime { get { return this.model.DateTime; } }

        public string ErrorMessage { get { return this.model.ErrorMessage; } }

        public bool HasErrorMessage { get { return !string.IsNullOrEmpty(this.ErrorMessage); } }

        public bool ShowInUI { get { return this.model.ShowInUI; } }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                this.isSelected = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isSelected;

        public ICommand CancelCommand { get; set; }

        public ICommand ReplayCommand { get; set; }

        public CommandInstanceViewModel(CommandInstanceModel model)
        {
            this.model = model;
            this.model.OnStateUpdated += Model_OnStateUpdated;

            this.CancelCommand = this.CreateCommand(() =>
            {
                ServiceManager.Get<CommandService>().Cancel(this.model);
                return Task.CompletedTask;
            });

            this.ReplayCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommandService>().Replay(this.model.Duplicate());
            });
        }

        public bool ShowActionButtons { get { return this.CommandType != CommandTypeEnum.Game; } }

        public bool ShowCancelButton { get { return this.State == CommandInstanceStateEnum.Pending || this.State == CommandInstanceStateEnum.Running; } }

        public bool ShowReplayButton { get { return !this.ShowCancelButton; } }

        private void Model_OnStateUpdated(object sender, CommandInstanceStateEnum e)
        {
            this.NotifyPropertyChanged("State");
            this.NotifyPropertyChanged("HasErrorMessage");
            this.NotifyPropertyChanged("ErrorMessage");
            this.NotifyPropertyChanged("ShowCancelButton");
            this.NotifyPropertyChanged("ShowReplayButton");
        }
    }
}
