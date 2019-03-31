using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public enum ModerationChatInteractiveParticipationEnum
    {
        None = 0,
        [Name("Account Is 1 Hour Old")]
        AccountHour = 1,
        [Name("Account Is 1 Day Old")]
        AccountDay = 2,
        [Name("Account Is 1 Week Old")]
        AccountWeek = 3,
        [Name("Account Is 1 Month Old")]
        AccountMonth = 4,
        [Name("Watched For 10 Minutes")]
        ViewingTenMinutes = 10,
        [Name("Watched For 30 Minutes")]
        ViewingThirtyMinutes = 11,
        [Name("Watched For 1 Hour")]
        ViewingOneHour = 12,
        [Name("Watched For 2 Hours")]
        ViewingTwoHours = 13,
        [Name("Watched For 10 Hours")]
        ViewingTenHours = 14,
        [Name("Follower Only")]
        FollowerOnly = 19,
        [Name("Subscriber Only")]
        SubscriberOnly = 20,
        [Name("Moderator Only")]
        ModeratorOnly = 30,
        [Name("Emotes & Skills Only")]
        EmotesSkillsOnly = 40,
    }

    public static class ModerationHelper
    {
        public const string ModerationReasonSpecialIdentifier = "moderationreason";

        public const string BannedWordRegexFormat = "(^|[^\\w]){0}([^\\w]|$)";
        public const string BannedWordWildcardRegexFormat = "\\S*";

        private const int MinimumMessageLengthForPercentageModeration = 5;

        private static readonly Regex EmoteRegex = new Regex(":\\w+ ");
        private static readonly Regex EmojiRegex = new Regex(@"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD");
        private static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

        public static async Task<string> ShouldBeModerated(UserViewModel user, string text, bool containsLink = false)
        {
            if (UserContainerViewModel.SpecialUserAccounts.Contains(user.UserName))
            {
                return null;
            }

            string reason = await ShouldBeFilteredWordModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationFilteredWordsApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldBeTextModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationChatTextApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldBeLinkModerated(user, text, containsLink);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationBlockLinksApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            return null;
        }

        public static async Task<string> ShouldBeFilteredWordModerated(UserViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationFilteredWordsExcempt))
            {
                if (ChannelSession.Settings.ModerationUseCommunityFilteredWords)
                {
                    foreach (string word in ChannelSession.Settings.CommunityFilteredWords)
                    {
                        if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word), RegexOptions.IgnoreCase))
                        {
                            return "Banned Word";
                        }
                    }
                }

                foreach (string word in ChannelSession.Settings.FilteredWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word), RegexOptions.IgnoreCase))
                    {
                        return "The following word is not allowed: " + word;
                    }
                }

                foreach (string word in ChannelSession.Settings.BannedWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word), RegexOptions.IgnoreCase))
                    {
                        await ChannelSession.Chat.BanUser(user);
                        return "The following word is banned: " + word;
                    }
                }
            }

            return null;
        }

        public static string ShouldBeTextModerated(UserViewModel user, string text)
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

        public static string ShouldBeLinkModerated(UserViewModel user, string text, bool containsLink = false)
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

        public static bool MeetsChatInteractiveParticipationRequirement(UserViewModel user)
        {
            if (ChannelSession.Settings.ModerationChatInteractiveParticipation != ModerationChatInteractiveParticipationEnum.None)
            {
                if (user == null)
                {
                    return false;
                }

                if (UserContainerViewModel.SpecialUserAccounts.Contains(user.UserName))
                {
                    return true;
                }

                if (user.HasPermissionsTo(ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt))
                {
                    return true;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly && !user.HasPermissionsTo(MixerRoleEnum.Follower))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly && !user.HasPermissionsTo(MixerRoleEnum.Subscriber))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly && user.HasPermissionsTo(MixerRoleEnum.Mod))
                {
                    return false;
                }

                if (user.MixerAccountDate.HasValue)
                {
                    TimeSpan accountLength = DateTimeOffset.Now - user.MixerAccountDate.GetValueOrDefault();
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
            }
            return true;
        }

        public static bool MeetsChatEmoteSkillsOnlyParticipationRequirement(UserViewModel user, ChatMessageViewModel message)
        {
            if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.EmotesSkillsOnly)
            {
                if (user.HasPermissionsTo(ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt))
                {
                    return true;
                }
                return message.IsAlert || message.ContainsOnlyEmotes();
            }
            return true;
        }

        public static async Task SendChatInteractiveParticipationWhisper(UserViewModel user, bool isChat = false, bool isInteractive = false)
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
                else if(ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour)
                {
                    reason = "accounts older than 1 hour";
                }
                else if(ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay)
                {
                    reason = "accounts older than 1 day";
                }
                else if(ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek)
                {
                    reason = "accounts older than 1 week";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes)
                {
                    reason = "viewers who have watched for 10 minutes";
                }
                else if(ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes)
                {
                    reason = "viewers who have watched for 30 minutes";
                }
                else if(ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour)
                {
                    reason = "viewers who have watched for 1 hour";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours)
                {
                    reason = "viewers who have watched for 2 hours";
                }

                if (isChat)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("Your message has been deleted because only {0} can participate currently.", reason));
                }
                else if (isInteractive)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("Your interactive selection has been ignored because only {0} can participate currently.", reason));
                }
            }
        }

        private static string PrepareTextForChecking(string text)
        {
            string result = text.ToLower();
            result = ChatAction.UserNameTagRegex.Replace(result, "");
            return result;
        }

        private static int ConvertCountToPercentage(int length, int count)
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
