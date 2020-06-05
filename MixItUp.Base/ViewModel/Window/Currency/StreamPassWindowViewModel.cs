using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Currency
{
    public class StreamPassCustomLevelUpCommandViewModel
    {
        public int Level { get; set; }

        public CustomCommand Command { get; set; }

        public StreamPassCustomLevelUpCommandViewModel(int level, CustomCommand command)
        {
            this.Level = level;
            this.Command = command;
        }
    }

    public class NewStreamPassCommand
    {
        public bool AddCommand { get; set; }
        public string Description { get; set; }
        public ChatCommand Command { get; set; }

        public NewStreamPassCommand(string description, ChatCommand command)
        {
            this.AddCommand = true;
            this.Description = description;
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
        public IEnumerable<UserRoleEnum> Permissions { get; private set; } = EnumHelper.GetEnumList<UserRoleEnum>();
        public UserRoleEnum Permission
        {
            get { return this.permission; }
            set
            {
                this.permission = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum permission = UserRoleEnum.User;
        public int MaxLevel
        {
            get { return this.maxLevel; }
            set
            {
                this.maxLevel = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxLevel = 100;
        public int PointsForLevelUp
        {
            get { return this.pointsForLevelUp; }
            set
            {
                this.pointsForLevelUp = value;
                this.NotifyPropertyChanged();
            }
        }
        private int pointsForLevelUp = 500;
        public double SubMultiplier
        {
            get { return this.subMultiplier; }
            set
            {
                this.subMultiplier = value;
                this.NotifyPropertyChanged();
            }
        }
        private double subMultiplier = 1.5;
        public ICommand ManualResetCommand { get; private set; }

        public DateTimeOffset StartDate
        {
            get { return this.startDate; }
            set
            {
                this.startDate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("StartDateString");
            }
        }
        private DateTimeOffset startDate = DateTimeOffset.Now;
        public string StartDateString { get { return this.StartDate.ToFriendlyDateString(); } }
        public DateTimeOffset EndDate
        {
            get { return this.endDate; }
            set
            {
                this.endDate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("EndDateString");
            }
        }
        private DateTimeOffset endDate = DateTimeOffset.Now;
        public string EndDateString { get { return this.EndDate.ToFriendlyDateString(); } }

        public int ViewingRateAmount
        {
            get { return this.viewingRateAmount; }
            set
            {
                this.viewingRateAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int viewingRateAmount = 1;
        public int ViewingRateMinutes
        {
            get { return this.viewingRateMinutes; }
            set
            {
                this.viewingRateMinutes = value;
                this.NotifyPropertyChanged();
            }
        }
        private int viewingRateMinutes = 1;
        public int MinimumActiveRate
        {
            get { return this.minimumActiveRate; }
            set
            {
                this.minimumActiveRate = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimumActiveRate = 0;
        public int FollowBonus
        {
            get { return this.followBonus; }
            set
            {
                this.followBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int followBonus = 1;
        public int HostBonus
        {
            get { return this.hostBonus; }
            set
            {
                this.hostBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int hostBonus = 1;
        public int SubscribeBonus
        {
            get { return this.subscribeBonus; }
            set
            {
                this.subscribeBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int subscribeBonus = 1;
        public int DonationBonus
        {
            get { return this.donationBonus; }
            set
            {
                this.donationBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int donationBonus = 1;
        public int SparkBonus
        {
            get { return this.sparkBonus; }
            set
            {
                this.sparkBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int sparkBonus = 1;
        public int EmberBonus
        {
            get { return this.emberBonus; }
            set
            {
                this.emberBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int emberBonus = 1;

        public ObservableCollection<StreamPassCustomLevelUpCommandViewModel> CustomLevelUpCommands { get; set; } = new ObservableCollection<StreamPassCustomLevelUpCommandViewModel>();

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
            this.ManualResetCommand = this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you want to reset progress for all user?"))
                {
                    if (this.IsExisting)
                    {
                        await this.StreamPass.Reset();
                    }
                }
            });
        }

        public StreamPassWindowViewModel(StreamPassModel seasonPass)
            : this()
        {
            this.StreamPass = seasonPass;

            this.Name = this.StreamPass.Name;
            this.Permission = this.StreamPass.Permission;
            this.MaxLevel = this.StreamPass.MaxLevel;
            this.PointsForLevelUp = this.StreamPass.PointsForLevelUp;
            this.SubMultiplier = this.StreamPass.SubMultiplier;

            this.StartDate = this.StreamPass.StartDate;
            this.EndDate = this.StreamPass.EndDate;

            this.ViewingRateAmount = this.StreamPass.ViewingRateAmount;
            this.ViewingRateMinutes = this.StreamPass.ViewingRateMinutes;
            this.MinimumActiveRate = this.StreamPass.MinimumActiveRate;
            this.FollowBonus = this.StreamPass.FollowBonus;
            this.HostBonus = this.StreamPass.HostBonus;
            this.SubscribeBonus = this.StreamPass.SubscribeBonus;
            this.DonationBonus = this.StreamPass.DonationBonus;
            this.SparkBonus = this.StreamPass.SparkBonus;
            this.EmberBonus = this.StreamPass.EmberBonus;

            this.DefaultLevelUpCommand = this.StreamPass.DefaultLevelUpCommand;
            foreach (var kvp in this.StreamPass.CustomLevelUpCommands)
            {
                this.CustomLevelUpCommands.Add(new StreamPassCustomLevelUpCommandViewModel(kvp.Key, kvp.Value));
            }
        }

        public async Task<bool> ValidateAddingCustomLevelUpCommand()
        {
            if (this.CustomLevelUpNumber <= 0)
            {
                await DialogHelper.ShowMessage("You must specify a number greater than 0");
                return false;
            }

            if (this.CustomLevelUpNumber > this.MaxLevel)
            {
                await DialogHelper.ShowMessage("You must specify a number less than or equal to the Max Level");
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
            List<StreamPassCustomLevelUpCommandViewModel> commands = this.CustomLevelUpCommands.ToList();
            commands.Add(new StreamPassCustomLevelUpCommandViewModel(this.savedCustomLevelUpNumber, command));

            this.CustomLevelUpCommands.Clear();
            foreach (StreamPassCustomLevelUpCommandViewModel c in commands.OrderBy(c => c.Level))
            {
                this.CustomLevelUpCommands.Add(c);
            }
        }

        public async Task DeleteCustomLevelUpCommand(StreamPassCustomLevelUpCommandViewModel command)
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

            if (this.Name.Any(c => !char.IsLetterOrDigit(c) && c != ' '))
            {
                await DialogHelper.ShowMessage("The name can only contain letters, numbers, or spaces");
                return false;
            }

            StreamPassModel dupeStreamPass = ChannelSession.Settings.StreamPass.Values.FirstOrDefault(s => s.Name.Equals(this.Name));
            if (dupeStreamPass != null && (this.StreamPass == null || !this.StreamPass.ID.Equals(dupeStreamPass.ID)))
            {
                await DialogHelper.ShowMessage("There already exists a Stream Pass with this name");
                return false;
            }

            if (this.MaxLevel <= 0)
            {
                await DialogHelper.ShowMessage("The Max Level must be greater than 0");
                return false;
            }

            if (this.PointsForLevelUp <= 0)
            {
                await DialogHelper.ShowMessage("The Points for Level Up must be greater than 0");
                return false;
            }

            if (this.SubMultiplier < 1.0)
            {
                await DialogHelper.ShowMessage("The Sub Multiplier must be greater than or equal to 1.0");
                return false;
            }

            if (this.StartDate >= this.EndDate)
            {
                await DialogHelper.ShowMessage("The End Date must be after the Start Date");
                return false;
            }

            if (this.ViewingRateAmount < 0)
            {
                await DialogHelper.ShowMessage("The Viewing Rate Amount must be greater than or equal to 0");
                return false;
            }

            if (this.ViewingRateMinutes < 0)
            {
                await DialogHelper.ShowMessage("The Viewing Rate Minutes must be greater than or equal to 0");
                return false;
            }

            if (this.ViewingRateAmount == 0 ^ this.ViewingRateMinutes == 0)
            {
                await DialogHelper.ShowMessage("The Viewing Rate Amount & Minutes must both be greater than 0 or both equal to 0");
                return false;
            }

            if (this.FollowBonus < 0)
            {
                await DialogHelper.ShowMessage("The Follow Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.HostBonus < 0)
            {
                await DialogHelper.ShowMessage("The Host Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.SubscribeBonus < 0)
            {
                await DialogHelper.ShowMessage("The Subscribe Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.DonationBonus < 0)
            {
                await DialogHelper.ShowMessage("The Donation Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.SparkBonus < 0)
            {
                await DialogHelper.ShowMessage("The Spark Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.EmberBonus < 0)
            {
                await DialogHelper.ShowMessage("The Ember Bonus must be greater than or equal to 0");
                return false;
            }

            if (this.CustomLevelUpCommands.Count == 0 && this.DefaultLevelUpCommand == null)
            {
                await DialogHelper.ShowMessage("At least 1 custom level up command or the default level up command must be set");
                return false;
            }

            if (this.CustomLevelUpCommands.GroupBy(c => c.Level).Any(c => c.Count() > 1))
            {
                await DialogHelper.ShowMessage("There can only be 1 custom level up command per individual level");
                return false;
            }

            if (this.CustomLevelUpCommands.Any(c => c.Level > this.MaxLevel))
            {
                await DialogHelper.ShowMessage("There can not be any custom level up commands that are greater than the Max Level");
                return false;
            }

            return true;
        }

        public async Task Save()
        {
            if (this.IsNew)
            {
                this.StreamPass = new StreamPassModel();
                ChannelSession.Settings.StreamPass[this.StreamPass.ID] = this.StreamPass;
            }

            this.StreamPass.Name = this.Name;
            this.StreamPass.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name, maxLength: 15);
            this.StreamPass.Permission = this.Permission;
            this.StreamPass.MaxLevel = this.MaxLevel;
            this.StreamPass.PointsForLevelUp = this.PointsForLevelUp;
            this.StreamPass.SubMultiplier = this.SubMultiplier;

            this.StreamPass.StartDate = this.StartDate;
            this.StreamPass.EndDate = this.EndDate;

            this.StreamPass.ViewingRateAmount = this.ViewingRateAmount;
            this.StreamPass.ViewingRateMinutes = this.ViewingRateMinutes;
            this.StreamPass.MinimumActiveRate = this.MinimumActiveRate;
            this.StreamPass.FollowBonus = this.FollowBonus;
            this.StreamPass.HostBonus = this.HostBonus;
            this.StreamPass.SubscribeBonus = this.SubscribeBonus;
            this.StreamPass.DonationBonus = this.DonationBonus;
            this.StreamPass.SparkBonus = this.SparkBonus;
            this.StreamPass.EmberBonus = this.EmberBonus;

            this.StreamPass.DefaultLevelUpCommand = this.DefaultLevelUpCommand;
            this.StreamPass.CustomLevelUpCommands.Clear();
            foreach (StreamPassCustomLevelUpCommandViewModel customCommand in this.CustomLevelUpCommands)
            {
                this.StreamPass.CustomLevelUpCommands[customCommand.Level] = customCommand.Command;
            }

            await ChannelSession.SaveSettings();
        }

        public IEnumerable<NewAutoChatCommand> GetNewAutoChatCommands()
        {
            List<NewAutoChatCommand> commandsToAdd = new List<NewAutoChatCommand>();
            if (this.StreamPass != null)
            {
                ChatCommand statusCommand = new ChatCommand("User " + this.StreamPass.Name, this.StreamPass.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
                statusCommand.Actions.Add(new ChatAction(string.Format("@$username is level ${0} with ${1} points!", this.StreamPass.UserLevelSpecialIdentifier, this.StreamPass.UserAmountSpecialIdentifier)));
                commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", statusCommand.Commands.First(), "Shows User's Amount"), statusCommand));

                ChatCommand addCommand = new ChatCommand("Add " + this.StreamPass.Name, "add" + this.StreamPass.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                addCommand.Actions.Add(new CurrencyAction(this.StreamPass, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername"));
                addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text points for {0}!", this.StreamPass.Name)));
                commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", addCommand.Commands.First(), "Adds Amount To Specified User"), addCommand));

                ChatCommand addAllCommand = new ChatCommand("Add All " + this.StreamPass.Name, "addall" + this.StreamPass.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                addAllCommand.Actions.Add(new CurrencyAction(this.StreamPass, CurrencyActionTypeEnum.AddToAllChatUsers, "$arg1text"));
                addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text points for {0}!", this.StreamPass.Name)));
                commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", addAllCommand.Commands.First(), "Adds Amount To All Chat Users"), addAllCommand));
            }
            return commandsToAdd;
        }
    }
}
