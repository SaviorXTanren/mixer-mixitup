using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Currency
{
    public class StreamPassCustomLevelUpCommandViewModel
    {
        public int Level { get; set; }

        public Guid CommandID { get; set; }

        public CommandModelBase Command
        {
            get { return ChannelSession.Settings.GetCommand(this.CommandID); }
            set
            {
                if (value != null)
                {
                    this.CommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.CommandID);
                    this.CommandID = Guid.Empty;
                }
            }
        }

        public StreamPassCustomLevelUpCommandViewModel(int level, Guid id)
        {
            this.Level = level;
            this.CommandID = id;
        }

        public StreamPassCustomLevelUpCommandViewModel(int level, CommandModelBase command)
        {
            this.Level = level;
            this.Command = command;
        }
    }

    public class NewStreamPassCommand
    {
        public bool AddCommand { get; set; }
        public string Description { get; set; }
        public CommandModelBase Command { get; set; }

        public NewStreamPassCommand(string description, CommandModelBase command)
        {
            this.AddCommand = true;
            this.Description = description;
            this.Command = command;
        }
    }

    public class StreamPassWindowViewModel : UIViewModelBase
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
        public IEnumerable<UserRoleEnum> Permissions { get; private set; } = UserRoles.All;
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
        public double DonationBonus
        {
            get { return this.donationBonus; }
            set
            {
                this.donationBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private double donationBonus = 1.5;
        public double BitsBonus
        {
            get { return this.bitsBonus; }
            set
            {
                this.bitsBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private double bitsBonus = 1.5;

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

        public CommandModelBase DefaultLevelUpCommand
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
        private CommandModelBase defaultLevelUpCommand;

        private int savedCustomLevelUpNumber;

        public StreamPassWindowViewModel()
        {
            this.ManualResetCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.ResetAllProgressPrompt))
                {
                    if (this.IsExisting)
                    {
                        await this.StreamPass.Reset();
                    }
                }
            });

            this.DefaultLevelUpCommand = new CustomCommandModel(MixItUp.Base.Resources.LevelUp);
        }

        public StreamPassWindowViewModel(StreamPassModel seasonPass)
            : this()
        {
            this.StreamPass = seasonPass;

            this.Name = this.StreamPass.Name;
            this.Permission = this.StreamPass.UserPermission;
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
            this.BitsBonus = this.StreamPass.BitsBonus;

            this.DefaultLevelUpCommand = this.StreamPass.DefaultLevelUpCommand;
            if (this.DefaultLevelUpCommand == null)
            {
                this.DefaultLevelUpCommand = new CustomCommandModel(MixItUp.Base.Resources.LevelUp);
                this.DefaultLevelUpCommand.IsEnabled = false;
            }

            this.CustomLevelUpCommands.AddRange(this.StreamPass.CustomLevelUpCommands.Select(kvp => new StreamPassCustomLevelUpCommandViewModel(kvp.Key, kvp.Value)));
        }

        public async Task<bool> ValidateAddingCustomLevelUpCommand()
        {
            if (this.CustomLevelUpNumber <= 0)
            {
                await DialogHelper.ShowMessage(Resources.CustomLevelGreaterThanZero);
                return false;
            }

            if (this.CustomLevelUpNumber > this.MaxLevel)
            {
                await DialogHelper.ShowMessage(Resources.LessThanMaxLevel);
                return false;
            }

            if (this.CustomLevelUpCommands.Any(c => c.Level == this.CustomLevelUpNumber))
            {
                await DialogHelper.ShowMessage(Resources.CustomCommandAlreadyExists);
                return false;
            }

            this.savedCustomLevelUpNumber = this.CustomLevelUpNumber;
            return true;
        }

        public void AddCustomLevelUpCommand(CommandModelBase command)
        {
            List<StreamPassCustomLevelUpCommandViewModel> commands = this.CustomLevelUpCommands.ToList();
            commands.Add(new StreamPassCustomLevelUpCommandViewModel(this.savedCustomLevelUpNumber, command));

            this.CustomLevelUpCommands.ClearAndAddRange(commands.OrderBy(c => c.Level));
        }

        public void DeleteCustomLevelUpCommand(StreamPassCustomLevelUpCommandViewModel command)
        {
            this.CustomLevelUpCommands.Remove(command);
            command.Command = null;
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                await DialogHelper.ShowMessage(Resources.StreamPassNameRequired);
                return false;
            }

            if (this.Name.Any(c => !char.IsLetterOrDigit(c) && c != ' '))
            {
                await DialogHelper.ShowMessage(Resources.StreamPassNameInvalid);
                return false;
            }

            StreamPassModel dupeStreamPass = ChannelSession.Settings.StreamPass.Values.FirstOrDefault(s => s.Name.Equals(this.Name));
            if (dupeStreamPass != null && (this.StreamPass == null || !this.StreamPass.ID.Equals(dupeStreamPass.ID)))
            {
                await DialogHelper.ShowMessage(Resources.StreamPassNameDuplicate);
                return false;
            }

            if (this.MaxLevel <= 0)
            {
                await DialogHelper.ShowMessage(Resources.MaxLevelGreaterThanZero);
                return false;
            }

            if (this.PointsForLevelUp <= 0)
            {
                await DialogHelper.ShowMessage(Resources.PointsForLevelUpGreaterThanZero);
                return false;
            }

            if (this.SubMultiplier < 1.0)
            {
                await DialogHelper.ShowMessage(Resources.SubMultiplierOneOrMore);
                return false;
            }

            if (this.StartDate >= this.EndDate)
            {
                await DialogHelper.ShowMessage(Resources.EndDateInvalid);
                return false;
            }

            if (this.ViewingRateAmount < 0)
            {
                await DialogHelper.ShowMessage(Resources.ViewingRateAmountZeroOrMore);
                return false;
            }

            if (this.ViewingRateMinutes < 0)
            {
                await DialogHelper.ShowMessage(Resources.ViewingRateMinutesZeroOrMore);
                return false;
            }

            if (this.ViewingRateAmount == 0 ^ this.ViewingRateMinutes == 0)
            {
                await DialogHelper.ShowMessage(Resources.ViewingRateMinutesInvalid);
                return false;
            }

            if (this.FollowBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.FollowBonusZeroOrMore);
                return false;
            }

            if (this.HostBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.HostBonusZeroOrMore);
                return false;
            }

            if (this.SubscribeBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.SubscriberBonusZeroOrMore);
                return false;
            }

            if (this.DonationBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.DonationBonusZeroOrMore);
                return false;
            }

            if (this.BitsBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.BitsBonusZeroOrMore);
                return false;
            }

            if (this.CustomLevelUpCommands.GroupBy(c => c.Level).Any(c => c.Count() > 1))
            {
                await DialogHelper.ShowMessage(Resources.OneCustomLevelCommand);
                return false;
            }

            if (this.CustomLevelUpCommands.Any(c => c.Level > this.MaxLevel))
            {
                await DialogHelper.ShowMessage(Resources.MaxCustomLevelCommand);
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

            this.StreamPass.Name = this.Name.Trim();
            this.StreamPass.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name, maxLength: 15);
            this.StreamPass.UserPermission = this.Permission;
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
            this.StreamPass.BitsBonus = this.BitsBonus;

            this.StreamPass.DefaultLevelUpCommand = this.DefaultLevelUpCommand;
            this.StreamPass.CustomLevelUpCommands.Clear();
            foreach (StreamPassCustomLevelUpCommandViewModel customCommand in this.CustomLevelUpCommands)
            {
                this.StreamPass.CustomLevelUpCommands[customCommand.Level] = customCommand.CommandID;
            }

            await ChannelSession.SaveSettings();
        }

        public IEnumerable<NewAutoChatCommandModel> GetNewAutoChatCommands()
        {
            List<NewAutoChatCommandModel> commandsToAdd = new List<NewAutoChatCommandModel>();
            if (this.StreamPass != null)
            {
                ChatCommandModel statusCommand = new ChatCommandModel($"{MixItUp.Base.Resources.User} {this.StreamPass.Name}", new HashSet<string>() { this.StreamPass.SpecialIdentifier });
                statusCommand.Requirements.AddBasicRequirements();
                statusCommand.Requirements.Role.UserRole = UserRoleEnum.User;
                statusCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                statusCommand.Requirements.Cooldown.IndividualAmount = 5;
                statusCommand.Actions.Add(new ChatActionModel(string.Format(MixItUp.Base.Resources.ConsumablesStreamPassCommandDefault, this.StreamPass.UserLevelSpecialIdentifier, this.StreamPass.UserAmountSpecialIdentifier)));
                commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", statusCommand.Triggers.First(), MixItUp.Base.Resources.ShowsUsersAmount), statusCommand));

                ChatCommandModel addCommand = new ChatCommandModel($"{MixItUp.Base.Resources.Add} {this.StreamPass.Name}", new HashSet<string>() { MixItUp.Base.Resources.Add.ToLower() + this.StreamPass.SpecialIdentifier });
                addCommand.Requirements.AddBasicRequirements();
                addCommand.Requirements.Role.UserRole = UserRoleEnum.Moderator;
                addCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                addCommand.Requirements.Cooldown.IndividualAmount = 5;
                addCommand.Actions.Add(new ConsumablesActionModel(this.StreamPass, ConsumablesActionTypeEnum.AddToSpecificUser, usersMustBePresent:true, "$arg2text", username: "$targetusername"));
                addCommand.Actions.Add(new ChatActionModel(string.Format(MixItUp.Base.Resources.ConsumablesStreamPassAddCommandDefault, this.StreamPass.Name)));
                commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", addCommand.Triggers.First(), MixItUp.Base.Resources.AddsAmountToSpecifiedUser), addCommand));

                ChatCommandModel addAllCommand = new ChatCommandModel($"{MixItUp.Base.Resources.AddAll} {this.StreamPass.Name}", new HashSet<string>() { MixItUp.Base.Resources.AddAll.ToLower() + this.StreamPass.SpecialIdentifier });
                addAllCommand.Requirements.AddBasicRequirements();
                addAllCommand.Requirements.Role.UserRole = UserRoleEnum.Moderator;
                addAllCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                addAllCommand.Requirements.Cooldown.IndividualAmount = 5;
                addAllCommand.Actions.Add(new ConsumablesActionModel(this.StreamPass, ConsumablesActionTypeEnum.AddToAllChatUsers, usersMustBePresent: true, "$arg1text"));
                addAllCommand.Actions.Add(new ChatActionModel(string.Format(MixItUp.Base.Resources.ConsumablesStreamPassAddAllCommandDefault, this.StreamPass.Name)));
                commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", addAllCommand.Triggers.First(), MixItUp.Base.Resources.AddsAmountToAllChatUsers), addAllCommand));
            }
            return commandsToAdd;
        }
    }
}
