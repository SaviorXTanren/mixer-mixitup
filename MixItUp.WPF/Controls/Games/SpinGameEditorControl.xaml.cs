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
        private ObservableCollection<SpinOutcome> spinOutcomes = new ObservableCollection<SpinOutcome>();

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
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A Game Name is required");
                return false;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("At least 1 chat trigger must be specified");
                return false;
            }

            if (!CommandBase.IsValidCommandString(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("The chat triggers contain an invalid character");
                return false;
            }

            IEnumerable<string> commandStrings = this.GetChatTriggers();
            if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Each chat trigger must be unique");
                return false;
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            if (this.CurrencyTypeComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A currency must be selected");
                return false;
            }

            if (string.IsNullOrEmpty(this.MinimumAmountTextBox.Text) || !int.TryParse(this.MinimumAmountTextBox.Text, out int minimum) || minimum <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid minimum amount must be specified");
                return false;
            }

            int maximum = 0;
            if (!string.IsNullOrEmpty(this.MaximumAmountTextBox.Text))
            {
                if (!int.TryParse(this.MaximumAmountTextBox.Text, out maximum) || maximum <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid maximum amount must be specified");
                    return false;
                }
            }

            if (maximum > 0 && maximum < minimum)
            {
                await MessageBoxHelper.ShowMessageDialog("Maximum amount must be greater than or equal to minimum amount");
                return false;
            }

            foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
            {
                if (command.IsEnabled && this.existingCommand != command)
                {
                    if (commandStrings.Any(c => command.Commands.Contains(c)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a command that uses one of the chat triggers you have specified");
                        return false;
                    }
                }
            }

            foreach (SpinOutcome outcome in this.spinOutcomes)
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

            int userTotalChance = this.spinOutcomes.Select(o => o.UserChance).Sum();
            if (userTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined User Chance %'s do not equal 100");
                return false;
            }

            int subscriberTotalChance = this.spinOutcomes.Select(o => o.SubscriberChance).Sum();
            if (subscriberTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined Sub Chance %'s do not equal 100");
                return false;
            }

            int modTotalChance = this.spinOutcomes.Select(o => o.ModChance).Sum();
            if (modTotalChance != 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The combined Mod Chance %'s do not equal 100");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            UserCurrencyViewModel currency = (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem;
            int.TryParse(this.MinimumAmountTextBox.Text, out int minimum);
            int.TryParse(this.MaximumAmountTextBox.Text, out int maximum);

            RequirementViewModel requirements = this.Requirements.GetRequirements();
            requirements.Currency = (maximum > 0) ? new CurrencyRequirementViewModel(currency, minimum, maximum) :
                new CurrencyRequirementViewModel(currency, CurrencyRequirementTypeEnum.MinimumOnly, minimum);

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new SpinGameCommand(this.NameTextBox.Text, this.GetChatTriggers(), requirements, this.spinOutcomes.Select(o => o.GetGameOutcome())));
        }

        protected override Task OnLoaded()
        {
            this.Requirements.HideCurrencyRequirement();
            this.Requirements.HideThresholdRequirement();

            IEnumerable<UserCurrencyViewModel> currencies = ChannelSession.Settings.Currencies.Values;
            this.IsEnabled = (currencies.Count() > 0);
            this.CurrencyTypeComboBox.ItemsSource = currencies;

            this.OutcomesItemsControl.ItemsSource = this.spinOutcomes;

            if (this.existingCommand != null)
            {
                this.NameTextBox.Text = this.existingCommand.Name;
                this.ChatCommandTextBox.Text = this.existingCommand.CommandsString.Replace("!", "");
                this.CurrencyTypeComboBox.SelectedItem = this.existingCommand.Requirements.Currency.GetCurrency();
                this.MinimumAmountTextBox.Text = this.existingCommand.Requirements.Currency.RequiredAmount.ToString();
                this.MaximumAmountTextBox.Text = (this.existingCommand.Requirements.Currency.MaximumAmount > 0) ? this.existingCommand.Requirements.Currency.MaximumAmount.ToString() : string.Empty;

                foreach (GameOutcome outcome in this.existingCommand.Outcomes)
                {
                    this.spinOutcomes.Add(new SpinOutcome(outcome));
                }
            }
            else
            {
                this.NameTextBox.Text = "Spin";
                this.ChatCommandTextBox.Text = "spin";
                this.CurrencyTypeComboBox.SelectedIndex = 0;
                this.MinimumAmountTextBox.Text = "10";
                this.MaximumAmountTextBox.Text = "1000";

                this.spinOutcomes.Add(new SpinOutcome("Lose", this.CreateBasicChatCommand("Sorry @$username, you lost the spin!"), 0, 70, 70, 70));
                this.spinOutcomes.Add(new SpinOutcome("Win", this.CreateBasicChatCommand("Congrats @$username, you won $gamepayout!"), 200, 30, 30, 30));
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
            this.spinOutcomes.Remove(outcome);
        }

        private void AddOutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.spinOutcomes.Add(new SpinOutcome("", this.CreateBasicChatCommand("@$username")));
        }

        private CustomCommand CreateBasicChatCommand(string message)
        {
            CustomCommand command = new CustomCommand("Game Outcome");
            command.Actions.Add(new ChatAction(message));
            return command;
        }

        private IEnumerable<string> GetChatTriggers()
        {
            return this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(t => "!" + t);
        }
    }
}
