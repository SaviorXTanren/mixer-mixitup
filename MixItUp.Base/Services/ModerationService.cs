using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum ModerationChatInteractiveParticipationEnum
    {
        None = 0,
        AccountHour = 1,
        AccountDay = 2,
        AccountWeek = 3,
        AccountMonth = 4,
        ViewingTenMinutes = 10,
        ViewingThirtyMinutes = 11,
        ViewingOneHour = 12,
        ViewingTwoHours = 13,
        ViewingTenHours = 14,
        FollowerOnly = 19,
        SubscriberOnly = 20,
        ModeratorOnly = 30,
        [Obsolete]
        EmotesSkillsOnly = 40,
        [Obsolete]
        SkillsOnly = 41,
        [Obsolete]
        EmberSkillsOnly = 42,
    }

    public class ModerationService
    {
        public const string ModerationReasonSpecialIdentifier = "moderationreason";

        public const string WordRegexFormat = "(^|[^\\w]){0}([^\\w]|$)";
        public const string WordWildcardRegex = "\\S*";
        public static readonly string WordWildcardRegexEscaped = Regex.Escape(WordWildcardRegex);

        public static LockedList<string> CommunityFilteredWords { get; set; } = new LockedList<string>();

        private const string CommunityFilteredWordsFilePath = "Assets\\CommunityBannedWords.txt";

        private const int MinimumMessageLengthForPercentageModeration = 5;

        private static readonly Regex EmoteRegex = new Regex(":\\w+ ");
        public static readonly Regex EmojiRegex = new Regex(@"[#*0-9]\uFE0F?\u20E3|©\uFE0F?|[®\u203C\u2049\u2122\u2139\u2194-\u2199\u21A9\u21AA]\uFE0F?|[\u231A\u231B]|[\u2328\u23CF]\uFE0F?|[\u23E9-\u23EC]|[\u23ED-\u23EF]\uFE0F?|\u23F0|[\u23F1\u23F2]\uFE0F?|\u23F3|[\u23F8-\u23FA\u24C2\u25AA\u25AB\u25B6\u25C0\u25FB\u25FC]\uFE0F?|[\u25FD\u25FE]|[\u2600-\u2604\u260E\u2611]\uFE0F?|[\u2614\u2615]|\u2618\uFE0F?|\u261D(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\u2620\u2622\u2623\u2626\u262A\u262E\u262F\u2638-\u263A\u2640\u2642]\uFE0F?|[\u2648-\u2653]|[\u265F\u2660\u2663\u2665\u2666\u2668\u267B\u267E]\uFE0F?|\u267F|\u2692\uFE0F?|\u2693|[\u2694-\u2697\u2699\u269B\u269C\u26A0]\uFE0F?|\u26A1|\u26A7\uFE0F?|[\u26AA\u26AB]|[\u26B0\u26B1]\uFE0F?|[\u26BD\u26BE\u26C4\u26C5]|\u26C8\uFE0F?|\u26CE|[\u26CF\u26D1\u26D3]\uFE0F?|\u26D4|\u26E9\uFE0F?|\u26EA|[\u26F0\u26F1]\uFE0F?|[\u26F2\u26F3]|\u26F4\uFE0F?|\u26F5|[\u26F7\u26F8]\uFE0F?|\u26F9(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\u26FA\u26FD]|\u2702\uFE0F?|\u2705|[\u2708\u2709]\uFE0F?|[\u270A\u270B](?:\uD83C[\uDFFB-\uDFFF])?|[\u270C\u270D](?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\u270F\uFE0F?|[\u2712\u2714\u2716\u271D\u2721]\uFE0F?|\u2728|[\u2733\u2734\u2744\u2747]\uFE0F?|[\u274C\u274E\u2753-\u2755\u2757]|\u2763\uFE0F?|\u2764(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79)|\uFE0F(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79))?)?|[\u2795-\u2797]|\u27A1\uFE0F?|[\u27B0\u27BF]|[\u2934\u2935\u2B05-\u2B07]\uFE0F?|[\u2B1B\u2B1C\u2B50\u2B55]|[\u3030\u303D\u3297\u3299]\uFE0F?|\uD83C(?:[\uDC04\uDCCF]|[\uDD70\uDD71\uDD7E\uDD7F]\uFE0F?|[\uDD8E\uDD91-\uDD9A]|\uDDE6\uD83C[\uDDE8-\uDDEC\uDDEE\uDDF1\uDDF2\uDDF4\uDDF6-\uDDFA\uDDFC\uDDFD\uDDFF]|\uDDE7\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEF\uDDF1-\uDDF4\uDDF6-\uDDF9\uDDFB\uDDFC\uDDFE\uDDFF]|\uDDE8\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDEE\uDDF0-\uDDF5\uDDF7\uDDFA-\uDDFF]|\uDDE9\uD83C[\uDDEA\uDDEC\uDDEF\uDDF0\uDDF2\uDDF4\uDDFF]|\uDDEA\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDED\uDDF7-\uDDFA]|\uDDEB\uD83C[\uDDEE-\uDDF0\uDDF2\uDDF4\uDDF7]|\uDDEC\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEE\uDDF1-\uDDF3\uDDF5-\uDDFA\uDDFC\uDDFE]|\uDDED\uD83C[\uDDF0\uDDF2\uDDF3\uDDF7\uDDF9\uDDFA]|\uDDEE\uD83C[\uDDE8-\uDDEA\uDDF1-\uDDF4\uDDF6-\uDDF9]|\uDDEF\uD83C[\uDDEA\uDDF2\uDDF4\uDDF5]|\uDDF0\uD83C[\uDDEA\uDDEC-\uDDEE\uDDF2\uDDF3\uDDF5\uDDF7\uDDFC\uDDFE\uDDFF]|\uDDF1\uD83C[\uDDE6-\uDDE8\uDDEE\uDDF0\uDDF7-\uDDFB\uDDFE]|\uDDF2\uD83C[\uDDE6\uDDE8-\uDDED\uDDF0-\uDDFF]|\uDDF3\uD83C[\uDDE6\uDDE8\uDDEA-\uDDEC\uDDEE\uDDF1\uDDF4\uDDF5\uDDF7\uDDFA\uDDFF]|\uDDF4\uD83C\uDDF2|\uDDF5\uD83C[\uDDE6\uDDEA-\uDDED\uDDF0-\uDDF3\uDDF7-\uDDF9\uDDFC\uDDFE]|\uDDF6\uD83C\uDDE6|\uDDF7\uD83C[\uDDEA\uDDF4\uDDF8\uDDFA\uDDFC]|\uDDF8\uD83C[\uDDE6-\uDDEA\uDDEC-\uDDF4\uDDF7-\uDDF9\uDDFB\uDDFD-\uDDFF]|\uDDF9\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDED\uDDEF-\uDDF4\uDDF7\uDDF9\uDDFB\uDDFC\uDDFF]|\uDDFA\uD83C[\uDDE6\uDDEC\uDDF2\uDDF3\uDDF8\uDDFE\uDDFF]|\uDDFB\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDEE\uDDF3\uDDFA]|\uDDFC\uD83C[\uDDEB\uDDF8]|\uDDFD\uD83C\uDDF0|\uDDFE\uD83C[\uDDEA\uDDF9]|\uDDFF\uD83C[\uDDE6\uDDF2\uDDFC]|\uDE01|\uDE02\uFE0F?|[\uDE1A\uDE2F\uDE32-\uDE36]|\uDE37\uFE0F?|[\uDE38-\uDE3A\uDE50\uDE51\uDF00-\uDF20]|[\uDF21\uDF24-\uDF2C]\uFE0F?|[\uDF2D-\uDF35]|\uDF36\uFE0F?|[\uDF37-\uDF7C]|\uDF7D\uFE0F?|[\uDF7E-\uDF84]|\uDF85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDF86-\uDF93]|[\uDF96\uDF97\uDF99-\uDF9B\uDF9E\uDF9F]\uFE0F?|[\uDFA0-\uDFC1]|\uDFC2(?:\uD83C[\uDFFB-\uDFFF])?|[\uDFC3\uDFC4](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFC5\uDFC6]|\uDFC7(?:\uD83C[\uDFFB-\uDFFF])?|[\uDFC8\uDFC9]|\uDFCA(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCB\uDFCC](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCD\uDFCE]\uFE0F?|[\uDFCF-\uDFD3]|[\uDFD4-\uDFDF]\uFE0F?|[\uDFE0-\uDFF0]|\uDFF3(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08)|\uFE0F(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08))?)?|\uDFF4(?:\u200D\u2620\uFE0F?|\uDB40\uDC67\uDB40\uDC62\uDB40(?:\uDC65\uDB40\uDC6E\uDB40\uDC67|\uDC73\uDB40\uDC63\uDB40\uDC74|\uDC77\uDB40\uDC6C\uDB40\uDC73)\uDB40\uDC7F)?|[\uDFF5\uDFF7]\uFE0F?|[\uDFF8-\uDFFF])|\uD83D(?:[\uDC00-\uDC07]|\uDC08(?:\u200D\u2B1B)?|[\uDC09-\uDC14]|\uDC15(?:\u200D\uD83E\uDDBA)?|[\uDC16-\uDC3A]|\uDC3B(?:\u200D\u2744\uFE0F?)?|[\uDC3C-\uDC3E]|\uDC3F\uFE0F?|\uDC40|\uDC41(?:\u200D\uD83D\uDDE8\uFE0F?|\uFE0F(?:\u200D\uD83D\uDDE8\uFE0F?)?)?|[\uDC42\uDC43](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC44\uDC45]|[\uDC46-\uDC50](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC51-\uDC65]|[\uDC66\uDC67](?:\uD83C[\uDFFB-\uDFFF])?|\uDC68(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|[\uDC68\uDC69]\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFC-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFD-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFD\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFE]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|\uDC69(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?[\uDC68\uDC69]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|\uDC69\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFC-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFD-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFD\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFE]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|\uDC6A|[\uDC6B-\uDC6D](?:\uD83C[\uDFFB-\uDFFF])?|\uDC6E(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC6F(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDC70\uDC71](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC72(?:\uD83C[\uDFFB-\uDFFF])?|\uDC73(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC74-\uDC76](?:\uD83C[\uDFFB-\uDFFF])?|\uDC77(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC78(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC79-\uDC7B]|\uDC7C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC7D-\uDC80]|[\uDC81\uDC82](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC83(?:\uD83C[\uDFFB-\uDFFF])?|\uDC84|\uDC85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC86\uDC87](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC88-\uDC8E]|\uDC8F(?:\uD83C[\uDFFB-\uDFFF])?|\uDC90|\uDC91(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC92-\uDCA9]|\uDCAA(?:\uD83C[\uDFFB-\uDFFF])?|[\uDCAB-\uDCFC]|\uDCFD\uFE0F?|[\uDCFF-\uDD3D]|[\uDD49\uDD4A]\uFE0F?|[\uDD4B-\uDD4E\uDD50-\uDD67]|[\uDD6F\uDD70\uDD73]\uFE0F?|\uDD74(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\uDD75(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD76-\uDD79]\uFE0F?|\uDD7A(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD87\uDD8A-\uDD8D]\uFE0F?|\uDD90(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\uDD95\uDD96](?:\uD83C[\uDFFB-\uDFFF])?|\uDDA4|[\uDDA5\uDDA8\uDDB1\uDDB2\uDDBC\uDDC2-\uDDC4\uDDD1-\uDDD3\uDDDC-\uDDDE\uDDE1\uDDE3\uDDE8\uDDEF\uDDF3\uDDFA]\uFE0F?|[\uDDFB-\uDE2D]|\uDE2E(?:\u200D\uD83D\uDCA8)?|[\uDE2F-\uDE34]|\uDE35(?:\u200D\uD83D\uDCAB)?|\uDE36(?:\u200D\uD83C\uDF2B\uFE0F?)?|[\uDE37-\uDE44]|[\uDE45-\uDE47](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDE48-\uDE4A]|\uDE4B(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE4D\uDE4E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE80-\uDEA2]|\uDEA3(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDEA4-\uDEB3]|[\uDEB4-\uDEB6](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDEB7-\uDEBF]|\uDEC0(?:\uD83C[\uDFFB-\uDFFF])?|[\uDEC1-\uDEC5]|\uDECB\uFE0F?|\uDECC(?:\uD83C[\uDFFB-\uDFFF])?|[\uDECD-\uDECF]\uFE0F?|[\uDED0-\uDED2\uDED5-\uDED7]|[\uDEE0-\uDEE5\uDEE9]\uFE0F?|[\uDEEB\uDEEC]|[\uDEF0\uDEF3]\uFE0F?|[\uDEF4-\uDEFC\uDFE0-\uDFEB])|\uD83E(?:\uDD0C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD0D\uDD0E]|\uDD0F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD10-\uDD17]|[\uDD18-\uDD1C](?:\uD83C[\uDFFB-\uDFFF])?|\uDD1D|[\uDD1E\uDD1F](?:\uD83C[\uDFFB-\uDFFF])?|[\uDD20-\uDD25]|\uDD26(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD27-\uDD2F]|[\uDD30-\uDD34](?:\uD83C[\uDFFB-\uDFFF])?|\uDD35(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD36(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD37-\uDD39](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD3A|\uDD3C(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDD3D\uDD3E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD3F-\uDD45\uDD47-\uDD76]|\uDD77(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD78\uDD7A-\uDDB4]|[\uDDB5\uDDB6](?:\uD83C[\uDFFB-\uDFFF])?|\uDDB7|[\uDDB8\uDDB9](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDBA|\uDDBB(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDBC-\uDDCB]|[\uDDCD-\uDDCF](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD0|\uDDD1(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1|[\uDDAF-\uDDB3\uDDBC\uDDBD]))|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFC-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFD-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFD\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFE]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|[\uDDD2\uDDD3](?:\uD83C[\uDFFB-\uDFFF])?|\uDDD4(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD5(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDD6-\uDDDD](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDDDE\uDDDF](?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDDE0-\uDDFF\uDE70-\uDE74\uDE78-\uDE7A\uDE80-\uDE86\uDE90-\uDEA8\uDEB0-\uDEB6\uDEC0-\uDEC2\uDED0-\uDED6])");
        public static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

        private LockedList<string> communityWords = new LockedList<string>();
        private LockedList<string> filteredWords = new LockedList<string>();
        private LockedList<string> bannedWords = new LockedList<string>();

        private DateTimeOffset chatParticipationLastErrorMessage = DateTimeOffset.MinValue;

        public async Task Initialize()
        {
            if (ServiceManager.Get<IFileService>().FileExists(ModerationService.CommunityFilteredWordsFilePath))
            {
                string text = await ServiceManager.Get<IFileService>().ReadFile(ModerationService.CommunityFilteredWordsFilePath);
                ModerationService.CommunityFilteredWords = new LockedList<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                foreach (string word in ModerationService.CommunityFilteredWords)
                {
                    this.communityWords.Add(string.Format(WordRegexFormat, Regex.Escape(word)));
                }
            }

            this.RebuildCache();
        }

        public void RebuildCache()
        {
            this.filteredWords.Clear();
            foreach (string word in ChannelSession.Settings.FilteredWords)
            {
                this.filteredWords.Add(word);
            }

            this.bannedWords.Clear();
            foreach (string word in ChannelSession.Settings.BannedWords)
            {
                this.bannedWords.Add(word);
            }
        }

        public async Task<string> ShouldTextBeModerated(UserV2ViewModel user, string text, bool containsLink = false)
        {
            string reason = null;

            if (string.IsNullOrEmpty(text) || user.IsSpecialtyExcluded)
            {
                return reason;
            }

            reason = await ShouldTextBeFilteredWordModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationFilteredWordsApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldTextBeExcessiveModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationChatTextApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldTextBeLinkModerated(user, text, containsLink);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationBlockLinksApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            return reason;
        }

        public async Task<string> ShouldTextBeFilteredWordModerated(UserV2ViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (!user.MeetsRole(ChannelSession.Settings.ModerationFilteredWordsExcemptUserRole))
            {
                if (ChannelSession.Settings.ModerationUseCommunityFilteredWords)
                {
                    foreach (string word in this.communityWords)
                    {
                        if (Regex.IsMatch(text, word, RegexOptions.IgnoreCase))
                        {
                            return string.Format(MixItUp.Base.Resources.ModerationFilteredWord, word);
                        }
                    }
                }

                foreach (string word in this.filteredWords)
                {
                    if (Regex.IsMatch(text, string.Format(WordRegexFormat, Regex.Escape(word).Replace(WordWildcardRegexEscaped, WordWildcardRegex)), RegexOptions.IgnoreCase))
                    {
                        return string.Format(MixItUp.Base.Resources.ModerationFilteredWord, word);
                    }
                }

                foreach (string word in this.bannedWords)
                {
                    if (Regex.IsMatch(text, string.Format(WordRegexFormat, Regex.Escape(word).Replace(WordWildcardRegexEscaped, WordWildcardRegex)), RegexOptions.IgnoreCase))
                    {
                        await ServiceManager.Get<ChatService>().BanUser(user);
                        return string.Format(MixItUp.Base.Resources.ModerationBannedWord, word);
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeExcessiveModerated(UserV2ViewModel user, string text)
        {
            if (!user.MeetsRole(ChannelSession.Settings.ModerationChatTextExcemptUserRole))
            {
                if (ChannelSession.Settings.ModerationCapsBlockCount > 0)
                {
                    int count = text.Count(c => char.IsUpper(c));
                    if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(text.Count(), count);
                    }

                    if (count >= ChannelSession.Settings.ModerationCapsBlockCount)
                    {
                        return MixItUp.Base.Resources.ModerationTooManyCaps;
                    }
                }

                // Perform text preparing after checking for caps
                text = PrepareTextForChecking(text);

                if (ChannelSession.Settings.ModerationPunctuationBlockCount > 0)
                {
                    string leftOverText = text.ToString();
                    List<string> messageSegments = new List<string>();
                    int count = 0;

                    foreach (Match match in EmoteRegex.Matches(text))
                    {
                        messageSegments.Add(match.Value);
                        leftOverText = leftOverText.Replace(match.Value, "");
                        count++;
                    }
                    foreach (Match match in EmojiRegex.Matches(text))
                    {
                        messageSegments.Add(match.Value);
                        leftOverText = leftOverText.Replace(match.Value, "");
                        count++;
                    }

                    if (!string.IsNullOrEmpty(leftOverText))
                    {
                        count += leftOverText.Count(c => char.IsSymbol(c) || char.IsPunctuation(c));
                        messageSegments.AddRange(leftOverText.ToCharArray().Select(c => c.ToString()));
                    }

                    if (ChannelSession.Settings.ModerationPunctuationBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(messageSegments.Count, count);
                    }

                    if (count >= ChannelSession.Settings.ModerationPunctuationBlockCount)
                    {
                        return MixItUp.Base.Resources.ModerationTooManyPunctuationSymbolsEmotes;
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeLinkModerated(UserV2ViewModel user, string text, bool containsLink = false)
        {
            text = PrepareTextForChecking(text);

            if (!user.MeetsRole(ChannelSession.Settings.ModerationBlockLinksExcemptUserRole))
            {
                if (ChannelSession.Settings.ModerationBlockLinks && (containsLink || LinkRegex.IsMatch(text)))
                {
                    return MixItUp.Base.Resources.ModerationNoLinks;
                }
            }

            return null;
        }

        public bool DoesUserMeetChatInteractiveParticipationRequirement(UserV2ViewModel user, ChatMessageViewModel message = null)
        {
            if (ChannelSession.Settings.ModerationChatInteractiveParticipation != ModerationChatInteractiveParticipationEnum.None)
            {
                if (user == null)
                {
                    return false;
                }

                if (user.IsSpecialtyExcluded)
                {
                    return true;
                }

                if (user.MeetsRole(ChannelSession.Settings.ModerationChatInteractiveParticipationExcemptUserRole))
                {
                    return true;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly && !user.MeetsRole(UserRoleEnum.Follower))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly && !user.MeetsRole(UserRoleEnum.Subscriber))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly && !user.MeetsRole(UserRoleEnum.Moderator))
                {
                    return false;
                }

                if (user.AccountDate.HasValue)
                {
                    TimeSpan accountLength = DateTimeOffset.Now - user.AccountDate.GetValueOrDefault();
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour && accountLength.TotalHours < 1)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay && accountLength.TotalDays < 1)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek && accountLength.TotalDays < 7)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountMonth && accountLength.TotalDays < 30)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                TimeSpan viewingLength = TimeSpan.FromMinutes(user.OnlineViewingMinutes);
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes && viewingLength.TotalMinutes < 10)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes && viewingLength.TotalMinutes < 30)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour && viewingLength.TotalHours < 1)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours && viewingLength.TotalHours < 2)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenHours && viewingLength.TotalHours < 10)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task SendChatInteractiveParticipationWhisper(UserV2ViewModel user, bool isChat = false)
        {
            if (user != null)
            {
                string reason = string.Empty;
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly)
                {
                    reason = MixItUp.Base.Resources.Followers;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly)
                {
                    reason = MixItUp.Base.Resources.Subscribers;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly)
                {
                    reason = MixItUp.Base.Resources.Moderators;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour)
                {
                    reason = MixItUp.Base.Resources.ModerationAccountsOlderThanOneHour;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay)
                {
                    reason = MixItUp.Base.Resources.ModerationAccountsOlderThanOneDay;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek)
                {
                    reason = MixItUp.Base.Resources.ModerationAccountsOlderThanOneWeek;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountMonth)
                {
                    reason = MixItUp.Base.Resources.ModerationAccountsOlderThanOneMonth;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes)
                {
                    reason = MixItUp.Base.Resources.ModerationViewingMoreThanTenMinutes;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes)
                {
                    reason = MixItUp.Base.Resources.ModerationViewingMoreThanThirtyMinutes;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour)
                {
                    reason = MixItUp.Base.Resources.ModerationViewingMoreThanOneHour;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours)
                {
                    reason = MixItUp.Base.Resources.ModerationViewingMoreThanTwoHours;
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenHours)
                {
                    reason = MixItUp.Base.Resources.ModerationViewingMoreThanTenHours;
                }

                if (isChat)
                {
                    if (this.chatParticipationLastErrorMessage > DateTimeOffset.Now)
                    {
                        return;
                    }

                    this.chatParticipationLastErrorMessage = DateTimeOffset.Now.AddSeconds(10);
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.ModerationParticipationMessageDeleted, user.Username, reason), platform: user.Platform);
                }
            }
        }

        private string PrepareTextForChecking(string text)
        {
            string result = string.IsNullOrEmpty(text) ? string.Empty : text.ToLower();
            result = ChatListControlViewModel.UserNameTagRegex.Replace(result, "");
            return result;
        }

        private int ConvertCountToPercentage(int length, int count)
        {
            if (length >= MinimumMessageLengthForPercentageModeration)
            {
                return (int)(((double)count) / ((double)length) * 100.0);
            }
            else
            {
                return 0;
            }
        }
    }
}
