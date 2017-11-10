using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
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
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return CustomAction.asyncSemaphore; } }

        private Func<UserViewModel, IEnumerable<string>, Task> action;

        internal CustomAction(Func<UserViewModel, IEnumerable<string>, Task> action) : base(ActionTypeEnum.Custom) { this.action = action; }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { await this.action(user, arguments); }
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

    public class StreamerAgeChatCommand : PreMadeChatCommand
    {
        public StreamerAgeChatCommand()
            : base("Streamer Age", new List<string>() { "streamerage", "age" }, UserRole.User, 60)
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
            : base("Add Quote", new List<string>() { "addquote", "quoteadd" }, UserRole.Mod, 5)
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
                        await ChannelSession.BotChat.SendMessage("Added Quote: \"" + quote + "\"");
                    }
                }
            }));
        }
    }

    public class Timeout1ChatCommand : PreMadeChatCommand
    {
        public Timeout1ChatCommand()
            : base("Timeout 1", "timeout1", UserRole.Mod, 5)
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
                        await ChannelSession.BotChat.Whisper(user.UserName, "Usage: !timeout1 @USERNAME");
                    }
                }
            }));
        }
    }

    public class Timeout5ChatCommand : PreMadeChatCommand
    {
        public Timeout5ChatCommand()
            : base("Timeout 5", "timeout5", UserRole.Mod, 5)
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

                        await ChannelSession.BotChat.Whisper(username, "You have been timed out for 5 minutes");
                        await ChannelSession.Chat.TimeoutUser(username, 300);
                    }
                    else
                    {
                        await ChannelSession.BotChat.Whisper(user.UserName, "Usage: !timeout5 @USERNAME");
                    }
                }
            }));
        }
    }

    public class PurgeChatCommand : PreMadeChatCommand
    {
        public PurgeChatCommand()
            : base("Purge", "purge", UserRole.Mod, 5)
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

    public class BanChatCommand : PreMadeChatCommand
    {
        public BanChatCommand()
            : base("Ban", "ban", UserRole.Mod, 5)
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

                        UserViewModel bannedUser = ChatClientWrapper.ChatUsers.Values.FirstOrDefault(u => u.UserName.Equals(username));
                        if (bannedUser != null)
                        {
                            await ChannelSession.Connection.AddUserRoles(ChannelSession.Channel, bannedUser.GetModel(), new List<UserRole>() { UserRole.Banned });
                        }
                    }
                    else
                    {
                        await ChannelSession.BotChat.Whisper(user.UserName, "Usage: !ban @USERNAME");
                    }
                }
            }));
        }
    }
}
