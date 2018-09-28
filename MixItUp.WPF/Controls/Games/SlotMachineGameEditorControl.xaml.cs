using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Games
{
    public class SlotMachineOutcome
    {
        public string Symbol1 { get; set; }
        public string Symbol2 { get; set; }
        public string Symbol3 { get; set; }

        public CustomCommand Command { get; set; }

        public double UserPayout { get; set; }
        public double SubscriberPayout { get; set; }
        public double ModPayout { get; set; }

        public bool AnyOrder { get; set; }

        public SlotMachineOutcome(string symbol1, string symbol2, string symbol3, CustomCommand command, double userPayout = 0, double subscriberPayout = 0, double modPayout = 0, bool anyOrder = false)
        {
            this.Symbol1 = symbol1;
            this.Symbol2 = symbol2;
            this.Symbol3 = symbol3;
            this.AnyOrder = anyOrder;
            this.Command = command;
            this.UserPayout = userPayout;
            this.SubscriberPayout = subscriberPayout;
            this.ModPayout = modPayout;
        }

        public SlotMachineOutcome(SlotsGameOutcome outcome)
        {
            this.Symbol1 = outcome.Symbol1;
            this.Symbol2 = outcome.Symbol2;
            this.Symbol3 = outcome.Symbol3;
            this.AnyOrder = outcome.AnyOrder;
            this.Command = outcome.Command;
            this.UserPayout = outcome.RolePayouts[MixerRoleEnum.User] * 100.0;
            this.SubscriberPayout = outcome.RolePayouts[MixerRoleEnum.Subscriber] * 100.0;
            this.ModPayout = outcome.RolePayouts[MixerRoleEnum.Mod] * 100.0;
        }

        public string UserPayoutString
        {
            get { return this.UserPayout.ToString(); }
            set { this.UserPayout = this.GetPercentageFromString(value); }
        }

        public string SubscriberPayoutString
        {
            get { return this.SubscriberPayout.ToString(); }
            set { this.SubscriberPayout = this.GetPercentageFromString(value); }
        }

        public string ModPayoutString
        {
            get { return this.ModPayout.ToString(); }
            set { this.ModPayout = this.GetPercentageFromString(value); }
        }

        private int GetPercentageFromString(string value)
        {
            if (int.TryParse(value, out int percentage) && percentage >= 0)
            {
                return percentage;
            }
            return 0;
        }

        public SlotsGameOutcome GetGameOutcome()
        {
            return new SlotsGameOutcome(this.Symbol1 + " " + this.Symbol2 + " " + this.Symbol3, this.Symbol1, this.Symbol2, this.Symbol3,
                new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, this.UserPayout / 100.0 }, { MixerRoleEnum.Subscriber, this.SubscriberPayout / 100.0 }, { MixerRoleEnum.Mod, this.ModPayout / 100.0 } },
                this.Command, this.AnyOrder);
        }
    }

    /// <summary>
    /// Interaction logic for SlotMachineGameEditorControl.xaml
    /// </summary>
    public partial class SlotMachineGameEditorControl : GameEditorControlBase
    {
        private static readonly string SymbolsMustLandInOrderTooltip = "For an outcome to occur, the symbols must" + Environment.NewLine +
                                                                        "match the order they are shown exactly.";

        private CustomCommand failureOutcomeCommand { get; set; }

        private ObservableCollection<SlotMachineOutcome> outcomes = new ObservableCollection<SlotMachineOutcome>();

        private SlotMachineGameCommand existingCommand;

        public SlotMachineGameEditorControl()
        {
            InitializeComponent();
        }

        public SlotMachineGameEditorControl(SlotMachineGameCommand command)
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

            if (string.IsNullOrEmpty(this.SymbolsTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("No slot symbols have been entered");
                return false;
            }

            List<string> symbolsList = new List<string>(this.SymbolsTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            if (symbolsList.Count < 2)
            {
                await MessageBoxHelper.ShowMessageDialog("At least 2 slots symbols must be entered");
                return false;
            }

            if (symbolsList.GroupBy(s => s).Any(g => g.Count() > 1))
            {
                await MessageBoxHelper.ShowMessageDialog("All slot symbols must be unique");
                return false;
            }

            HashSet<string> symbols = new HashSet<string>(symbolsList);

            foreach (SlotMachineOutcome outcome in this.outcomes)
            {
                if (string.IsNullOrEmpty(outcome.Symbol1) || string.IsNullOrEmpty(outcome.Symbol2) || string.IsNullOrEmpty(outcome.Symbol3))
                {
                    await MessageBoxHelper.ShowMessageDialog("An outcome is missing its symbols");
                    return false;
                }

                if (!symbols.Contains(outcome.Symbol1) || !symbols.Contains(outcome.Symbol2) || !symbols.Contains(outcome.Symbol3))
                {
                    await MessageBoxHelper.ShowMessageDialog("An outcome contains a symbol not found in the set of all slot symbols");
                    return false;
                }

                if (outcome.UserPayout < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The User Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.SubscriberPayout < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Subscriber Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.ModPayout < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Mod Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("An outcome is missing a command");
                    return false;
                }
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            List<string> symbolsList = new List<string>(this.SymbolsTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));

            GameCommandBase newCommand = new SlotMachineGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), this.outcomes.Select(o => o.GetGameOutcome()), symbolsList, this.failureOutcomeCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        protected override Task OnLoaded()
        {
            this.OutcomesItemsControl.ItemsSource = this.outcomes;

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);

                this.SymbolsTextBox.Text = string.Join(" ", this.existingCommand.AllSymbols);
                this.failureOutcomeCommand = this.existingCommand.FailureOutcomeCommand;

                foreach (GameOutcome outcome in this.existingCommand.Outcomes)
                {                  
                    this.outcomes.Add(new SlotMachineOutcome((SlotsGameOutcome)outcome));
                }
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Slot Machine", "slots", CurrencyRequirementTypeEnum.MinimumOnly, 10);

                this.SymbolsTextBox.Text = "X O $";

                this.failureOutcomeCommand = this.CreateBasicChatCommand("Result: $gameslotsoutcome - Looks like luck was not on your side. Better luck next time...", whisper: true);

                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.outcomes.Add(new SlotMachineOutcome("O", "O", "O", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 200, 200, 200));

                this.outcomes.Add(new SlotMachineOutcome("$", "O", "$", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 150, 150, 150, anyOrder: true));

                this.outcomes.Add(new SlotMachineOutcome("X", "$", "O", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 500, 500, 500, anyOrder: true));
            }

            this.FailureOutcomeCommandButtonsControl.DataContext = this.failureOutcomeCommand;

            return base.OnLoaded();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SlotMachineOutcome outcome = (SlotMachineOutcome)button.DataContext;
            this.outcomes.Remove(outcome);
        }

        private void AddOutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            UserCurrencyViewModel currency = null;

            RequirementViewModel requirements = this.CommandDetailsControl.GetRequirements();
            if (requirements.Currency != null)
            {
                currency = requirements.Currency.GetCurrency();
            }

            if (currency == null)
            {
                currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
            }

            this.outcomes.Add(new SlotMachineOutcome(null, null, null, this.CreateBasicChatCommand("Result: $gameslotsoutcome - $@username walks away with $gamepayout " + currency.Name + "!")));
        }

        private void AnyOrderToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggle = (ToggleButton)sender;
            SlotMachineOutcome outcome = (SlotMachineOutcome)toggle.DataContext;
            outcome.AnyOrder = toggle.IsChecked.GetValueOrDefault();
        }
    }
}
