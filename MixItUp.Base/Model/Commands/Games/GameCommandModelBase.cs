using MixItUp.Base.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
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
    [DataContract]
    public class RoleProbabilityPayoutModel
    {
        [DataMember]
        public UserRoleEnum Role { get; set; }

        [DataMember]
        public int Probability { get; set; }

        [DataMember]
        public double Payout { get; set; }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability) : this(role, probability, 0) { }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability, double payout)
        {
            this.Role = role;
            this.Probability = probability;
            this.Payout = payout;
        }

        private RoleProbabilityPayoutModel() { }
    }

    [DataContract]
    public class GameOutcomeModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> RoleProbabilityPayouts { get; set; }

        [DataMember]
        public CustomCommandModel Command { get; set; }

        public GameOutcomeModel(string name, Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> roleProbabilityPayouts, CustomCommandModel command)
        {
            this.Name = name;
            this.RoleProbabilityPayouts = roleProbabilityPayouts;
            this.Command = command;
        }

        protected GameOutcomeModel() { }

        public RoleProbabilityPayoutModel GetRoleProbabilityPayout(UserViewModel user)
        {
            var roleProbabilities = this.RoleProbabilityPayouts.Where(kvp => user.HasPermissionsTo(kvp.Key)).OrderByDescending(kvp => kvp.Key);
            if (roleProbabilities.Count() > 0)
            {
                return roleProbabilities.FirstOrDefault().Value;
            }
            return null;
        }

        public int GetPayout(UserViewModel user, int betAmount)
        {
            RoleProbabilityPayoutModel roleProbabilityPayout = this.GetRoleProbabilityPayout(user);
            if (roleProbabilityPayout != null)
            {
                return Convert.ToInt32(Convert.ToDouble(betAmount) * roleProbabilityPayout.Payout);
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
        public const string GameTotalAmountSpecialIdentifier = "gametotalamount";

        public static readonly HashSet<string> DefaultWords = new HashSet<string>() { "ABLE", "ACCEPTABLE", "ACCORDING", "ACCURATE", "ACTION", "ACTIVE", "ACTUAL", "ADDITIONAL", "ADMINISTRATIVE", "ADULT", "AFRAID", "AFTER", "AFTERNOON", "AGENT", "AGGRESSIVE", "AGO", "AIRLINE", "ALIVE", "ALL", "ALONE", "ALTERNATIVE", "AMAZING", "ANGRY", "ANIMAL", "ANNUAL", "ANOTHER", "ANXIOUS", "ANY", "APART", "APPROPRIATE", "ASLEEP", "AUTOMATIC", "AVAILABLE", "AWARE", "AWAY", "BACKGROUND", "BASIC", "BEAUTIFUL", "BEGINNING", "BEST", "BETTER", "BIG", "BITTER", "BORING", "BORN", "BOTH", "BRAVE", "BRIEF", "BRIGHT", "BRILLIANT", "BROAD", "BROWN", "BUDGET", "BUSINESS", "BUSY", "CALM", "CAPABLE", "CAPITAL", "CAR", "CAREFUL", "CERTAIN", "CHANCE", "CHARACTER", "CHEAP", "CHEMICAL", "CHICKEN", "CHOICE", "CIVIL", "CLASSIC", "CLEAN", "CLEAR", "CLOSE", "COLD", "COMFORTABLE", "COMMERCIAL", "COMMON", "COMPETITIVE", "COMPLETE", "COMPLEX", "COMPREHENSIVE", "CONFIDENT", "CONNECT", "CONSCIOUS", "CONSISTENT", "CONSTANT", "CONTENT", "COOL", "CORNER", "CORRECT", "CRAZY", "CREATIVE", "CRITICAL", "CULTURAL", "CURIOUS", "CURRENT", "CUTE", "DANGEROUS", "DARK", "DAUGHTER", "DAY", "DEAD", "DEAR", "DECENT", "DEEP", "DEPENDENT", "DESIGNER", "DESPERATE", "DIFFERENT", "DIFFICULT", "DIRECT", "DIRTY", "DISTINCT", "DOUBLE", "DOWNTOWN", "DRAMATIC", "DRESS", "DRUNK", "DRY", "DUE", "EACH", "EAST", "EASTERN", "EASY", "ECONOMY", "EDUCATIONAL", "EFFECTIVE", "EFFICIENT", "EITHER", "ELECTRICAL", "ELECTRONIC", "EMBARRASSED", "EMERGENCY", "EMOTIONAL", "EMPTY", "ENOUGH", "ENTIRE", "ENVIRONMENTAL", "EQUAL", "EQUIVALENT", "EVEN", "EVENING", "EVERY", "EXACT", "EXCELLENT", "EXCITING", "EXISTING", "EXPENSIVE", "EXPERT", "EXPRESS", "EXTENSION", "EXTERNAL", "EXTRA", "EXTREME", "FAIR", "FAMILIAR", "FAMOUS", "FAR", "FAST", "FAT", "FEDERAL", "FEELING", "FEMALE", "FEW", "FINAL", "FINANCIAL", "FINE", "FIRM", "FIRST", "FIT", "FLAT", "FOREIGN", "FORMAL", "FORMER", "FORWARD", "FREE", "FREQUENT", "FRESH", "FRIENDLY", "FRONT", "FULL", "FUN", "FUNNY", "FUTURE", "GAME", "GENERAL", "GLAD", "GLASS", "GLOBAL", "GOLD", "GOOD", "GRAND", "GREAT", "GREEN", "GROSS", "GUILTY", "HAPPY", "HARD", "HEAD", "HEALTHY", "HEAVY", "HELPFUL", "HIGH", "HIS", "HISTORICAL", "HOLIDAY", "HOME", "HONEST", "HORROR", "HOT", "HOUR", "HOUSE", "HUGE", "HUMAN", "HUNGRY", "IDEAL", "ILL", "ILLEGAL", "IMMEDIATE", "IMPORTANT", "IMPOSSIBLE", "IMPRESSIVE", "INCIDENT", "INDEPENDENT", "INDIVIDUAL", "INEVITABLE", "INFORMAL", "INITIAL", "INNER", "INSIDE", "INTELLIGENT", "INTERESTING", "INTERNAL", "INTERNATIONAL", "JOINT", "JUNIOR", "JUST", "KEY", "KIND", "KITCHEN", "KNOWN", "LARGE", "LAST", "LATE", "LATTER", "LEADING", "LEAST", "LEATHER", "LEFT", "LEGAL", "LESS", "LEVEL", "LIFE", "LITTLE", "LIVE", "LIVING", "LOCAL", "LOGICAL", "LONELY", "LONG", "LOOSE", "LOST", "LOUD", "LOW", "LOWER", "LUCKY", "MAD", "MAIN", "MAJOR", "MALE", "MANY", "MASSIVE", "MASTER", "MATERIAL", "MAXIMUM", "MEAN", "MEDICAL", "MEDIUM", "MENTAL", "MIDDLE", "MINIMUM", "MINOR", "MINUTE", "MISSION", "MOBILE", "MONEY", "MORE", "MOST", "MOTHER", "MOTOR", "MOUNTAIN", "MUCH", "NARROW", "NASTY", "NATIONAL", "NATIVE", "NATURAL", "NEARBY", "NEAT", "NECESSARY", "NEGATIVE", "NEITHER", "NERVOUS", "NEW", "NEXT", "NICE", "NO", "NORMAL", "NORTH", "NOVEL", "NUMEROUS", "OBJECTIVE", "OBVIOUS", "ODD", "OFFICIAL", "OK", "OLD", "ONE", "ONLY", "OPEN", "OPENING", "OPPOSITE", "ORDINARY", "ORIGINAL", "OTHER", "OTHERWISE", "OUTSIDE", "OVER", "OVERALL", "OWN", "PARKING", "PARTICULAR", "PARTY", "PAST", "PATIENT", "PERFECT", "PERIOD", "PERSONAL", "PHYSICAL", "PLANE", "PLASTIC", "PLEASANT", "PLENTY", "PLUS", "POLITICAL", "POOR", "POPULAR", "POSITIVE", "POSSIBLE", "POTENTIAL", "POWERFUL", "PRACTICAL", "PREGNANT", "PRESENT", "PRETEND", "PRETTY", "PREVIOUS", "PRIMARY", "PRIOR", "PRIVATE", "PRIZE", "PROFESSIONAL", "PROOF", "PROPER", "PROUD", "PSYCHOLOGICAL", "PUBLIC", "PURE", "PURPLE", "QUICK", "QUIET", "RARE", "RAW", "READY", "REAL", "REALISTIC", "REASONABLE", "RECENT", "RED", "REGULAR", "RELATIVE", "RELEVANT", "REMARKABLE", "REMOTE", "REPRESENTATIVE", "RESIDENT", "RESPONSIBLE", "RICH", "RIGHT", "ROUGH", "ROUND", "ROUTINE", "ROYAL", "SAD", "SAFE", "SALT", "SAME", "SAVINGS", "SCARED", "SEA", "SECRET", "SECURE", "SELECT", "SENIOR", "SENSITIVE", "SEPARATE", "SERIOUS", "SEVERAL", "SEVERE", "SEXUAL", "SHARP", "SHORT", "SHOT", "SICK", "SIGNAL", "SIGNIFICANT", "SILLY", "SILVER", "SIMILAR", "SIMPLE", "SINGLE", "SLIGHT", "SLOW", "SMALL", "SMART", "SMOOTH", "SOFT", "SOLID", "SOME", "SORRY", "SOUTH", "SOUTHERN", "SPARE", "SPECIAL", "SPECIALIST", "SPECIFIC", "SPIRITUAL", "SQUARE", "STANDARD", "STATUS", "STILL", "STOCK", "STRAIGHT", "STRANGE", "STREET", "STRICT", "STRONG", "STUPID", "SUBJECT", "SUBSTANTIAL", "SUCCESSFUL", "SUCH", "SUDDEN", "SUFFICIENT", "SUITABLE", "SUPER", "SURE", "SUSPICIOUS", "SWEET", "SWIMMING", "TALL", "TECHNICAL", "TEMPORARY", "TERRIBLE", "THAT", "THEN", "THESE", "THICK", "THIN", "THINK", "THIS", "TIGHT", "TIME", "TINY", "TOP", "TOTAL", "TOUGH", "TRADITIONAL", "TRAINING", "TRICK", "TYPICAL", "UGLY", "UNABLE", "UNFAIR", "UNHAPPY", "UNIQUE", "UNITED", "UNLIKELY", "UNUSUAL", "UPPER", "UPSET", "UPSTAIRS", "USED", "USEFUL", "USUAL", "VALUABLE", "VARIOUS", "VAST", "VEGETABLE", "VISIBLE", "VISUAL", "WARM", "WASTE", "WEAK", "WEEKLY", "WEIRD", "WEST", "WESTERN", "WHAT", "WHICH", "WHITE", "WHOLE", "WIDE", "WILD", "WILLING", "WINE", "WINTER", "WISE", "WONDERFUL", "WOODEN", "WORK", "WORKING", "WORTH", "WRONG", "YELLOW", "YOUNG" };

        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public GameCommandModelBase(string name, HashSet<string> triggers) : base(name, CommandTypeEnum.Game, triggers, includeExclamation: true, wildcards: false) { }

        protected GameCommandModelBase() : base() { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return GameCommandModelBase.commandLockSemaphore; } }

        public override bool DoesCommandHaveWork { get { return true; } }

        public GameCurrencyRequirementModel GameCurrencyRequirement { get { return (GameCurrencyRequirementModel)this.Requirements.Requirements.FirstOrDefault(r => r is GameCurrencyRequirementModel); } }

        public CooldownRequirementModel CooldownRequirement { get { return (CooldownRequirementModel)this.Requirements.Requirements.FirstOrDefault(r => r is CooldownRequirementModel); } }

        public virtual IEnumerable<CommandModelBase> GetInnerCommands() { return new List<CommandModelBase>(); }

        protected int GetBetAmount(CommandParametersModel parameters)
        {
            GameCurrencyRequirementModel gameCurrencyRequirement = this.GameCurrencyRequirement;
            return (gameCurrencyRequirement != null) ? gameCurrencyRequirement.GetGameAmount(parameters) : 0;
        }

        protected void ResetCooldown() { this.CooldownRequirement.Reset(); }

        protected UserViewModel GetRandomUser(CommandParametersModel parameters)
        {
            return ChannelSession.Services.User.GetRandomUser(parameters);
        }

        protected override Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            return base.ValidateRequirements(parameters);
        }

        protected override async Task<IEnumerable<CommandParametersModel>> PerformRequirements(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandBase.GameBetSpecialIdentifier] = this.GetBetAmount(parameters).ToString();
            return await base.PerformRequirements(parameters);
        }

        protected override Task PerformInternal(CommandParametersModel parameters)
        {
            return base.PerformInternal(parameters);
        }

        protected override void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type, this.GetType().ToString()); }

        protected GameOutcomeModel SelectRandomOutcome(UserViewModel user, IEnumerable<GameOutcomeModel> outcomes)
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

        protected async Task PerformOutcome(CommandParametersModel parameters, GameOutcomeModel outcome, int betAmount)
        {
            parameters.SpecialIdentifiers[GameCommandBase.GamePayoutSpecialIdentifier] = outcome.GetPayout(parameters.User, betAmount).ToString();
            if (outcome.Command != null)
            {
                await outcome.Command.Perform(parameters);
            }
        }

        protected async Task<string> GetRandomWord(string customWordsFilePath)
        {
            HashSet<string> wordsToUse = GameCommandModelBase.DefaultWords;
            if (!string.IsNullOrEmpty(customWordsFilePath) && ChannelSession.Services.FileService.FileExists(customWordsFilePath))
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(customWordsFilePath);
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
    }
}
