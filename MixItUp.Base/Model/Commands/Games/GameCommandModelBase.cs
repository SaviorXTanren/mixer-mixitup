using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    public enum GameCommandTypeEnum
    {
        Bet,
        Bid,
        CoinPusher,
        Duel,
        Hangman,
        Heist,
        Hitman,
        HotPotato,
        LockBox,
        Roulette,
        RussianRoulette,
        SlotMachine,
        Spin,
        Steal,
        TreasureDefense,
        Trivia,
        Volcano,
        WordScramble
    }

    [Flags]
    public enum GamePlayerSelectionType
    {
        [Obsolete]
        None = 0,
        Targeted = 1,
        Random = 2,
    }

    [DataContract]
    public class RoleProbabilityPayoutModel
    {
        [DataMember]
        public UserRoleEnum UserRole { get; set; }

        [DataMember]
        public int Probability { get; set; }

        [DataMember]
        public double Payout { get; set; }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability) : this(role, probability, 0) { }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability, double payout)
        {
            this.UserRole = role;
            this.Probability = probability;
            this.Payout = payout;
        }

        [Obsolete]
        public RoleProbabilityPayoutModel() { }
    }

    [DataContract]
    public class GameOutcomeModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> UserRoleProbabilityPayouts { get; set; } = new Dictionary<UserRoleEnum, RoleProbabilityPayoutModel>();

        [DataMember]
        public CustomCommandModel Command { get; set; }

        public GameOutcomeModel(string name, Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> roleProbabilityPayouts, CustomCommandModel command)
        {
            this.Name = name;
            this.UserRoleProbabilityPayouts = roleProbabilityPayouts;
            this.Command = command;
        }

        [Obsolete]
        public GameOutcomeModel() { }

        public RoleProbabilityPayoutModel GetRoleProbabilityPayout(UserV2ViewModel user)
        {
            var roleProbabilities = this.UserRoleProbabilityPayouts.Where(kvp => user.MeetsRole(kvp.Key)).OrderByDescending(kvp => kvp.Key);
            if (roleProbabilities.Count() > 0)
            {
                return roleProbabilities.FirstOrDefault().Value;
            }
            return null;
        }

        public double GetPayoutMultiplier(UserV2ViewModel user)
        {
            RoleProbabilityPayoutModel roleProbabilityPayout = this.GetRoleProbabilityPayout(user);
            if (roleProbabilityPayout != null)
            {
                return roleProbabilityPayout.Payout;
            }
            return 0;
        }
    }

    [DataContract]
    public abstract class GameCommandModelBase : ChatCommandModel
    {
        public const string GameBetSpecialIdentifier = "gamebet";
        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameAllPayoutSpecialIdentifier = "gameallpayout";
        public const string GameWinnersSpecialIdentifier = "gamewinners";
        public const string GameWinnersCountSpecialIdentifier = "gamewinnerscount";
        public const string GameTotalAmountSpecialIdentifier = "gametotalamount";

        public static readonly HashSet<string> DefaultWords = new HashSet<string>() { "ABLE", "ACCEPTABLE", "ACCORDING", "ACCURATE", "ACTION", "ACTIVE", "ACTUAL", "ADDITIONAL", "ADMINISTRATIVE", "ADULT", "AFRAID", "AFTER", "AFTERNOON", "AGENT", "AGGRESSIVE", "AGO", "AIRLINE", "ALIVE", "ALL", "ALONE", "ALTERNATIVE", "AMAZING", "ANGRY", "ANIMAL", "ANNUAL", "ANOTHER", "ANXIOUS", "ANY", "APART", "APPROPRIATE", "ASLEEP", "AUTOMATIC", "AVAILABLE", "AWARE", "AWAY", "BACKGROUND", "BASIC", "BEAUTIFUL", "BEGINNING", "BEST", "BETTER", "BIG", "BITTER", "BORING", "BORN", "BOTH", "BRAVE", "BRIEF", "BRIGHT", "BRILLIANT", "BROAD", "BROWN", "BUDGET", "BUSINESS", "BUSY", "CALM", "CAPABLE", "CAPITAL", "CAR", "CAREFUL", "CERTAIN", "CHANCE", "CHARACTER", "CHEAP", "CHEMICAL", "CHICKEN", "CHOICE", "CIVIL", "CLASSIC", "CLEAN", "CLEAR", "CLOSE", "COLD", "COMFORTABLE", "COMMERCIAL", "COMMON", "COMPETITIVE", "COMPLETE", "COMPLEX", "COMPREHENSIVE", "CONFIDENT", "CONNECT", "CONSCIOUS", "CONSISTENT", "CONSTANT", "CONTENT", "COOL", "CORNER", "CORRECT", "CRAZY", "CREATIVE", "CRITICAL", "CULTURAL", "CURIOUS", "CURRENT", "CUTE", "DANGEROUS", "DARK", "DAUGHTER", "DAY", "DEAD", "DEAR", "DECENT", "DEEP", "DEPENDENT", "DESIGNER", "DESPERATE", "DIFFERENT", "DIFFICULT", "DIRECT", "DIRTY", "DISTINCT", "DOUBLE", "DOWNTOWN", "DRAMATIC", "DRESS", "DRUNK", "DRY", "DUE", "EACH", "EAST", "EASTERN", "EASY", "ECONOMY", "EDUCATIONAL", "EFFECTIVE", "EFFICIENT", "EITHER", "ELECTRICAL", "ELECTRONIC", "EMBARRASSED", "EMERGENCY", "EMOTIONAL", "EMPTY", "ENOUGH", "ENTIRE", "ENVIRONMENTAL", "EQUAL", "EQUIVALENT", "EVEN", "EVENING", "EVERY", "EXACT", "EXCELLENT", "EXCITING", "EXISTING", "EXPENSIVE", "EXPERT", "EXPRESS", "EXTENSION", "EXTERNAL", "EXTRA", "EXTREME", "FAIR", "FAMILIAR", "FAMOUS", "FAR", "FAST", "FAT", "FEDERAL", "FEELING", "FEMALE", "FEW", "FINAL", "FINANCIAL", "FINE", "FIRM", "FIRST", "FIT", "FLAT", "FOREIGN", "FORMAL", "FORMER", "FORWARD", "FREE", "FREQUENT", "FRESH", "FRIENDLY", "FRONT", "FULL", "FUN", "FUNNY", "FUTURE", "GAME", "GENERAL", "GLAD", "GLASS", "GLOBAL", "GOLD", "GOOD", "GRAND", "GREAT", "GREEN", "GROSS", "GUILTY", "HAPPY", "HARD", "HEAD", "HEALTHY", "HEAVY", "HELPFUL", "HIGH", "HIS", "HISTORICAL", "HOLIDAY", "HOME", "HONEST", "HORROR", "HOT", "HOUR", "HOUSE", "HUGE", "HUMAN", "HUNGRY", "IDEAL", "ILL", "ILLEGAL", "IMMEDIATE", "IMPORTANT", "IMPOSSIBLE", "IMPRESSIVE", "INCIDENT", "INDEPENDENT", "INDIVIDUAL", "INEVITABLE", "INFORMAL", "INITIAL", "INNER", "INSIDE", "INTELLIGENT", "INTERESTING", "INTERNAL", "INTERNATIONAL", "JOINT", "JUNIOR", "JUST", "KEY", "KIND", "KITCHEN", "KNOWN", "LARGE", "LAST", "LATE", "LATTER", "LEADING", "LEAST", "LEATHER", "LEFT", "LEGAL", "LESS", "LEVEL", "LIFE", "LITTLE", "LIVE", "LIVING", "LOCAL", "LOGICAL", "LONELY", "LONG", "LOOSE", "LOST", "LOUD", "LOW", "LOWER", "LUCKY", "MAD", "MAIN", "MAJOR", "MALE", "MANY", "MASSIVE", "MASTER", "MATERIAL", "MAXIMUM", "MEAN", "MEDICAL", "MEDIUM", "MENTAL", "MIDDLE", "MINIMUM", "MINOR", "MINUTE", "MISSION", "MOBILE", "MONEY", "MORE", "MOST", "MOTHER", "MOTOR", "MOUNTAIN", "MUCH", "NARROW", "NASTY", "NATIONAL", "NATIVE", "NATURAL", "NEARBY", "NEAT", "NECESSARY", "NEGATIVE", "NEITHER", "NERVOUS", "NEW", "NEXT", "NICE", "NO", "NORMAL", "NORTH", "NOVEL", "NUMEROUS", "OBJECTIVE", "OBVIOUS", "ODD", "OFFICIAL", "OK", "OLD", "ONE", "ONLY", "OPEN", "OPENING", "OPPOSITE", "ORDINARY", "ORIGINAL", "OTHER", "OTHERWISE", "OUTSIDE", "OVER", "OVERALL", "OWN", "PARKING", "PARTICULAR", "PARTY", "PAST", "PATIENT", "PERFECT", "PERIOD", "PERSONAL", "PHYSICAL", "PLANE", "PLASTIC", "PLEASANT", "PLENTY", "PLUS", "POLITICAL", "POOR", "POPULAR", "POSITIVE", "POSSIBLE", "POTENTIAL", "POWERFUL", "PRACTICAL", "PREGNANT", "PRESENT", "PRETEND", "PRETTY", "PREVIOUS", "PRIMARY", "PRIOR", "PRIVATE", "PRIZE", "PROFESSIONAL", "PROOF", "PROPER", "PROUD", "PSYCHOLOGICAL", "PUBLIC", "PURE", "PURPLE", "QUICK", "QUIET", "RARE", "RAW", "READY", "REAL", "REALISTIC", "REASONABLE", "RECENT", "RED", "REGULAR", "RELATIVE", "RELEVANT", "REMARKABLE", "REMOTE", "REPRESENTATIVE", "RESIDENT", "RESPONSIBLE", "RICH", "RIGHT", "ROUGH", "ROUND", "ROUTINE", "ROYAL", "SAD", "SAFE", "SALT", "SAME", "SAVINGS", "SCARED", "SEA", "SECRET", "SECURE", "SELECT", "SENIOR", "SENSITIVE", "SEPARATE", "SERIOUS", "SEVERAL", "SEVERE", "SEXUAL", "SHARP", "SHORT", "SHOT", "SICK", "SIGNAL", "SIGNIFICANT", "SILLY", "SILVER", "SIMILAR", "SIMPLE", "SINGLE", "SLIGHT", "SLOW", "SMALL", "SMART", "SMOOTH", "SOFT", "SOLID", "SOME", "SORRY", "SOUTH", "SOUTHERN", "SPARE", "SPECIAL", "SPECIALIST", "SPECIFIC", "SPIRITUAL", "SQUARE", "STANDARD", "STATUS", "STILL", "STOCK", "STRAIGHT", "STRANGE", "STREET", "STRICT", "STRONG", "STUPID", "SUBJECT", "SUBSTANTIAL", "SUCCESSFUL", "SUCH", "SUDDEN", "SUFFICIENT", "SUITABLE", "SUPER", "SURE", "SUSPICIOUS", "SWEET", "SWIMMING", "TALL", "TECHNICAL", "TEMPORARY", "TERRIBLE", "THAT", "THEN", "THESE", "THICK", "THIN", "THINK", "THIS", "TIGHT", "TIME", "TINY", "TOP", "TOTAL", "TOUGH", "TRADITIONAL", "TRAINING", "TRICK", "TYPICAL", "UGLY", "UNABLE", "UNFAIR", "UNHAPPY", "UNIQUE", "UNITED", "UNLIKELY", "UNUSUAL", "UPPER", "UPSET", "UPSTAIRS", "USED", "USEFUL", "USUAL", "VALUABLE", "VARIOUS", "VAST", "VEGETABLE", "VISIBLE", "VISUAL", "WARM", "WASTE", "WEAK", "WEEKLY", "WEIRD", "WEST", "WESTERN", "WHAT", "WHICH", "WHITE", "WHOLE", "WIDE", "WILD", "WILLING", "WINE", "WINTER", "WISE", "WONDERFUL", "WOODEN", "WORK", "WORKING", "WORTH", "WRONG", "YELLOW", "YOUNG" };

        private static readonly HashSet<Type> RequirementSkipTypes = new HashSet<Type>() { typeof(CooldownRequirementModel) };

        [DataMember]
        public GameCommandTypeEnum GameType { get; set; }

        public GameCommandModelBase(string name, HashSet<string> triggers, GameCommandTypeEnum gameType)
            : base(name, CommandTypeEnum.Game, triggers, includeExclamation: true, wildcards: false)
        {
            this.GameType = gameType;
        }

        [Obsolete]
        public GameCommandModelBase() : base() { }

        public override bool HasCustomRun { get { return true; } }

        public virtual IEnumerable<CommandModelBase> GetInnerCommands() { return new List<CommandModelBase>(); }

        public override HashSet<ActionTypeEnum> GetActionTypesInCommand(HashSet<Guid> commandIDs = null)
        {
            HashSet<ActionTypeEnum> actionTypes = new HashSet<ActionTypeEnum>() { ActionTypeEnum.Chat };

            if (commandIDs == null)
            {
                commandIDs = new HashSet<Guid>();
            }

            if (commandIDs.Contains(this.ID))
            {
                return actionTypes;
            }
            commandIDs.Add(this.ID);

            foreach (CommandModelBase subCommand in this.GetInnerCommands())
            {
                foreach (ActionTypeEnum subActionType in subCommand.GetActionTypesInCommand(commandIDs))
                {
                    actionTypes.Add(subActionType);
                }
            }

            return actionTypes;
        }

        protected async Task RefundCooldown(CommandParametersModel parameters) { await this.Requirements.Requirements.FirstOrDefault(r => r is CooldownRequirementModel).Refund(parameters); }

        protected async Task PerformCooldown(CommandParametersModel parameters) { await this.Requirements.Requirements.FirstOrDefault(r => r is CooldownRequirementModel).Perform(parameters); }

        protected async Task SetSelectedUser(GamePlayerSelectionType selectionType, CommandParametersModel parameters)
        {
            if (selectionType.HasFlag(GamePlayerSelectionType.Targeted))
            {
                await parameters.SetTargetUser();
                if (parameters.TargetUser != null)
                {
                    if (parameters.IsTargetUserSelf)
                    {
                        parameters.TargetUser = null;
                    }
                    else if (!ServiceManager.Get<UserService>().IsUserActive(parameters.TargetUser.ID))
                    {
                        parameters.TargetUser = null;
                        return;
                    }
                }
            }

            if (parameters.TargetUser == null && selectionType.HasFlag(GamePlayerSelectionType.Random))
            {
                parameters.TargetUser = this.GetRandomUser(parameters);
            }
        }

        protected UserV2ViewModel GetRandomUser(CommandParametersModel parameters)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            int betAmount = this.GetPrimaryBetAmount(parameters);
            if (currencyRequirement != null && betAmount > 0)
            {
                string currencyName = currencyRequirement.Currency?.Name;
                List<UserV2ViewModel> users = new List<UserV2ViewModel>(ServiceManager.Get<UserService>().GetActiveUsers(parameters.Platform).Shuffle());
                users.Remove(parameters.User);
                foreach (UserV2ViewModel user in users)
                {
                    if (!user.IsSpecialtyExcluded && currencyRequirement.Currency.HasAmount(user, betAmount))
                    {
                        return user;
                    }
                }
                return null;
            }
            else
            {
                return ServiceManager.Get<UserService>().GetActiveUsers(parameters.Platform, excludeSpecialtyExcluded: true).Random();
            }
        }

        public override async Task PreRun(CommandParametersModel parameters)
        {
            await base.PreRun(parameters);

            parameters.SpecialIdentifiers[GameCommandModelBase.GameBetSpecialIdentifier] = this.GetPrimaryBetAmount(parameters).ToString();
        }

        public override void TrackTelemetry() { ServiceManager.Get<ITelemetryService>().TrackCommand(this.Type, this.GetType().ToString()); }

        protected CurrencyRequirementModel GetPrimaryCurrencyRequirement() { return this.Requirements.Currency.FirstOrDefault(); }

        protected void SetPrimaryCurrencyRequirementArgumentIndex(int argumentIndex)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            if (currencyRequirement != null)
            {
                currencyRequirement.ArgumentIndex = argumentIndex;
            }
        }

        protected int GetPrimaryBetAmount(CommandParametersModel parameters)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            return (currencyRequirement != null) ? currencyRequirement.GetAmount(parameters) : 0;
        }

        protected async Task<bool> ValidateTargetUserPrimaryBetAmount(CommandParametersModel parameters)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            int betAmount = this.GetPrimaryBetAmount(parameters);
            if (currencyRequirement != null && betAmount > 0)
            {
                string currencyName = currencyRequirement.Currency?.Name;
                if (currencyRequirement.Currency.HasAmount(parameters.TargetUser, betAmount))
                {
                    return true;
                }

                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCommandTargetUserInvalidAmount, betAmount, currencyName), parameters);
                return false;
            }
            return true;
        }

        protected bool ValidatePrimaryCurrencyAmount(CommandParametersModel parameters, int amount)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            return (currencyRequirement != null) ? currencyRequirement.Currency.HasAmount(parameters.User, amount) : false;
        }

        protected GameOutcomeModel SelectRandomOutcome(UserV2ViewModel user, IEnumerable<GameOutcomeModel> outcomes)
        {
            int randomNumber = this.GenerateProbability();
            int cumulativeOutcomeProbability = 0;
            foreach (GameOutcomeModel outcome in outcomes)
            {
                RoleProbabilityPayoutModel roleProbabilityPayout = outcome.GetRoleProbabilityPayout(user);
                if (roleProbabilityPayout != null)
                {
                    if (cumulativeOutcomeProbability < randomNumber && randomNumber <= (cumulativeOutcomeProbability + roleProbabilityPayout.Probability))
                    {
                        return outcome;
                    }
                    cumulativeOutcomeProbability += roleProbabilityPayout.Probability;
                }
            }
            return outcomes.Last();
        }

        protected async Task<int> RunOutcome(CommandParametersModel parameters, GameOutcomeModel outcome)
        {
            int payout = this.PerformPrimaryMultiplierPayout(parameters, outcome.GetPayoutMultiplier(parameters.User));
            parameters.SpecialIdentifiers[GameCommandModelBase.GameBetSpecialIdentifier] = this.GetPrimaryBetAmount(parameters).ToString();
            parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();
            if (outcome.Command != null)
            {
                await this.RunSubCommand(outcome.Command, parameters);
            }

            return payout;
        }

        protected async Task RunSubCommand(CommandModelBase command, CommandParametersModel parameters)
        {
            await ServiceManager.Get<CommandService>().Queue(new CommandInstanceModel(command, parameters)
            {
                ShowInUI = false
            });
        }

        protected void PerformPrimarySetPayout(UserV2ViewModel user, int payout)
        {
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            if (currencyRequirement != null)
            {
                currencyRequirement.AddSubtractAmount(user, payout);
            }
        }

        protected int PerformPrimaryMultiplierPayout(CommandParametersModel parameters, double payoutPercentage)
        {
            int payout = 0;
            CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
            if (currencyRequirement != null)
            {
                payout = (int)(currencyRequirement.GetAmount(parameters) * payoutPercentage);
                currencyRequirement.AddSubtractAmount(parameters.User, payout);
            }
            return payout;
        }

        protected void SetGameWinners(CommandParametersModel parameters, IEnumerable<CommandParametersModel> winners)
        {
            parameters.SpecialIdentifiers[GameCommandModelBase.GameWinnersCountSpecialIdentifier] = winners.Count().ToString();
            parameters.SpecialIdentifiers[GameCommandModelBase.GameWinnersSpecialIdentifier] = string.Join(", ", winners.Select(u => "@" + u.User.Username));
        }

        protected async Task<string> GetRandomWord(string customWordsFilePath)
        {
            HashSet<string> wordsToUse = GameCommandModelBase.DefaultWords;
            if (!string.IsNullOrEmpty(customWordsFilePath) && ServiceManager.Get<IFileService>().FileExists(customWordsFilePath))
            {
                string fileData = await ServiceManager.Get<IFileService>().ReadFile(customWordsFilePath);
                if (!string.IsNullOrEmpty(fileData))
                {
                    wordsToUse = new HashSet<string>();
                    foreach (string split in fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordsToUse.Add(split);
                    }
                }
            }
            return wordsToUse.Random().ToUpper();
        }

        protected int GenerateProbability() { return RandomHelper.GenerateProbability(); }

        protected int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(maxValue); }

        protected int GenerateRandomNumber(int minValue, int maxValue) { return RandomHelper.GenerateRandomNumber(minValue, maxValue); }

        protected int GenerateRandomNumber(int total, double minimumPercentage, double maximumPercentage)
        {
            double amount = Convert.ToDouble(total);
            int minAmount = Convert.ToInt32(amount * minimumPercentage);
            int maxAmount = Convert.ToInt32(amount * maximumPercentage);
            return this.GenerateRandomNumber(minAmount, maxAmount);
        }

        protected async Task DelayNoThrow(int millisecondDelay, CancellationToken token)
        {
            try
            {
                await Task.Delay(millisecondDelay, token);
            }
            catch { }
        }
    }
}
