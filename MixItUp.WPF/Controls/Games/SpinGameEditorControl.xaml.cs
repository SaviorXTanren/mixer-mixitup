using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    public class SpinOutcome
    {
        public string Name { get; set; }

        public CustomCommand Command { get; set; }

        public int Payout { get; set; }

        public int UserChance { get; set; }
        public int SubscriberChance { get; set; }
        public int ModChance { get; set; }

        public SpinOutcome(string name, CustomCommand command, int payout = 0, int userChance = 0, int subscriberChance = 0, int modChance = 0)
        {
            this.Name = name;
            this.Command = command;
            this.Payout = payout;
            this.UserChance = userChance;
            this.SubscriberChance = subscriberChance;
            this.ModChance = modChance;
        }

        public SpinOutcome(GameOutcome outcome) : this(outcome.Name, outcome.Command)
        {
            this.Payout = Convert.ToInt32(outcome.Payout * 100.0);
            this.UserChance = outcome.RoleProbabilities[MixerRoleEnum.User];
            this.SubscriberChance = outcome.RoleProbabilities[MixerRoleEnum.Subscriber];
            this.ModChance = outcome.RoleProbabilities[MixerRoleEnum.Mod];
        }

        public string PayoutString
        {
            get { return this.Payout.ToString(); }
            set { this.Payout = this.GetPercentageFromString(value); }
        }

        public string UserChanceString
        {
            get { return this.UserChance.ToString(); }
            set { this.UserChance = this.GetPercentageFromString(value); }
        }

        public string SubscriberChanceString
        {
            get { return this.SubscriberChance.ToString(); }
            set { this.SubscriberChance = this.GetPercentageFromString(value); }
        }

        public string ModChanceString
        {
            get { return this.ModChance.ToString(); }
            set { this.ModChance = this.GetPercentageFromString(value); }
        }

        private int GetPercentageFromString(string value)
        {
            if (int.TryParse(value, out int percentage) && percentage >= 0)
            {
                return percentage;
            }
            return 0;
        }

        public GameOutcome GetGameOutcome()
        {
            return new GameOutcome(this.Name, Convert.ToDouble(this.Payout) / 100.0,
                new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserChance }, { MixerRoleEnum.Subscriber, this.SubscriberChance }, { MixerRoleEnum.Mod, this.ModChance } },
                this.Command);
        }
    }

    /// <summary>
    /// Interaction logic for SpinGameEditorControl.xaml
    /// </summary>
    public partial class SpinGameEditorControl : GameEditorControlBase
    {
        private ObservableCollection<SpinOutcome> outcomes = new ObservableCollection<SpinOutcome>();

        private SpinGameCommand existingCommand;

        public SpinGameEditorControl()
        {
            InitializeComponent();
        }

        public SpinGameEditorControl(SpinGameCommand command)
            : this()
        {
            this.existingCommand = command;
        }

        public override async Task<bool> Validate()
        {
            if (!await this.CommandDetailsControl.Validate())
            {
                return false;
            }

            foreach (SpinOutcome outcome in this.outcomes)
            {
                if (string.IsNullOrEmpty(outcome.Name))
                {
                    await MessageBoxHelper.ShowMessageDialog("An outcome is missing a name");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("An outcome is missing a command");
                    return false;
                }
            }

            int userTotalChance = this.outcomes.Select(o => o.UserChance).Sum();
            if (userTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined User Chance %'s do not equal 100");
                return false;
            }

            int subscriberTotalChance = this.outcomes.Select(o => o.SubscriberChance).Sum();
            if (subscriberTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined Sub Chance %'s do not equal 100");
                return false;
            }

            int modTotalChance = this.outcomes.Select(o => o.ModChance).Sum();
            if (modTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined Mod Chance %'s do not equal 100");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new SpinGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), this.outcomes.Select(o => o.GetGameOutcome())));
        }

        protected override Task OnLoaded()
        {
            this.OutcomesItemsControl.ItemsSource = this.outcomes;

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                foreach (GameOutcome outcome in this.existingCommand.Outcomes)
                {
                    this.outcomes.Add(new SpinOutcome(outcome));
                }
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Spin", "spin", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.outcomes.Add(new SpinOutcome("Lose", this.CreateBasicChatCommand("Sorry @$username, you lost the spin!"), 0, 70, 70, 70));
                this.outcomes.Add(new SpinOutcome("Win", this.CreateBasicChatCommand("Congrats @$username, you won $gamepayout " + currency.Name + "!"), 200, 30, 30, 30));
            }

            return base.OnLoaded();
        }

        private void OutcomeCommandButtonsControl_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SpinOutcome outcome = (SpinOutcome)button.DataContext;
            this.outcomes.Remove(outcome);
        }

        private void AddOutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.outcomes.Add(new SpinOutcome("", this.CreateBasicChatCommand("@$username")));
        }

        private CustomCommand CreateBasicChatCommand(string message)
        {
            CustomCommand command = new CustomCommand("Game Outcome");
            command.Actions.Add(new ChatAction(message));
            return command;
        }
    }
}
