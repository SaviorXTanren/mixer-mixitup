using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    public class VendingMachineOutcome
    {
        public string Name { get; set; }

        public CustomCommand Command { get; set; }

        public int UserChance { get; set; }
        public int SubscriberChance { get; set; }
        public int ModChance { get; set; }

        public VendingMachineOutcome(string name, CustomCommand command, int userChance = 0, int subscriberChance = 0, int modChance = 0)
        {
            this.Name = name;
            this.Command = command;
            this.UserChance = userChance;
            this.SubscriberChance = subscriberChance;
            this.ModChance = modChance;
        }

        public VendingMachineOutcome(GameOutcome outcome) : this(outcome.Name, outcome.Command)
        {
            this.UserChance = outcome.RoleProbabilities[MixerRoleEnum.User];
            this.SubscriberChance = outcome.RoleProbabilities[MixerRoleEnum.Subscriber];
            this.ModChance = outcome.RoleProbabilities[MixerRoleEnum.Mod];
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
            return new GameOutcome(this.Name, 0, new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserChance }, { MixerRoleEnum.Subscriber, this.SubscriberChance },
                { MixerRoleEnum.Mod, this.ModChance } }, this.Command);
        }
    }

    /// <summary>
    /// Interaction logic for VendingMachineGameEditorControl.xaml
    /// </summary>
    public partial class VendingMachineGameEditorControl : GameEditorControlBase
    {
        private ObservableCollection<VendingMachineOutcome> outcomes = new ObservableCollection<VendingMachineOutcome>();

        private VendingMachineGameCommand existingCommand;

        public VendingMachineGameEditorControl()
        {
            InitializeComponent();
        }

        public VendingMachineGameEditorControl(VendingMachineGameCommand command)
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

            foreach (VendingMachineOutcome outcome in this.outcomes)
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
            ChannelSession.Settings.GameCommands.Add(new VendingMachineGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
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
                    this.outcomes.Add(new VendingMachineOutcome(outcome));
                }
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Vending Machine", "vend", CurrencyRequirementTypeEnum.RequiredAmount, 10);

                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.outcomes.Add(new VendingMachineOutcome("Nothing", this.CreateBasicChatCommand("@$username opened their capsule and found nothing..."), 40, 40, 40));

                CustomCommand currencyCommand = this.CreateBasicChatCommand("@$username opened their capsule and found 50 " + currency.Name + "!");
                currencyCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, 50.ToString()));
                this.outcomes.Add(new VendingMachineOutcome("50", currencyCommand, 30, 30, 30));

                CustomCommand overlayCommand = this.CreateBasicChatCommand("@$username opened their capsule and found a dancing Carlton!");
                overlayCommand.Actions.Add(new OverlayAction(new OverlayImageEffect("https://78.media.tumblr.com/1921bcd13e12643771410200a322cb0e/tumblr_ogs5bcHWUc1udh5n8o1_500.gif",
                    500, 500, OverlayEffectEntranceAnimationTypeEnum.FadeIn, OverlayEffectVisibleAnimationTypeEnum.None, OverlayEffectExitAnimationTypeEnum.FadeOut, 3, 50, 50)));
                this.outcomes.Add(new VendingMachineOutcome("Dancing Carlton", overlayCommand, 30, 30, 30));
            }

            return base.OnLoaded();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            VendingMachineOutcome outcome = (VendingMachineOutcome)button.DataContext;
            this.outcomes.Remove(outcome);
        }

        private void AddOutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.outcomes.Add(new VendingMachineOutcome("", this.CreateBasicChatCommand("@$username opened their capsule and found ")));
        }
    }
}
