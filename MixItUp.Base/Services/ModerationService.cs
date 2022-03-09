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
        private static readonly Regex EmojiRegex = new Regex(@"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD");
        private static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

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
