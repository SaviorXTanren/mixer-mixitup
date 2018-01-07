using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    internal static class PreMadeGameCommandHelper
    {
        public static CustomCommand CreateCustomChatCommand(string text, bool isWhisper = false)
        {
            CustomCommand command = new CustomCommand("Game Custom Command");
            command.Actions.Add(new ChatAction(text, isWhisper, false));
            return command;
        }
    }

    public class RouletteGameCommand : SinglePlayerGameCommand
    {
        public RouletteGameCommand(UserCurrencyViewModel currency)
        {
            this.Name = "Roulette";
            this.Commands = new List<string>() { "spin", "roulette" };
            this.Permissions = UserRole.User;
            this.Cooldown = 15;
            this.CurrencyRequirement = new UserCurrencyRequirementViewModel(currency, 1, 100);
            this.ResultProbabilities.Add(new GameResultProbability(37.5, "$gamebet * 2", PreMadeGameCommandHelper.CreateCustomChatCommand("Congrats @$username, you won the spin and got $gamepayout $gamecurrencyname!")));
            this.ResultProbabilities.Add(new GameResultProbability(12.5, "$gamebet * 3", PreMadeGameCommandHelper.CreateCustomChatCommand("Congrats @$username, you won the BONUS spin and got $gamepayout $gamecurrencyname!")));
            this.ResultProbabilities.Add(new GameResultProbability(50, null, PreMadeGameCommandHelper.CreateCustomChatCommand("Sorry @$username, you lost the spin!")));
        }
    }

    public class RussianRouletteGameCommand : MultiPlayerGameCommand
    {
        public RussianRouletteGameCommand(UserCurrencyViewModel currency)
        {
            this.Name = "Russian Roulette";
            this.Commands = new List<string>() { "rr", "russian" };
            this.Permissions = UserRole.User;
            this.Cooldown = 30;
            this.CurrencyRequirement = new UserCurrencyRequirementViewModel(currency, 10);

            this.GameLength = 30;
            this.MinimumParticipants = 2;
            this.ResultType = GameResultType.SelectRandomEnteredUser;

            this.GameStartedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$username starting a game of Russian Roulette! Type \"!rr <BET>\" in chat to join in and win the whole pot!");
            this.GameEndedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Russian Roulette has ended...and @$username emerged victorious with $gamepayout $gamecurrencyname!");
            this.UserJoinedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("You've taken your seat at the circle, let's wait and see what happens!", isWhisper: true);
            this.NotEnoughUsersCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Apparently not enough people wanted to join @$username...");

            this.ResultProbabilities.Add(new GameResultProbability(100, "$gametotalbets * 2", null));
        }
    }

    public class HeistGameCommand : MultiPlayerGameCommand
    {
        public HeistGameCommand(UserCurrencyViewModel currency)
        {
            this.Name = "Heist";
            this.Commands = new List<string>() { "heist" };
            this.Permissions = UserRole.User;
            this.Cooldown = 30;
            this.CurrencyRequirement = new UserCurrencyRequirementViewModel(currency, 10, 1000);

            this.GameLength = 30;
            this.MinimumParticipants = 2;
            this.ResultType = GameResultType.IndividualUserProbability;

            this.GameStartedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$username is starting a heist! Type \"!heist <BET>\" in chat to join in!");
            this.GameEndedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("The cops have shown up! Everyone from the heist scatters with whatever they were able to get...");
            this.UserJoinedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("You've hopped into the heist van! Let's see if you can pull it off...", isWhisper: true);
            this.NotEnoughUsersCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("Apparently not enough people wanted to join @$username...");

            this.ResultProbabilities.Add(new GameResultProbability(10, "$gamebet * 3", PreMadeGameCommandHelper.CreateCustomChatCommand("You made it out like a SUPER BANDIT with $gamepayout!", isWhisper: true)));
            this.ResultProbabilities.Add(new GameResultProbability(40, "$gamebet * 2", PreMadeGameCommandHelper.CreateCustomChatCommand("You made it out successfully with $gamepayout!", isWhisper: true)));
            this.ResultProbabilities.Add(new GameResultProbability(50, null, PreMadeGameCommandHelper.CreateCustomChatCommand("Tough luck, you walked away empty-handed!", isWhisper: true)));
        }
    }

    public class CharityGameCommand : MultiPlayerGameCommand
    {
        public CharityGameCommand(UserCurrencyViewModel currency)
        {
            this.Name = "Charity";
            this.Commands = new List<string>() { "charity" };
            this.Permissions = UserRole.User;
            this.Cooldown = 15;
            this.CurrencyRequirement = new UserCurrencyRequirementViewModel(currency, 10);

            this.GameLength = 0;
            this.MinimumParticipants = 1;
            this.ResultType = GameResultType.SelectRandomChatUser;

            this.GameEndedCommand = PreMadeGameCommandHelper.CreateCustomChatCommand("@$gamestarterusername just gave $gametotalbets $gamecurrencyname to @$username, what a generous soul!");

            this.ResultProbabilities.Add(new GameResultProbability(100, "$gametotalbets", null));
        }
    }
}
