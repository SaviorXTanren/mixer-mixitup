using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class ModerationHelper
    {
        private const int MinimumMessageLengthForPercentageModeration = 5;

        private static readonly string BannedWordRegexFormat = "(^|\\s){0}(\\s|$)";

        private static readonly Regex EmoteRegex = new Regex(":\\w+ ");
        private static readonly Regex EmojiRegex = new Regex(@"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD");
        private static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

        public static async Task<string> ShouldBeModerated(UserViewModel user, string text, bool containsLink = false)
        {
            string reason = await ShouldBeFilteredWordModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }

            reason = ShouldBeTextModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }

            reason = ShouldBeLinkModerated(user, text, containsLink);
            if (!string.IsNullOrEmpty(reason))
            {
                return reason;
            }

            return null;
        }

        public static async Task<string> ShouldBeFilteredWordModerated(UserViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (user.PrimaryRole < ChannelSession.Settings.ModerationFilteredWordsExcempt)
            {
                if (ChannelSession.Settings.ModerationUseCommunityFilteredWords)
                {
                    foreach (string word in ChannelSession.Settings.CommunityFilteredWords)
                    {
                        if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word)))
                        {
                            return "Banned Word";
                        }
                    }
                }

                foreach (string word in ChannelSession.Settings.FilteredWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word)))
                    {
                        return "The following word is not allowed: " + word;
                    }
                }

                foreach (string word in ChannelSession.Settings.BannedWords)
                {
                    if (Regex.IsMatch(text, string.Format(BannedWordRegexFormat, word)))
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

            if (user.PrimaryRole < ChannelSession.Settings.ModerationChatTextExcempt)
            {
                if (ChannelSession.Settings.ModerationCapsBlockCount > 0)
                {
                    int count = text.Count(c => char.IsUpper(c));
                    if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(text, count);
                    }

                    if (count >= ChannelSession.Settings.ModerationCapsBlockCount)
                    {
                        return "Too Many Caps";
                    }
                }

                if (ChannelSession.Settings.ModerationPunctuationBlockCount > 0)
                {
                    int count = text.Count(c => char.IsSymbol(c) || char.IsPunctuation(c));
                    if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(text, count);
                    }

                    if (count >= ChannelSession.Settings.ModerationPunctuationBlockCount)
                    {
                        return "Too Many Punctuation/Symbols";
                    }
                }

                if (ChannelSession.Settings.ModerationEmoteBlockCount > 0)
                {
                    MatchCollection emoteMatches = EmoteRegex.Matches(text);
                    MatchCollection emojiMatches = EmojiRegex.Matches(text);
                    int count = emoteMatches.Count + emojiMatches.Count;
                    if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                    {
                        List<Match> matches = new List<Match>();
                        foreach (Match match in emoteMatches)
                        {
                            matches.Add(match);
                        }
                        foreach (Match match in emojiMatches)
                        {
                            matches.Add(match);
                        }

                        string leftOverText = text.ToString();
                        foreach (Match match in matches)
                        {
                            leftOverText = leftOverText.Replace(match.Value, "");
                        }

                        int messageLength = leftOverText.Count() + matches.Count;

                        if (messageLength >= MinimumMessageLengthForPercentageModeration)
                        {
                            count = (int)(((double)count) / ((double)messageLength) * 100.0);
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    if (count >= ChannelSession.Settings.ModerationEmoteBlockCount)
                    {
                        return "Too Many Emotes";
                    }
                }
            }

            return null;
        }

        public static string ShouldBeLinkModerated(UserViewModel user, string text, bool containsLink = false)
        {
            text = PrepareTextForChecking(text);

            if (user.PrimaryRole < ChannelSession.Settings.ModerationBlockLinksExcempt)
            {
                if (ChannelSession.Settings.ModerationBlockLinks && (containsLink || LinkRegex.IsMatch(text)))
                {
                    return "No Links";
                }
            }

            return null;
        }

        public static async Task SendModerationWhisper(UserViewModel user, string moderationReason)
        {
            string whisperMessage = " due to chat moderation for the following reason: " + moderationReason + ". Please watch what you type in chat or further actions may be taken.";

            user.ChatOffenses++;
            if (ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount > 0 && user.ChatOffenses >= ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount)
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You have been timed out from chat for 5 minutes" + whisperMessage);
                await ChannelSession.Chat.TimeoutUser(user.UserName, 300);
            }
            else if (ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount > 0 && user.ChatOffenses >= ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount)
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You have been timed out from chat for 1 minute" + whisperMessage);
                await ChannelSession.Chat.TimeoutUser(user.UserName, 60);
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, "Your message has been deleted" + whisperMessage);
            }
        }

        private static string PrepareTextForChecking(string text)
        {
            string result = text.ToLower();
            result = ChatAction.UserNameTagRegex.Replace(result, "");
            return result;
        }

        private static int ConvertCountToPercentage(string text, int count)
        {
            if (text.Count() >= MinimumMessageLengthForPercentageModeration)
            {
                return (int)(((double)count) / ((double)text.Count()) * 100.0);
            }
            else
            {
                return 0;
            }
        }
    }
}
