using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class VendingMachineOutcome : UIViewModelBase
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
            set { this.UserChance = this.GetPositiveIntFromString(value); }
        }

        public string SubscriberChanceString
        {
            get { return this.SubscriberChance.ToString(); }
            set { this.SubscriberChance = this.GetPositiveIntFromString(value); }
        }

        public string ModChanceString
        {
            get { return this.ModChance.ToString(); }
            set { this.ModChance = this.GetPositiveIntFromString(value); }
        }

        public GameOutcome GetGameOutcome()
        {
            return new GameOutcome(this.Name, 0, new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserChance }, { MixerRoleEnum.Subscriber, this.SubscriberChance },
                { MixerRoleEnum.Mod, this.ModChance } }, this.Command);
        }
    }

    public class VendingMachineGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public ObservableCollection<VendingMachineOutcome> Outcomes { get; set; } = new ObservableCollection<VendingMachineOutcome>();

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        private VendingMachineGameCommand existingCommand;

        public VendingMachineGameEditorControlViewModel(UserCurrencyViewModel currency)
            : this()
        {
            this.Outcomes.Add(new VendingMachineOutcome("Nothing", this.CreateBasicChatCommand("@$username opened their capsule and found nothing..."), 40, 40, 40));

            CustomCommand currencyCommand = this.CreateBasicChatCommand("@$username opened their capsule and found 50 " + currency.Name + "!");
            currencyCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, 50.ToString()));
            this.Outcomes.Add(new VendingMachineOutcome("50", currencyCommand, 30, 30, 30));

            CustomCommand overlayCommand = this.CreateBasicChatCommand("@$username opened their capsule and found a dancing Carlton!");
            OverlayImageItemModel overlayImage = new OverlayImageItemModel("https://78.media.tumblr.com/1921bcd13e12643771410200a322cb0e/tumblr_ogs5bcHWUc1udh5n8o1_500.gif", 500, 500);
            overlayImage.Position = new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 50, 0);
            overlayImage.Effects = new OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum.FadeIn, OverlayItemEffectVisibleAnimationTypeEnum.None, OverlayItemEffectExitAnimationTypeEnum.FadeOut, 3);
            overlayCommand.Actions.Add(new OverlayAction(ChannelSession.Services.OverlayServers.DefaultOverlayName, overlayImage));

            this.Outcomes.Add(new VendingMachineOutcome("Dancing Carlton", overlayCommand, 30, 30, 30));
        }

        public VendingMachineGameEditorControlViewModel(VendingMachineGameCommand command)
            : this()
        {
            this.existingCommand = command;

            foreach (GameOutcome outcome in this.existingCommand.Outcomes)
            {
                this.Outcomes.Add(new VendingMachineOutcome(outcome));
            }
        }

        private VendingMachineGameEditorControlViewModel()
        {
            this.AddOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Add(new VendingMachineOutcome("", this.CreateBasicChatCommand("@$username opened their capsule and found ")));
                return Task.FromResult(0);
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Remove((VendingMachineOutcome)parameter);
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new VendingMachineGameCommand(name, triggers, requirements, this.Outcomes.Select(o => o.GetGameOutcome()));
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            foreach (VendingMachineOutcome outcome in this.Outcomes)
            {
                if (string.IsNullOrEmpty(outcome.Name))
                {
                    await DialogHelper.ShowMessage("An outcome is missing a name");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await DialogHelper.ShowMessage("An outcome is missing a command");
                    return false;
                }
            }

            int userTotalChance = this.Outcomes.Select(o => o.UserChance).Sum();
            if (userTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined User Chance %'s do not equal 100");
                return false;
            }

            int subscriberTotalChance = this.Outcomes.Select(o => o.SubscriberChance).Sum();
            if (subscriberTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined Sub Chance %'s do not equal 100");
                return false;
            }

            int modTotalChance = this.Outcomes.Select(o => o.ModChance).Sum();
            if (modTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined Mod Chance %'s do not equal 100");
                return false;
            }

            return true;
        }
    }
}
