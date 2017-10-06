using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
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

    public class FollowAgeChatCommand : PreMadeChatCommand
    {
        public FollowAgeChatCommand()
            : base("Follow Age", "followage", UserRole.Follower, 60)
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
                if (ChannelSession.BotChatClient != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.BotChatClient.SendMessage("Game: " + ChannelSession.Channel.type.name);
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

    public class MixerAgeChatCommand : PreMadeChatCommand
    {
        public MixerAgeChatCommand()
            : base("Mixer Age", "mixerage", UserRole.User, 60)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.BotChatClient != null)
                {
                    UserModel userModel = await ChannelSession.MixerConnection.Users.GetUser(user.ID);
                    await ChannelSession.BotChatClient.SendMessage(userModel.GetMixerAge());
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

    public class QuoteChatCommand : PreMadeChatCommand
    {
        public QuoteChatCommand()
            : base("Quote", "quote", UserRole.User, 60)
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

    public class AddQuoteChatCommand : PreMadeChatCommand
    {
        public AddQuoteChatCommand()
            : base("Add Quote", "addquote", UserRole.Mod, 5)
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

                    if (ChannelSession.BotChatClient != null)
                    {
                        await ChannelSession.BotChatClient.SendMessage("Added Quote #" + (ChannelSession.Settings.Quotes.Count - 1) + ": \"" + quote + "\"");
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
                if (ChannelSession.BotChatClient != null)
                {
                    UserWithChannelModel userModel = await ChannelSession.MixerConnection.Users.GetUser(user.ID);
                    await ChannelSession.BotChatClient.SendMessage(user.UserName + "'s Sparks: " + userModel.sparks);
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
                if (ChannelSession.BotChatClient != null)
                {
                    await ChannelSession.BotChatClient.SendMessage(ChannelSession.Channel.user.GetMixerAge());
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

    public class TitleChatCommand : PreMadeChatCommand
    {
        public TitleChatCommand()
            : base("Title", new List<string>() { "title", "stream" }, UserRole.User, 60)
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

    public class UptimeChatCommand : PreMadeChatCommand
    {
        public UptimeChatCommand()
            : base("Uptime", "uptime", UserRole.User, 60)
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
}
