using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    internal class CustomAction : ActionBase
    {
        private Func<UserViewModel, IEnumerable<string>, Task> action;

        internal CustomAction(Func<UserViewModel, IEnumerable<string>, Task> action) : base(ActionTypeEnum.Custom) { this.action = action; }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments) { await this.action(user, arguments); }
    }

    public class UptimeChatCommand : ChatCommand
    {
        public UptimeChatCommand()
            : base("Uptime", "uptime", UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    IEnumerable<StreamSessionsAnalyticModel> sessions = await ChannelSession.MixerConnection.Channels.GetStreamSessions(ChannelSession.Channel, DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)));
                    if (sessions.Count() > 0)
                    {
                        StreamSessionsAnalyticModel session = sessions.OrderBy(s => s.dateTime).Last();

                        TimeSpan duration = DateTimeOffset.Now.Subtract(session.dateTime);

                        await ChannelSession.BotChatClient.SendMessage("Start Time: " + session.dateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await ChannelSession.BotChatClient.SendMessage("Stream is currently offline");
                    }
                }
            }));
        }
    }

    public class GameChatCommand : ChatCommand
    {
        public GameChatCommand()
            : base("Game", new List<string>() { "game", "status" }, UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.BotChatClient.SendMessage("Game: " + ChannelSession.Channel.type.name);
                }
            }));
        }
    }

    public class TitleChatCommand : ChatCommand
    {
        public TitleChatCommand()
            : base("Title", new List<string>() { "title", "stream" }, UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.BotChatClient.SendMessage("Stream Title: " + ChannelSession.Channel.name);
                }
            }));
        }
    }

    public class TimeoutChatCommand : ChatCommand
    {
        public TimeoutChatCommand()
            : base("Timeout", "timeout", UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.BotChatClient.Whisper(username, "You have been timed out for 1 minute");
                        await ChannelSession.ChatClient.TimeoutUser(username, 60);
                    }
                    else
                    {
                        await ChannelSession.BotChatClient.Whisper(user.UserName, "Usage: !timeout @USERNAME");
                    }
                }
            }));
        }
    }

    public class PurgeChatCommand : ChatCommand
    {
        public PurgeChatCommand()
            : base("Purge", "purge", UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.ChatClient.PurgeUser(username);
                    }
                    else
                    {
                        await ChannelSession.BotChatClient.Whisper(user.UserName, "Usage: !purge @USERNAME");
                    }
                }
            }));
        }
    }

    public class StreamerAgeChatCommand : ChatCommand
    {
        public StreamerAgeChatCommand()
            : base("Streamer Age", "streamerage", UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    await ChannelSession.BotChatClient.SendMessage(ChannelSession.Channel.user.GetMixerAge());
                }
            }));
        }
    }

    public class MixerAgeChatCommand : ChatCommand
    {
        public MixerAgeChatCommand()
            : base("Mixer Age", "mixerage", UserRole.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    UserModel userModel = await ChannelSession.MixerConnection.Users.GetUser(user.ID);
                    await ChannelSession.BotChatClient.Whisper(userModel.username, userModel.GetMixerAge());
                }
            }));
        }
    }

    public class FollowAgeChatCommand : ChatCommand
    {
        public FollowAgeChatCommand()
            : base("Follow Age", "followage", UserRole.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    DateTimeOffset? followDate = await ChannelSession.MixerConnection.Channels.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                    if (followDate != null)
                    {
                        await ChannelSession.BotChatClient.SendMessage(user.GetModel().GetFollowAge(followDate.GetValueOrDefault()));
                    }
                    else
                    {
                        await ChannelSession.BotChatClient.Whisper(user.UserName, "You must follow the channel to use this");
                    }
                }
            }));
        }
    }

    public class SparksChatCommand : ChatCommand
    {
        public SparksChatCommand()
            : base("Sparks", "sparks", UserRole.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    UserWithChannelModel userModel = await ChannelSession.MixerConnection.Users.GetUser(user.ID);
                    await ChannelSession.BotChatClient.SendMessage("Sparks: " + userModel.sparks);
                }
            }));
        }
    }

    public class QuoteChatCommand : ChatCommand
    {
        public QuoteChatCommand()
            : base("Quotes", "quote", UserRole.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null && ChannelSession.Settings.QuotesEnabled && ChannelSession.Settings.Quotes.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(ChannelSession.Settings.Quotes.Count);
                    string quote = ChannelSession.Settings.Quotes[index];

                    await ChannelSession.BotChatClient.SendMessage("Quote #" + (index + 1) + ": \"" + quote + "\"");
                }
            }));
        }
    }

    public class GiveawayChatCommand : ChatCommand
    {
        public GiveawayChatCommand()
            : base("Giveaway", "giveaway", UserRole.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    if (ChannelSession.Giveaway.IsEnabled)
                    {
                        await ChannelSession.BotChatClient.SendMessage(string.Format("There is a giveaway running for {0} for {1}! You must be present in chat to win to receive this giveaway.", ChannelSession.Giveaway.Item, ChannelSession.Giveaway.Type));
                    }
                    else
                    {
                        await ChannelSession.BotChatClient.SendMessage("A giveaway is not currently running at this time");
                    }
                }
            }));
        }
    }
}
