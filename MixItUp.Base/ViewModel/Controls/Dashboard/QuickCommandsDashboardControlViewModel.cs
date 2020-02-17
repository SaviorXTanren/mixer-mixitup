using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Dashboard
{
    public class QuickCommandsDashboardControlViewModel : WindowControlViewModelBase
    {
        public CommandBase CommandOne
        {
            get { return commandOne; }
            set
            {
                this.commandOne = value;
                this.AssignCommand(0, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandBase commandOne;
        public string CommandOneName { get { return this.GetCommandName(this.CommandOne); } }
        public ICommand CommandOneCommand { get; set; }

        public CommandBase CommandTwo
        {
            get { return commandTwo; }
            set
            {
                this.commandTwo = value;
                this.AssignCommand(1, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandBase commandTwo;
        public string CommandTwoName { get { return this.GetCommandName(this.CommandTwo); } }
        public ICommand CommandTwoCommand { get; set; }

        public CommandBase CommandThree
        {
            get { return commandThree; }
            set
            {
                this.commandThree = value;
                this.AssignCommand(2, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandBase commandThree;
        public string CommandThreeName { get { return this.GetCommandName(this.CommandThree); } }
        public ICommand CommandThreeCommand { get; set; }

        public CommandBase CommandFour
        {
            get { return commandFour; }
            set
            {
                this.commandFour = value;
                this.AssignCommand(3, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandBase commandFour;
        public string CommandFourName { get { return this.GetCommandName(this.CommandFour); } }
        public ICommand CommandFourCommand { get; set; }

        public CommandBase CommandFive
        {
            get { return commandFive; }
            set
            {
                this.commandFive = value;
                this.AssignCommand(4, value);
                this.NotifyPropertiesChanged();
            }
        }
        private CommandBase commandFive;
        public string CommandFiveName { get { return this.GetCommandName(this.CommandFive); } }
        public ICommand CommandFiveCommand { get; set; }

        public QuickCommandsDashboardControlViewModel(WindowViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            this.commandOne = this.GetCommand(0);
            this.commandTwo = this.GetCommand(1);
            this.commandThree = this.GetCommand(2);
            this.commandFour = this.GetCommand(3);
            this.commandFive = this.GetCommand(4);

            this.CommandOneCommand = this.CreateCommand(async (parameter) => { await this.RunCommand(this.CommandOne); });
            this.CommandTwoCommand = this.CreateCommand(async (parameter) => { await this.RunCommand(this.CommandTwo); });
            this.CommandThreeCommand = this.CreateCommand(async (parameter) => { await this.RunCommand(this.CommandThree); });
            this.CommandFourCommand = this.CreateCommand(async (parameter) => { await this.RunCommand(this.CommandFour); });
            this.CommandFiveCommand = this.CreateCommand(async (parameter) => { await this.RunCommand(this.CommandFive); });

            this.NotifyPropertiesChanged();
        }

        public async Task<bool> CanSelectCommands()
        {
            if (!ChannelSession.AllCommands.Any(c => !(c is PreMadeChatCommand)))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.QuickCommandSelectFail);
                return false;
            }
            return true;
        }

        private CommandBase GetCommand(int index)
        {
            Guid id = ChannelSession.Settings.DashboardQuickCommands[index];
            if (id != Guid.Empty)
            {
                return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(id));
            }
            return null;
        }

        private void AssignCommand(int index, CommandBase command)
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

        private string GetCommandName(CommandBase command) { return (command != null) ? command.Name : MixItUp.Base.Resources.Unassigned; }

        private async Task RunCommand(CommandBase command)
        {
            if (command != null)
            {
                await command.Perform();
            }
        }

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
