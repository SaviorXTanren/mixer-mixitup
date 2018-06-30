using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    internal static class PreMadeGameCommandHelper
    {
        public static CustomCommand CreateCustomChatCommand(string text)
        {
            CustomCommand command = new CustomCommand("Game Custom Command");
            command.Actions.Add(new ChatAction(text));
            return command;
        }

        public static CustomCommand CreateCustomWhisperChatCommand(string text)
        {
            CustomCommand command = new CustomCommand("Game Custom Command");
            command.Actions.Add(new ChatAction(text, isWhisper: true));
            return command;
        }
    }

    public class SpinWheelGameCommand : SinglePlayerGameCommand
    {
        public SpinWheelGameCommand() { }

        public SpinWheelGameCommand(UserCurrencyViewModel currency)
            : base("Spin Wheel", new List<string>() { "spin" }, new RequirementViewModel(role: new RoleRequirementViewModel(MixerRoleEnum.User),
                cooldown: new CooldownRequirementViewModel(CooldownTypeEnum.Static, 15), currency: new CurrencyRequirementViewModel(currency, 1, 100)),
                  new List<GameOutcome>(), new List<GameOutcomeGroup>(), null)
        {
            this.Outcomes.Add(new GameOutcome("Win", PreMadeGameCommandHelper.CreateCustomChatCommand("@$username won $gamepayout $gamecurrencyname!")));
            
            GameOutcomeGroup userGroup = new GameOutcomeGroup(MixerRoleEnum.User);
            userGroup.Probabilities.Add(new GameOutcomeProbability(50, 25, "Win"));
            this.Groups.Add(userGroup);

            GameOutcomeGroup subscriberGroup = new GameOutcomeGroup(MixerRoleEnum.Subscriber);
            subscriberGroup.Probabilities.Add(new GameOutcomeProbability(50, 25, "Win"));
            this.Groups.Add(subscriberGroup);

            GameOutcomeGroup modGroup = new GameOutcomeGroup(MixerRoleEnum.Mod);
            modGroup.Probabilities.Add(new GameOutcomeProbability(50, 25, "Win"));
            this.Groups.Add(modGroup);

            this.LoseLeftoverCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Sorry @$username, you lost the spin!");
        }
    }

    public class HeistGameCommand : IndividualProbabilityGameCommand
    {
        public HeistGameCommand() { }

        public HeistGameCommand(UserCurrencyViewModel currency)
            : base("Heist", new List<string>() { "heist" }, new RequirementViewModel(role: new RoleRequirementViewModel(MixerRoleEnum.User),
                cooldown: new CooldownRequirementViewModel(CooldownTypeEnum.Static, 60), currency: new CurrencyRequirementViewModel(currency, CurrencyRequirementTypeEnum.MinimumOnly, 10)),
                  new List<GameOutcome>(), new List<GameOutcomeGroup>(), null, gameLength: 30, minimumParticipants: 2)
        {
            this.GameStartedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$username is starting a heist! Type \"!heist <BET>\" in chat to join in!");
            this.GameEndedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("The cops have shown up! Everyone from the heist scatters with whatever they were able to get...");
            this.UserJoinedCommand = PreMadeGameCommandHelper.CreateCustomWhisperChatCommand("You've hopped into the heist van! Let's see if you can pull it off...");
            this.NotEnoughUsersCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Apparently not enough people wanted to join @$username...");

            this.Outcomes.Add(new GameOutcome("Win", PreMadeGameCommandHelper.CreateCustomWhisperChatCommand("You made out successfully with $gamepayout!")));
            this.Outcomes.Add(new GameOutcome("Super Win", PreMadeGameCommandHelper.CreateCustomWhisperChatCommand("You made out like a SUPER BANDIT with $gamepayout!")));

            GameOutcomeGroup userGroup = new GameOutcomeGroup(MixerRoleEnum.User);
            userGroup.Probabilities.Add(new GameOutcomeProbability(40, 25, "Win"));
            userGroup.Probabilities.Add(new GameOutcomeProbability(10, 75, "Super Win"));
            this.Groups.Add(userGroup);

            GameOutcomeGroup subscriberGroup = new GameOutcomeGroup(MixerRoleEnum.Subscriber);
            subscriberGroup.Probabilities.Add(new GameOutcomeProbability(40, 25, "Win"));
            subscriberGroup.Probabilities.Add(new GameOutcomeProbability(10, 75, "Super Win"));
            this.Groups.Add(subscriberGroup);

            GameOutcomeGroup modGroup = new GameOutcomeGroup(MixerRoleEnum.Mod);
            modGroup.Probabilities.Add(new GameOutcomeProbability(40, 25, "Win"));
            modGroup.Probabilities.Add(new GameOutcomeProbability(10, 75, "Super Win"));
            this.Groups.Add(modGroup);

            this.LoseLeftoverCommand = PreMadeGameCommandHelper.CreateCustomWhisperChatCommand("Tough luck, the cops caught you before you could get anything good!");
        }
    }

    public class RussianRouletteGameCommand : OnlyOneWinnerGameCommand
    {
        public RussianRouletteGameCommand() { }

        public RussianRouletteGameCommand(UserCurrencyViewModel currency)
            : base("Russian Roulette", new List<string>() { "rr", "russian" }, new RequirementViewModel(role: new RoleRequirementViewModel(MixerRoleEnum.User),
                cooldown: new CooldownRequirementViewModel(CooldownTypeEnum.Static, 60), currency: new CurrencyRequirementViewModel(currency, 10)), gameLength: 30, minimumParticipants: 2)
        {
            this.GameStartedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$username is starting a game of Russian Roulette! Type \"!rr\" in chat to join in and win the whole pot!");
            this.GameEndedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Russian Roulette has ended...and @$username emerged victorious with $gamepayout $gamecurrencyname!");
            this.UserJoinedCommand = PreMadeGameCommandHelper.CreateCustomWhisperChatCommand("You've taken your seat at the circle, let's wait and see what happens!");
            this.NotEnoughUsersCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Apparently not enough people wanted to join @$username...");
        }
    }

    public class CharityGameCommand : UserCharityGameCommand
    {
        public CharityGameCommand() { }

        public CharityGameCommand(UserCurrencyViewModel currency)
            : base("Charity", new List<string>() { "charity" }, new RequirementViewModel(role: new RoleRequirementViewModel(MixerRoleEnum.User),
                cooldown: new CooldownRequirementViewModel(CooldownTypeEnum.Static, 15),
                currency: new CurrencyRequirementViewModel(currency, CurrencyRequirementTypeEnum.MinimumOnly, 1)), giveToRandomUser: true)
        {
            this.UserParticipatedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$username just received $gamepayout $gamecurrencyname randomly from @$gamestarterusername!");
        }
    }

    public class GiveGameCommand : UserCharityGameCommand
    {
        public GiveGameCommand() { }

        public GiveGameCommand(UserCurrencyViewModel currency)
            : base("Give", new List<string>() { "give" }, new RequirementViewModel(role: new RoleRequirementViewModel(MixerRoleEnum.User),
                cooldown: new CooldownRequirementViewModel(CooldownTypeEnum.Static, 15),
                currency: new CurrencyRequirementViewModel(currency, CurrencyRequirementTypeEnum.MinimumOnly, 1)), giveToRandomUser: false)
        {
            this.UserParticipatedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$gamestarterusername just gave $gamepayout $gamecurrencyname to @$username!");
        }
    }
}
