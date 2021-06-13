using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Dashboard
{
    public class QuickCommandsDashboardControlViewModel : WindowControlViewModelBase
    {
        public CommandModelBase CommandOne
        {
            get { return commandOne; }
            set
            {
                this.commandOne = value;
                this.AssignCommand(0, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandModelBase commandOne;
        public string CommandOneName { get { return this.GetCommandName(this.CommandOne); } }
        public ICommand CommandOneCommand { get; set; }

        public CommandModelBase CommandTwo
        {
            get { return commandTwo; }
            set
            {
                this.commandTwo = value;
                this.AssignCommand(1, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandModelBase commandTwo;
        public string CommandTwoName { get { return this.GetCommandName(this.CommandTwo); } }
        public ICommand CommandTwoCommand { get; set; }

        public CommandModelBase CommandThree
        {
            get { return commandThree; }
            set
            {
                this.commandThree = value;
                this.AssignCommand(2, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandModelBase commandThree;
        public string CommandThreeName { get { return this.GetCommandName(this.CommandThree); } }
        public ICommand CommandThreeCommand { get; set; }

        public CommandModelBase CommandFour
        {
            get { return commandFour; }
            set
            {
                this.commandFour = value;
                this.AssignCommand(3, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandModelBase commandFour;
        public string CommandFourName { get { return this.GetCommandName(this.CommandFour); } }
        public ICommand CommandFourCommand { get; set; }

        public CommandModelBase CommandFive
        {
            get { return commandFive; }
            set
            {
                this.commandFive = value;
                this.AssignCommand(4, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandModelBase commandFive;
        public string CommandFiveName { get { return this.GetCommandName(this.CommandFive); } }
        public ICommand CommandFiveCommand { get; set; }

        public QuickCommandsDashboardControlViewModel(UIViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            this.commandOne = this.GetCommand(0);
            this.commandTwo = this.GetCommand(1);
            this.commandThree = this.GetCommand(2);
            this.commandFour = this.GetCommand(3);
            this.commandFive = this.GetCommand(4);

            this.CommandOneCommand = this.CreateCommand(async () => { await this.RunCommand(this.CommandOne); });
            this.CommandTwoCommand = this.CreateCommand(async () => { await this.RunCommand(this.CommandTwo); });
            this.CommandThreeCommand = this.CreateCommand(async () => { await this.RunCommand(this.CommandThree); });
            this.CommandFourCommand = this.CreateCommand(async () => { await this.RunCommand(this.CommandFour); });
            this.CommandFiveCommand = this.CreateCommand(async () => { await this.RunCommand(this.CommandFive); });

            this.NotifyPropertiesChanged();
        }

        public async Task<bool> CanSelectCommands()
        {
            if (!ServiceManager.Get<CommandService>().AllCommands.Any(c => !(c is PreMadeChatCommandModelBase)))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.QuickCommandSelectFail);
                return false;
            }
            return true;
        }

        private CommandModelBase GetCommand(int index)
        {
            Guid id = ChannelSession.Settings.DashboardQuickCommands[index];
            if (id != Guid.Empty)
            {
                return ChannelSession.Settings.GetCommand(id);
            }
            return null;
        }

        private void AssignCommand(int index, CommandModelBase command)
        {
            if (command != null)
            {
                ChannelSession.Settings.DashboardQuickCommands[index] = command.ID;
            }
            else
            {
                ChannelSession.Settings.DashboardQuickCommands[index] = Guid.Empty;
            }
        }

        private string GetCommandName(CommandModelBase command) { return (command != null) ? command.Name : MixItUp.Base.Resources.Unassigned; }

        private async Task RunCommand(CommandModelBase command) { await ServiceManager.Get<CommandService>().Queue(command); }

        private void NotifyPropertiesChanged()
        {
            this.NotifyPropertyChanged("CommandOne");
            this.NotifyPropertyChanged("CommandTwo");
            this.NotifyPropertyChanged("CommandThree");
            this.NotifyPropertyChanged("CommandFour");
            this.NotifyPropertyChanged("CommandFive");
            this.NotifyPropertyChanged("CommandOneName");
            this.NotifyPropertyChanged("CommandTwoName");
            this.NotifyPropertyChanged("CommandThreeName");
            this.NotifyPropertyChanged("CommandFourName");
            this.NotifyPropertyChanged("CommandFiveName");
        }
    }
}
