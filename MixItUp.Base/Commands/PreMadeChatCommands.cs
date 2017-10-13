using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class PreMadeChatCommandSettings
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public UserRole Permissions { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettings() { }

        public PreMadeChatCommandSettings(PreMadeChatCommand command)
        {
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
            this.Permissions = command.Permissions;
            this.Cooldown = command.Cooldown;
        }
    }

    internal class CustomAction : ActionBase
    {
        private Func<UserViewModel, IEnumerable<string>, Task> action;

        internal CustomAction(Func<UserViewModel, IEnumerable<string>, Task> action) : base(ActionTypeEnum.Custom) { this.action = action; }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments) { await this.action(user, arguments); }
    }

    public class PreMadeChatCommand : ChatCommand
    {
        public PreMadeChatCommand(string name, string command, UserRole lowestAllowedRole, int cooldown) : base(name, command, lowestAllowedRole, cooldown) { }

        public PreMadeChatCommand(string name, List<string> commands, UserRole lowestAllowedRole, int cooldown) : base(name, commands, lowestAllowedRole, cooldown) { }

        public void UpdateFromSettings(PreMadeChatCommandSettings settings)
        {
            this.IsEnabled = settings.IsEnabled;
            this.Permissions = settings.Permissions;
            this.Cooldown = settings.Cooldown;
        }
    }

    public class MixItUpChatCommand : PreMadeChatCommand
    {
        public MixItUpChatCommand()
            : base("Mix It Up", "mixitup", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    await ChannelSession.BotChat.SendMessage("This channel uses the Mix It Up app to improve their stream. Check out https://github.com/SaviorXTanren/mixer-mixitup for more information!");
                }
            }));
        }
    }

    public class CommandsChatCommand : PreMadeChatCommand
    {
        public CommandsChatCommand()
            : base("Commands", "commands", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    await ChannelSession.BotChat.SendMessage("All common chat commands can be found here: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Pre-Made-Chat-Commands. For commands specific to this stream, ask your streamer/moderator.");
                }
            }));
        }
    }

    public class FollowAgeChatCommand : PreMadeChatCommand
    {
        public FollowAgeChatCommand()
            : base("Follow Age", "followage", UserRole.Follower, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    DateTimeOffset? followDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                    if (followDate != null)
                    {
                        await ChannelSession.BotChat.SendMessage(user.GetModel().GetFollowAge(followDate.GetValueOrDefault()));
                    }
                }
            }));
        }
    }

    public class GameChatCommand : PreMadeChatCommand
    {
        public GameChatCommand()
            : base("Game", new List<string>() { "game", "status" }, UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.BotChat.SendMessage("Game: " + ChannelSession.Channel.type.name);
                }
            }));
        }
    }

    public class GiveawayChatCommand : PreMadeChatCommand
    {
        public GiveawayChatCommand()
            : base("Giveaway", "giveaway", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    if (ChannelSession.Giveaway.IsEnabled)
                    {
                        await ChannelSession.BotChat.SendMessage(string.Format("There is a giveaway running for {0} for {1}! You must be present in chat to win to receive this giveaway.", ChannelSession.Giveaway.Item, ChannelSession.Giveaway.Type));
                    }
                    else
                    {
                        await ChannelSession.BotChat.SendMessage("A giveaway is not currently running at this time");
                    }
                }
            }));
        }
    }

    public class MixerAgeChatCommand : PreMadeChatCommand
    {
        public MixerAgeChatCommand()
            : base("Mixer Age", "mixerage", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    UserModel userModel = await ChannelSession.Connection.GetUser(user.GetModel());
                    await ChannelSession.BotChat.SendMessage(userModel.GetMixerAge());
                }
            }));
        }
    }

    public class PurgeChatCommand : PreMadeChatCommand
    {
        public PurgeChatCommand()
            : base("Purge", "purge", UserRole.Mod, 0)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.Chat.PurgeUser(username);
                    }
                    else
                    {
                        await ChannelSession.BotChat.Whisper(user.UserName, "Usage: !purge @USERNAME");
                    }
                }
            }));
        }
    }

    public class QuoteChatCommand : PreMadeChatCommand
    {
        public QuoteChatCommand()
            : base("Quote", "quote", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null && ChannelSession.Settings.QuotesEnabled && ChannelSession.Settings.Quotes.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(ChannelSession.Settings.Quotes.Count);
                    string quote = ChannelSession.Settings.Quotes[index];

                    await ChannelSession.BotChat.SendMessage("Quote #" + (index + 1) + ": \"" + quote + "\"");
                }
            }));
        }
    }

    public class AddQuoteChatCommand : PreMadeChatCommand
    {
        public AddQuoteChatCommand()
            : base("Add Quote", "addquote quoteadd", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Settings.QuotesEnabled)
                {
                    StringBuilder quoteBuilder = new StringBuilder();
                    foreach (string arg in arguments)
                    {
                        quoteBuilder.Append(arg + " ");
                    }

                    string quote = quoteBuilder.ToString();
                    quote = quote.Trim(new char[] { ' ', '\'', '\"' });

                    ChannelSession.Settings.Quotes.Add(quote);
                    GlobalEvents.QuoteAdded(quote);

                    if (ChannelSession.BotChat != null)
                    {
                        await ChannelSession.BotChat.SendMessage("Added Quote #" + (ChannelSession.Settings.Quotes.Count - 1) + ": \"" + quote + "\"");
                    }
                }
            }));
        }
    }

    public class SparksChatCommand : PreMadeChatCommand
    {
        public SparksChatCommand()
            : base("Sparks", "sparks", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    UserWithChannelModel userModel = await ChannelSession.Connection.GetUser(user.GetModel());
                    await ChannelSession.BotChat.SendMessage(user.UserName + "'s Sparks: " + userModel.sparks);
                }
            }));
        }
    }

    public class StreamerAgeChatCommand : PreMadeChatCommand
    {
        public StreamerAgeChatCommand()
            : base("Streamer Age", "streamerage age", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    await ChannelSession.BotChat.SendMessage(ChannelSession.Channel.user.GetMixerAge());
                }
            }));
        }
    }

    public class TimeoutChatCommand : PreMadeChatCommand
    {
        public TimeoutChatCommand()
            : base("Timeout", "timeout", UserRole.Mod, 0)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.BotChat.Whisper(username, "You have been timed out for 1 minute");
                        await ChannelSession.Chat.TimeoutUser(username, 60);
                    }
                    else
                    {
                        await ChannelSession.BotChat.Whisper(user.UserName, "Usage: !timeout @USERNAME");
                    }
                }
            }));
        }
    }

    public class TitleChatCommand : PreMadeChatCommand
    {
        public TitleChatCommand()
            : base("Title", new List<string>() { "title", "stream" }, UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.BotChat.SendMessage("Stream Title: " + ChannelSession.Channel.name);
                }
            }));
        }
    }

    public class UptimeChatCommand : PreMadeChatCommand
    {
        public UptimeChatCommand()
            : base("Uptime", "uptime", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChat != null)
                {
                    IEnumerable<StreamSessionsAnalyticModel> sessions = await ChannelSession.Connection.GetStreamSessions(ChannelSession.Channel, DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)));
                    if (sessions.Count() > 0)
                    {
                        StreamSessionsAnalyticModel session = sessions.OrderBy(s => s.dateTime).Last();

                        TimeSpan duration = DateTimeOffset.Now.Subtract(session.dateTime);

                        await ChannelSession.BotChat.SendMessage("Start Time: " + session.dateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await ChannelSession.BotChat.SendMessage("Stream is currently offline");
                    }
                }
            }));
        }
    }

    public class JoinGameChatCommand : PreMadeChatCommand
    {
        public JoinGameChatCommand()
            : base("Join Game", new List<string>() { "joingame", "join" }, UserRole.User, 0)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.JoinGameQueueEnabled && ChannelSession.BotChat != null)
                {
                    int position = ChannelSession.JoinGameQueue.IndexOf(user);
                    if (position == -1)
                    {
                        ChannelSession.JoinGameQueue.Add(user);
                        position = ChannelSession.JoinGameQueue.Count;
                    }
                    await ChannelSession.BotChat.Whisper(user.UserName, "You are #" + position + " in the queue to play with " + ChannelSession.Channel.user.username + ".");
                }
            }));
        }
    }

    public class GameQueueChatCommand : PreMadeChatCommand
    {
        public GameQueueChatCommand()
            : base("Game Queue", new List<string>() { "queue", "gamequeue" }, UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.JoinGameQueueEnabled && ChannelSession.BotChat != null)
                {
                    int total = ChannelSession.JoinGameQueue.Count();

                    StringBuilder message = new StringBuilder();
                    message.Append("There are currently " + total + " waiting to play with " + ChannelSession.Channel.user.username + ".");

                    if (total > 0)
                    {
                        message.Append(" The following users are next up to play: ");

                        List<string> users = new List<string>();
                        for (int i = 0; i < total && i < 5; i++)
                        {
                            users.Add(ChannelSession.JoinGameQueue[i].UserName);
                        }

                        message.Append(string.Join(", ", users));
                        message.Append(".");
                    }

                    await ChannelSession.BotChat.SendMessage(message.ToString());
                }
            }));
        }
    }

    public class RemoveQueueChatCommand : PreMadeChatCommand
    {
        public RemoveQueueChatCommand()
            : base("Leave Game", new List<string>() { "leavegame", "leavequeue" }, UserRole.Mod, 0)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.JoinGameQueueEnabled && ChannelSession.BotChat != null)
                {
                    ChannelSession.JoinGameQueue.Remove(user);
                    await ChannelSession.BotChat.Whisper(user.UserName, "You have been removed from the queue to play with " + ChannelSession.Channel.user.username + ".");
                }
            }));
        }
    }
}
