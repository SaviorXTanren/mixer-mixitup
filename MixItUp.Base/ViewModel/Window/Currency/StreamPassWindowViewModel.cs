using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Currency
{
    public class StreamPassUniqueLevelUpCommandViewModel
    {
        public int Level { get; set; }

        public CustomCommand Command { get; set; }

        public StreamPassUniqueLevelUpCommandViewModel(int level, CustomCommand command)
        {
            this.Level = level;
            this.Command = command;
        }
    }

    public class StreamPassWindowViewModel : WindowViewModelBase
    {
        public StreamPassModel StreamPass { get; private set; }

        public bool IsNew { get { return this.StreamPass == null; } }
        public bool IsExisting { get { return !this.IsNew; } }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public int TotalLevels
        {
            get { return this.totalLevels; }
            set
            {
                this.totalLevels = value;
                this.NotifyPropertyChanged();
            }
        }
        private int totalLevels;

        public ObservableCollection<StreamPassUniqueLevelUpCommandViewModel> CustomLevelUpCommands { get; set; } = new ObservableCollection<StreamPassUniqueLevelUpCommandViewModel>();

        public int CustomLevelUpNumber
        {
            get { return this.customLevelUpNumber; }
            set
            {
                this.customLevelUpNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        private int customLevelUpNumber;

        public CustomCommand DefaultLevelUpCommand
        {
            get { return this.defaultLevelUpCommand; }
            set
            {
                this.defaultLevelUpCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("DefaultLevelUpCommandSet");
                this.NotifyPropertyChanged("DefaultLevelUpCommandNotSet");
            }
        }
        private CustomCommand defaultLevelUpCommand;

        public bool DefaultLevelUpCommandSet { get { return this.DefaultLevelUpCommand != null; } }
        public bool DefaultLevelUpCommandNotSet { get { return !this.DefaultLevelUpCommandSet; } }

        private int savedCustomLevelUpNumber;

        public StreamPassWindowViewModel()
        {

        }

        public StreamPassWindowViewModel(StreamPassModel seasonPass)
            : this()
        {
            this.StreamPass = seasonPass;
        }

        public async Task<bool> ValidateAddingCustomLevelUpCommand()
        {
            if (this.CustomLevelUpNumber > 0)
            {
                await DialogHelper.ShowMessage("You must specify a number greater than 0");
                return false;
            }

            if (this.CustomLevelUpNumber <= this.TotalLevels)
            {
                await DialogHelper.ShowMessage("You must specify less than or equal to the Total Levels");
                return false;
            }

            if (this.CustomLevelUpCommands.Any(c => c.Level == this.CustomLevelUpNumber))
            {
                await DialogHelper.ShowMessage("There already exists a custom command for this level");
                return false;
            }

            this.savedCustomLevelUpNumber = this.CustomLevelUpNumber;
            return true;
        }

        public void AddCustomLevelUpCommand(CustomCommand command)
        {
            List<StreamPassUniqueLevelUpCommandViewModel> commands = this.CustomLevelUpCommands.ToList();
            commands.Add(new StreamPassUniqueLevelUpCommandViewModel(this.savedCustomLevelUpNumber, command));

            this.CustomLevelUpCommands.Clear();
            foreach (StreamPassUniqueLevelUpCommandViewModel c in commands.OrderBy(c => c.Level))
            {
                this.CustomLevelUpCommands.Add(c);
            }
        }

        public async Task DeleteCustomLevelUpCommand(StreamPassUniqueLevelUpCommandViewModel command)
        {
            if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmDeleteCustomLevelUpCommand))
            {
                this.CustomLevelUpCommands.Remove(command);
            }
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                await DialogHelper.ShowMessage("A valid name must be specified");
                return false;
            }

            return true;
        }

        public async Task Save()
        {
            await ChannelSession.SaveSettings();
        }
    }
}
