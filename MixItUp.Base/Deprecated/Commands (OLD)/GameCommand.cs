using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Commands
{
    #region Base Game Classes

    [Obsolete]
    [DataContract]
    public class GameOutcome
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double Payout { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, double> RolePayouts { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, int> RoleProbabilities { get; set; }

        [DataMember]
        public CustomCommand Command { get; set; }

        public GameOutcome() { }

        public GameOutcome(string name, double payout, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommand command)
        {
            this.Name = name;
            this.Payout = payout;
            this.RolePayouts = new Dictionary<UserRoleEnum, double>();
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public GameOutcome(string name, Dictionary<UserRoleEnum, double> rolePayouts, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommand command)
        {
            this.Name = name;
            this.Payout = 0;
            this.RolePayouts = rolePayouts;
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public int GetRoleProbability(UserV2ViewModel user)
        {
            var roleProbabilities = this.RoleProbabilities.Select(kvp => kvp.Key).OrderByDescending(k => k);
            if (roleProbabilities.Any(r => user.HasPermissionsTo(r)))
            {
                return this.RoleProbabilities[roleProbabilities.FirstOrDefault(r => user.HasPermissionsTo(r))];
            }
            return this.RoleProbabilities[roleProbabilities.LastOrDefault()];
        }

        public int GetPayout(UserV2ViewModel user, int betAmount)
        {
            return Convert.ToInt32(Convert.ToDouble(betAmount) * this.GetPayoutAmount(user));
        }

        private double GetPayoutAmount(UserV2ViewModel user)
        {
            if (this.RolePayouts.Count > 0)
            {
                var rolePayouts = this.RolePayouts.Select(kvp => kvp.Key).OrderByDescending(k => k);
                if (rolePayouts.Any(r => user.HasPermissionsTo(r)))
                {
                    return this.RolePayouts[rolePayouts.FirstOrDefault(r => user.HasPermissionsTo(r))];
                }
                return this.RolePayouts[rolePayouts.LastOrDefault()];
            }
            return this.Payout;
        }
    }

    [Obsolete]
    [DataContract]
    public abstract class GameCommandBase : PermissionsCommandBase
    {
        public const string GameBetSpecialIdentifier = "gamebet";

        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameAllPayoutSpecialIdentifier = "gameallpayout";

        public const string GameWinnersSpecialIdentifier = "gamewinners";

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        public GameCommandBase() { }

        [JsonIgnore]
        public override HashSet<string> CommandTriggers { get { return new HashSet<string>(this.Commands.Select(c => "!" + c)); } }

        protected override SemaphoreSlim AsyncSemaphore { get { return GameCommandBase.gameCommandPerformSemaphore; } }
    }

    [Obsolete]
    [DataContract]
    public abstract class SinglePlayerOutcomeGameCommand : GameCommandBase
    {
        [DataMember]
        public List<GameOutcome> Outcomes { get; set; }

        public SinglePlayerOutcomeGameCommand()
        {
            this.Outcomes = new List<GameOutcome>();
        }
    }

    [Obsolete]
    [DataContract]
    public abstract class TwoPlayerGameCommandBase : GameCommandBase
    {
        [DataMember]
        public GameOutcome SuccessfulOutcome { get; set; }

        [DataMember]
        public GameOutcome FailedOutcome { get; set; }

        public TwoPlayerGameCommandBase() { }
    }

    [Obsolete]
    [DataContract]
    public abstract class GroupGameCommand : GameCommandBase
    {
        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand UserJoinCommand { get; set; }

        [DataMember]
        public GameOutcome UserSuccessOutcome { get; set; }
        [DataMember]
        public GameOutcome UserFailOutcome { get; set; }

        [DataMember]
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public GroupGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public abstract class LongRunningGameCommand : GameCommandBase
    {
        private const string GameTotalAmountSpecialIdentifier = "gametotalamount";

        [DataMember]
        public int TotalAmount { get; set; }

        [DataMember]
        public string StatusArgument { get; set; }

        public LongRunningGameCommand() { }
    }

    #endregion Base Game Classes

    [Obsolete]
    [DataContract]
    public class HotPotatoGameCommand : GameCommandBase
    {
        [DataMember]
        [Obsolete]
        public int TimeLimit { get; set; }

        [DataMember]
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand TossPotatoCommand { get; set; }

        [DataMember]
        public CustomCommand PotatoExplodeCommand { get; set; }

        public HotPotatoGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class BeachBallGameCommand : GameCommandBase
    {
        [DataMember]
        [Obsolete]
        public int HitTimeLimit { get; set; }

        [DataMember]
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand BallHitCommand { get; set; }

        [DataMember]
        public CustomCommand BallMissedCommand { get; set; }

        public BeachBallGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class SpinGameCommand : SinglePlayerOutcomeGameCommand
    {
        public SpinGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class VendingMachineGameCommand : SinglePlayerOutcomeGameCommand
    {
        public VendingMachineGameCommand() { }
    }

    #region SlotsGameOutcome

    [Obsolete]
    [DataContract]
    public class SlotsGameOutcome : GameOutcome
    {
        [DataMember]
        public string Symbol1 { get; set; }
        [DataMember]
        public string Symbol2 { get; set; }
        [DataMember]
        public string Symbol3 { get; set; }

        [DataMember]
        public bool AnyOrder { get; set; }

        public SlotsGameOutcome() { }

        public SlotsGameOutcome(string name, string symbol1, string symbol2, string symbol3, Dictionary<UserRoleEnum, double> rolePayouts, CustomCommand command, bool anyOrder)
            : base(name, rolePayouts, new Dictionary<UserRoleEnum, int>() { { UserRoleEnum.User, 0 }, { UserRoleEnum.Subscriber, 0 }, { UserRoleEnum.Mod, 0 } }, command)
        {
            this.Symbol1 = symbol1;
            this.Symbol2 = symbol2;
            this.Symbol3 = symbol3;
            this.AnyOrder = anyOrder;
        }
    }

    #endregion SlotsGameOutcome

    [Obsolete]
    [DataContract]
    public class SlotMachineGameCommand : SinglePlayerOutcomeGameCommand
    {
        public const string GameSlotsOutcomeSpecialIdentifier = "gameslotsoutcome";

        [DataMember]
        public List<string> AllSymbols { get; set; }

        [DataMember]
        public CustomCommand FailureOutcomeCommand { get; set; }

        public SlotMachineGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class StealGameCommand : TwoPlayerGameCommandBase
    {
        public StealGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class PickpocketGameCommand : TwoPlayerGameCommandBase
    {
        public PickpocketGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class DuelGameCommand : TwoPlayerGameCommandBase
    {
        [DataMember]
        public CustomCommand StartedCommand { get; set; }
        [DataMember]
        public CustomCommand NotAcceptedCommand { get; set; }

        [DataMember]
        public int TimeLimit { get; set; }

        public DuelGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class HeistGameCommand : GroupGameCommand
    {
        [DataMember]
        public CustomCommand AllSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand TopThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand MiddleThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand LowThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand NoneSucceedCommand { get; set; }

        public HeistGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class RussianRouletteGameCommand : GroupGameCommand
    {
        [DataMember]
        public int MaxWinners { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        public RussianRouletteGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class TreasureDefenseGameCommand : GroupGameCommand
    {
        public const string GameWinLoseTypeSpecialIdentifier = "gameusertype";

        private enum WinLosePlayerType
        {
            King,
            Knight,
            Thief
        }

        [DataMember]
        public int ThiefPlayerPercentage { get; set; }

        [DataMember]
        public CustomCommand KnightUserCommand { get; set; }
        [DataMember]
        public CustomCommand ThiefUserCommand { get; set; }
        [DataMember]
        public CustomCommand KingUserCommand { get; set; }

        [DataMember]
        public CustomCommand KnightSelectedCommand { get; set; }
        [DataMember]
        public CustomCommand ThiefSelectedCommand { get; set; }

        public TreasureDefenseGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class BidGameCommand : GroupGameCommand
    {
        [DataMember]
        public RoleRequirementViewModel GameStarterRequirement { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        public BidGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class BetGameCommand : GroupGameCommand
    {
        public const string GameBetOptionsSpecialIdentifier = "gamebetoptions";
        public const string GameBetWinningOptionSpecialIdentifier = "gamebetwinningoption";

        [DataMember]
        public RoleRequirementViewModel GameStarterRequirement { get; set; }

        [DataMember]
        public List<GameOutcome> BetOptions { get; set; }

        [DataMember]
        public CustomCommand BetsClosedCommand { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        public BetGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class RouletteGameCommand : GroupGameCommand
    {
        public const string GameBetTypeSpecialIdentifier = "gamebettype";

        public const string GameValidBetTypesSpecialIdentifier = "gamevalidbettypes";

        public const string GameWinningBetTypeSpecialIdentifier = "gamewinningbettype";

        [DataMember]
        public bool IsNumberRange { get; set; }
        [DataMember]
        public HashSet<string> ValidBetTypes { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        public RouletteGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class HitmanGameCommand : GroupGameCommand
    {
        public const string GameHitmanNameSpecialIdentifier = "gamehitmanname";

        public static readonly HashSet<string> DefaultWords = new HashSet<string>() { "ABLE", "ACCEPTABLE", "ACCORDING", "ACCURATE", "ACTION", "ACTIVE", "ACTUAL", "ADDITIONAL", "ADMINISTRATIVE", "ADULT", "AFRAID", "AFTER", "AFTERNOON", "AGENT", "AGGRESSIVE", "AGO", "AIRLINE", "ALIVE", "ALL", "ALONE", "ALTERNATIVE", "AMAZING", "ANGRY", "ANIMAL", "ANNUAL", "ANOTHER", "ANXIOUS", "ANY", "APART", "APPROPRIATE", "ASLEEP", "AUTOMATIC", "AVAILABLE", "AWARE", "AWAY", "BACKGROUND", "BASIC", "BEAUTIFUL", "BEGINNING", "BEST", "BETTER", "BIG", "BITTER", "BORING", "BORN", "BOTH", "BRAVE", "BRIEF", "BRIGHT", "BRILLIANT", "BROAD", "BROWN", "BUDGET", "BUSINESS", "BUSY", "CALM", "CAPABLE", "CAPITAL", "CAR", "CAREFUL", "CERTAIN", "CHANCE", "CHARACTER", "CHEAP", "CHEMICAL", "CHICKEN", "CHOICE", "CIVIL", "CLASSIC", "CLEAN", "CLEAR", "CLOSE", "COLD", "COMFORTABLE", "COMMERCIAL", "COMMON", "COMPETITIVE", "COMPLETE", "COMPLEX", "COMPREHENSIVE", "CONFIDENT", "CONNECT", "CONSCIOUS", "CONSISTENT", "CONSTANT", "CONTENT", "COOL", "CORNER", "CORRECT", "CRAZY", "CREATIVE", "CRITICAL", "CULTURAL", "CURIOUS", "CURRENT", "CUTE", "DANGEROUS", "DARK", "DAUGHTER", "DAY", "DEAD", "DEAR", "DECENT", "DEEP", "DEPENDENT", "DESIGNER", "DESPERATE", "DIFFERENT", "DIFFICULT", "DIRECT", "DIRTY", "DISTINCT", "DOUBLE", "DOWNTOWN", "DRAMATIC", "DRESS", "DRUNK", "DRY", "DUE", "EACH", "EAST", "EASTERN", "EASY", "ECONOMY", "EDUCATIONAL", "EFFECTIVE", "EFFICIENT", "EITHER", "ELECTRICAL", "ELECTRONIC", "EMBARRASSED", "EMERGENCY", "EMOTIONAL", "EMPTY", "ENOUGH", "ENTIRE", "ENVIRONMENTAL", "EQUAL", "EQUIVALENT", "EVEN", "EVENING", "EVERY", "EXACT", "EXCELLENT", "EXCITING", "EXISTING", "EXPENSIVE", "EXPERT", "EXPRESS", "EXTENSION", "EXTERNAL", "EXTRA", "EXTREME", "FAIR", "FAMILIAR", "FAMOUS", "FAR", "FAST", "FAT", "FEDERAL", "FEELING", "FEMALE", "FEW", "FINAL", "FINANCIAL", "FINE", "FIRM", "FIRST", "FIT", "FLAT", "FOREIGN", "FORMAL", "FORMER", "FORWARD", "FREE", "FREQUENT", "FRESH", "FRIENDLY", "FRONT", "FULL", "FUN", "FUNNY", "FUTURE", "GAME", "GENERAL", "GLAD", "GLASS", "GLOBAL", "GOLD", "GOOD", "GRAND", "GREAT", "GREEN", "GROSS", "GUILTY", "HAPPY", "HARD", "HEAD", "HEALTHY", "HEAVY", "HELPFUL", "HIGH", "HIS", "HISTORICAL", "HOLIDAY", "HOME", "HONEST", "HORROR", "HOT", "HOUR", "HOUSE", "HUGE", "HUMAN", "HUNGRY", "IDEAL", "ILL", "ILLEGAL", "IMMEDIATE", "IMPORTANT", "IMPOSSIBLE", "IMPRESSIVE", "INCIDENT", "INDEPENDENT", "INDIVIDUAL", "INEVITABLE", "INFORMAL", "INITIAL", "INNER", "INSIDE", "INTELLIGENT", "INTERESTING", "INTERNAL", "INTERNATIONAL", "JOINT", "JUNIOR", "JUST", "KEY", "KIND", "KITCHEN", "KNOWN", "LARGE", "LAST", "LATE", "LATTER", "LEADING", "LEAST", "LEATHER", "LEFT", "LEGAL", "LESS", "LEVEL", "LIFE", "LITTLE", "LIVE", "LIVING", "LOCAL", "LOGICAL", "LONELY", "LONG", "LOOSE", "LOST", "LOUD", "LOW", "LOWER", "LUCKY", "MAD", "MAIN", "MAJOR", "MALE", "MANY", "MASSIVE", "MASTER", "MATERIAL", "MAXIMUM", "MEAN", "MEDICAL", "MEDIUM", "MENTAL", "MIDDLE", "MINIMUM", "MINOR", "MINUTE", "MISSION", "MOBILE", "MONEY", "MORE", "MOST", "MOTHER", "MOTOR", "MOUNTAIN", "MUCH", "NARROW", "NASTY", "NATIONAL", "NATIVE", "NATURAL", "NEARBY", "NEAT", "NECESSARY", "NEGATIVE", "NEITHER", "NERVOUS", "NEW", "NEXT", "NICE", "NO", "NORMAL", "NORTH", "NOVEL", "NUMEROUS", "OBJECTIVE", "OBVIOUS", "ODD", "OFFICIAL", "OK", "OLD", "ONE", "ONLY", "OPEN", "OPENING", "OPPOSITE", "ORDINARY", "ORIGINAL", "OTHER", "OTHERWISE", "OUTSIDE", "OVER", "OVERALL", "OWN", "PARKING", "PARTICULAR", "PARTY", "PAST", "PATIENT", "PERFECT", "PERIOD", "PERSONAL", "PHYSICAL", "PLANE", "PLASTIC", "PLEASANT", "PLENTY", "PLUS", "POLITICAL", "POOR", "POPULAR", "POSITIVE", "POSSIBLE", "POTENTIAL", "POWERFUL", "PRACTICAL", "PREGNANT", "PRESENT", "PRETEND", "PRETTY", "PREVIOUS", "PRIMARY", "PRIOR", "PRIVATE", "PRIZE", "PROFESSIONAL", "PROOF", "PROPER", "PROUD", "PSYCHOLOGICAL", "PUBLIC", "PURE", "PURPLE", "QUICK", "QUIET", "RARE", "RAW", "READY", "REAL", "REALISTIC", "REASONABLE", "RECENT", "RED", "REGULAR", "RELATIVE", "RELEVANT", "REMARKABLE", "REMOTE", "REPRESENTATIVE", "RESIDENT", "RESPONSIBLE", "RICH", "RIGHT", "ROUGH", "ROUND", "ROUTINE", "ROYAL", "SAD", "SAFE", "SALT", "SAME", "SAVINGS", "SCARED", "SEA", "SECRET", "SECURE", "SELECT", "SENIOR", "SENSITIVE", "SEPARATE", "SERIOUS", "SEVERAL", "SEVERE", "SEXUAL", "SHARP", "SHORT", "SHOT", "SICK", "SIGNAL", "SIGNIFICANT", "SILLY", "SILVER", "SIMILAR", "SIMPLE", "SINGLE", "SLIGHT", "SLOW", "SMALL", "SMART", "SMOOTH", "SOFT", "SOLID", "SOME", "SORRY", "SOUTH", "SOUTHERN", "SPARE", "SPECIAL", "SPECIALIST", "SPECIFIC", "SPIRITUAL", "SQUARE", "STANDARD", "STATUS", "STILL", "STOCK", "STRAIGHT", "STRANGE", "STREET", "STRICT", "STRONG", "STUPID", "SUBJECT", "SUBSTANTIAL", "SUCCESSFUL", "SUCH", "SUDDEN", "SUFFICIENT", "SUITABLE", "SUPER", "SURE", "SUSPICIOUS", "SWEET", "SWIMMING", "TALL", "TECHNICAL", "TEMPORARY", "TERRIBLE", "THAT", "THEN", "THESE", "THICK", "THIN", "THINK", "THIS", "TIGHT", "TIME", "TINY", "TOP", "TOTAL", "TOUGH", "TRADITIONAL", "TRAINING", "TRICK", "TYPICAL", "UGLY", "UNABLE", "UNFAIR", "UNHAPPY", "UNIQUE", "UNITED", "UNLIKELY", "UNUSUAL", "UPPER", "UPSET", "UPSTAIRS", "USED", "USEFUL", "USUAL", "VALUABLE", "VARIOUS", "VAST", "VEGETABLE", "VISIBLE", "VISUAL", "WARM", "WASTE", "WEAK", "WEEKLY", "WEIRD", "WEST", "WESTERN", "WHAT", "WHICH", "WHITE", "WHOLE", "WIDE", "WILD", "WILLING", "WINE", "WINTER", "WISE", "WONDERFUL", "WOODEN", "WORK", "WORKING", "WORTH", "WRONG", "YELLOW", "YOUNG" };

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommand HitmanApproachingCommand { get; set; }
        [DataMember]
        public CustomCommand HitmanAppearsCommand { get; set; }

        [DataMember]
        public int HitmanTimeLimit { get; set; }

        public HitmanGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class WordScrambleGameCommand : GroupGameCommand
    {
        public const string GameWordScrambleWordSpecialIdentifier = "gamewordscrambleword";
        public const string GameWordScrambleAnswerSpecialIdentifier = "gamewordscrambleanswer";

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommand WordScramblePrepareCommand { get; set; }
        [DataMember]
        public CustomCommand WordScrambleBeginCommand { get; set; }

        [DataMember]
        public int WordScrambleTimeLimit { get; set; }

        public WordScrambleGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class CoinPusherGameCommand : LongRunningGameCommand
    {
        [DataMember]
        public int MinimumAmountForPayout { get; set; }
        [DataMember]
        public int PayoutProbability { get; set; }

        [DataMember]
        public double PayoutPercentageMinimum { get; set; }
        [DataMember]
        public double PayoutPercentageMaximum { get; set; }

        [DataMember]
        public CustomCommand StatusCommand { get; set; }
        [DataMember]
        public CustomCommand NoPayoutCommand { get; set; }
        [DataMember]
        public CustomCommand PayoutCommand { get; set; }

        public CoinPusherGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class VolcanoGameCommand : LongRunningGameCommand
    {
        [DataMember]
        public CustomCommand Stage1DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage1StatusCommand { get; set; }

        [DataMember]
        public int Stage2MinimumAmount { get; set; }
        [DataMember]
        public CustomCommand Stage2DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage2StatusCommand { get; set; }

        [DataMember]
        public int Stage3MinimumAmount { get; set; }
        [DataMember]
        public CustomCommand Stage3DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage3StatusCommand { get; set; }

        [DataMember]
        public int PayoutProbability { get; set; }
        [DataMember]
        public double PayoutPercentageMinimum { get; set; }
        [DataMember]
        public double PayoutPercentageMaximum { get; set; }
        [DataMember]
        public CustomCommand PayoutCommand { get; set; }

        [DataMember]
        public string CollectArgument { get; set; }
        [DataMember]
        public int CollectTimeLimit { get; set; }
        [DataMember]
        public double CollectPayoutPercentageMinimum { get; set; }
        [DataMember]
        public double CollectPayoutPercentageMaximum { get; set; }
        [DataMember]
        public CustomCommand CollectCommand { get; set; }

        public VolcanoGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class LockBoxGameCommand : LongRunningGameCommand
    {
        public const string GameHitmanHintSpecialIdentifier = "gamelockboxhint";
        public const string GameHitmanInspectionSpecialIdentifier = "gamelockboxinspection";

        [DataMember]
        public CustomCommand StatusCommand { get; set; }

        [DataMember]
        public int CombinationLength { get; set; }

        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public CustomCommand SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommand FailedGuessCommand { get; set; }

        [DataMember]
        public string InspectionArgument { get; set; }
        [DataMember]
        public int InspectionCost { get; set; }
        [DataMember]
        public CustomCommand InspectionCommand { get; set; }

        [DataMember]
        public string CurrentCombination { get; set; }

        public LockBoxGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class HangmanGameCommand : LongRunningGameCommand
    {
        public const string GameHangmanCurrentSpecialIdentifier = "gamehangmancurrent";
        public const string GameHangmanFailedGuessesSpecialIdentifier = "gamehangmanfailedguesses";
        public const string GameHangmanAnswerSpecialIdentifier = "gamehangmananswer";

        [DataMember]
        public CustomCommand StatusCommand { get; set; }

        [DataMember]
        public int MaxFailures { get; set; }
        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public CustomCommand SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommand FailedGuessCommand { get; set; }

        [DataMember]
        public CustomCommand GameWonCommand { get; set; }
        [DataMember]
        public CustomCommand GameLostCommand { get; set; }

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public string CurrentWord { get; set; }
        [DataMember]
        public HashSet<char> SuccessfulGuesses { get; set; } = new HashSet<char>();
        [DataMember]
        public HashSet<char> FailedGuesses { get; set; } = new HashSet<char>();

        public HangmanGameCommand() { }
    }

    [Obsolete]
    [DataContract]
    public class TriviaGameQuestion
    {
        [DataMember]
        public string Question { get; set; }

        [DataMember]
        public string CorrectAnswer { get; set; }
        [DataMember]
        public List<string> WrongAnswers { get; set; } = new List<string>();

        public uint TotalAnswers { get { return (uint)this.WrongAnswers.Count + 1; } }
    }

    [Obsolete]
    [DataContract]
    public class TriviaGameCommand : GroupGameCommand
    {
        private class OpenTDBResults
        {
            public int response_code { get; set; }
            public List<OpenTDBQuestion> results { get; set; }
        }

        private class OpenTDBQuestion
        {
            public string category { get; set; }
            public string type { get; set; }
            public string difficulty { get; set; }
            public string question { get; set; }
            public string correct_answer { get; set; }
            public List<string> incorrect_answers { get; set; }
        }

        public const string GameQuestion = "gamequestion";
        public const string GameAnswers = "gameanswers";
        public const string GameCorrectAnswer = "gamecorrectanswer";

        private const string OpenTDBUrl = "https://opentdb.com/api.php?amount=1&type=multiple";

        private static readonly TriviaGameQuestion FallbackQuestion = new TriviaGameQuestion()
        {
            Question = "Looks like we failed to get a question, who's to blame?",
            CorrectAnswer = "SaviorXTanren",
            WrongAnswers = new List<string>() { "SaviorXTanren", "SaviorXTanren", "SaviorXTanren" }
        };

        [DataMember]
        public List<TriviaGameQuestion> CustomQuestions { get; set; } = new List<TriviaGameQuestion>();

        [DataMember]
        public int WinAmount { get; set; }

        [DataMember]
        public CustomCommand CorrectAnswerCommand { get; set; }

        [DataMember]
        public bool UseRandomOnlineQuestions { get; set; }

        public TriviaGameCommand() { }
    }
}
