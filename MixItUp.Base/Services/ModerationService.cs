using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        EmotesSkillsOnly = 40,
        SkillsOnly = 41,
        EmberSkillsOnly = 42,
    }

    public interface IModerationService
    {
        Task Initialize();

        Task<string> ShouldTextBeModerated(UserViewModel user, string text, bool containsLink = false);
        Task<string> ShouldTextBeFilteredWordModerated(UserViewModel user, string text);
        string ShouldTextBeExcessiveModerated(UserViewModel user, string text);
        string ShouldTextBeLinkModerated(UserViewModel user, string text, bool containsLink = false);

        bool DoesUserMeetChatInteractiveParticipationRequirement(UserViewModel user, ChatMessageViewModel message = null);
        Task SendChatInteractiveParticipationWhisper(UserViewModel user, bool isChat = false, bool isInteractive = false);
    }

    public class ModerationService : IModerationService
    {
        public const string ModerationReasonSpecialIdentifier = "moderationreason";

        public const string BannedWordRegexFormat = "(^|[^\\w]){0}([^\\w]|$)";
        public const string BannedWordWildcardRegexFormat = "\\S*";

        public static LockedList<string> CommunityFilteredWords { get; set; } = new LockedList<string>();

        private const string CommunityFilteredWordsFilePath = "Assets\\CommunityBannedWords.txt";

        private const int MinimumMessageLengthForPercentageModeration = 5;

        private static readonly Regex EmoteRegex = new Regex(":\\w+ ");
        private static readonly Regex EmojiRegex = new Regex(@"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD");
        private static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

        public async Task Initialize()
        {
            if (ChannelSession.Services.FileService.FileExists(ModerationService.CommunityFilteredWordsFilePath))
            {
                string text = await ChannelSession.Services.FileService.ReadFile(ModerationService.CommunityFilteredWordsFilePath);
                ModerationService.CommunityFilteredWords = new LockedList<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public async Task<string> ShouldTextBeModerated(UserViewModel user, string text, bool containsLink = false)
        {
            string reason = null;

            if (user.IgnoreForQueries)
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

        public async Task<string> ShouldTextBeFilteredWordModerated(UserViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationFilteredWordsExcempt))
            {
                if (ChannelSession.Settings.ModerationUseCommunityFilteredWords)
                {
                    foreach (string word in ModerationService.CommunityFilteredWords)
                    {
                        if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, Regex.Escape(word)), RegexOptions.IgnoreCase))
                        {
                            return "Banned Word";
                        }
                    }
                }

                foreach (string word in ChannelSession.Settings.FilteredWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, Regex.Escape(word)), RegexOptions.IgnoreCase))
                    {
                        return "The following word is not allowed: " + word;
                    }
                }

                foreach (string word in ChannelSession.Settings.BannedWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, Regex.Escape(word)), RegexOptions.IgnoreCase))
                    {
                        await ChannelSession.Services.Chat.BanUser(user);
                        return "The following word is banned: " + word;
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeExcessiveModerated(UserViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationChatTextExcempt))
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
                        return "Too Many Caps";
                    }
                }

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

                    count += leftOverText.Count(c => char.IsSymbol(c) || char.IsPunctuation(c));
                    messageSegments.AddRange(leftOverText.ToCharArray().Select(c => c.ToString()));

                    if (ChannelSession.Settings.ModerationPunctuationBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(messageSegments.Count, count);
                    }

                    if (count >= ChannelSession.Settings.ModerationPunctuationBlockCount)
                    {
                        return "Too Many Punctuation/Symbols/Emotes";
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeLinkModerated(UserViewModel user, string text, bool containsLink = false)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationBlockLinksExcempt))
            {
                if (ChannelSession.Settings.ModerationBlockLinks && (containsLink || LinkRegex.IsMatch(text)))
                {
                    return "No Links";
                }
            }

            return null;
        }

        public bool DoesUserMeetChatInteractiveParticipationRequirement(UserViewModel user, ChatMessageViewModel message = null)
        {
            if (ChannelSession.Settings.ModerationChatInteractiveParticipation != ModerationChatInteractiveParticipationEnum.None)
            {
                if (user == null)
                {
                    return false;
                }

                if (user.IgnoreForQueries)
                {
                    return true;
                }

                if (user.HasPermissionsTo(ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt))
                {
                    return true;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly && !user.HasPermissionsTo(UserRoleEnum.Follower))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly && !user.HasPermissionsTo(UserRoleEnum.Subscriber))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly && user.HasPermissionsTo(UserRoleEnum.Mod))
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

                TimeSpan viewingLength = TimeSpan.FromMinutes(user.Data.ViewingMinutes);
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

                if (message != null)
                {
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.EmotesSkillsOnly)
                    {
                        return message.ContainsOnlyEmotes() || message is MixerSkillChatMessageViewModel;
                    }
                    else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SkillsOnly)
                    {
                        return message is MixerSkillChatMessageViewModel;
                    }
                    else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.EmberSkillsOnly)
                    {
                        return message is MixerSkillChatMessageViewModel && ((MixerSkillChatMessageViewModel)message).Skill.IsEmbersSkill;
                    }
                }
            }
            return true;
        }

        public async Task SendChatInteractiveParticipationWhisper(UserViewModel user, bool isChat = false, bool isInteractive = false)
        {
            if (user != null)
            {
                string reason = string.Empty;
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly)
                {
                    reason = "Followers";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly)
                {
                    reason = "Subscribers";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly)
                {
                    reason = "Moderators";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.EmotesSkillsOnly)
                {
                    reason = "Emotes & Skills";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SkillsOnly)
                {
                    reason = "Skills";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.EmberSkillsOnly)
                {
                    reason = "Ember Skills";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour)
                {
                    reason = "accounts older than 1 hour";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay)
                {
                    reason = "accounts older than 1 day";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek)
                {
                    reason = "accounts older than 1 week";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes)
                {
                    reason = "viewers who have watched for 10 minutes";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes)
                {
                    reason = "viewers who have watched for 30 minutes";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour)
                {
                    reason = "viewers who have watched for 1 hour";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours)
                {
                    reason = "viewers who have watched for 2 hours";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenHours)
                {
                    reason = "viewers who have watched for 10 hours";
                }

                if (isChat)
                {
                    await ChannelSession.Services.Chat.Whisper(user, string.Format("Your message has been deleted because only {0} can participate currently.", reason));
                }
                else if (isInteractive)
                {
                    await ChannelSession.Services.Chat.Whisper(user, string.Format("Your interactive selection has been ignored because only {0} can participate currently.", reason));
                }
            }
        }

        private string PrepareTextForChecking(string text)
        {
            string result = text.ToLower();
            result = ChatAction.UserNameTagRegex.Replace(result, "");
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
