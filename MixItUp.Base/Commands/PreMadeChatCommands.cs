using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    IEnumerable<StreamSessionsAnalyticModel> sessions = await MixerAPIHandler.MixerConnection.Channels.GetStreamSessions(ChannelSession.Channel, DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)));
                    if (sessions.Count() > 0)
                    {
                        StreamSessionsAnalyticModel session = sessions.OrderBy(s => s.dateTime).Last();

                        TimeSpan duration = DateTimeOffset.Now.Subtract(session.dateTime);

                        await MixerAPIHandler.BotChatClient.SendMessage("Start Time: " + session.dateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await MixerAPIHandler.BotChatClient.SendMessage("Stream is currently offline");
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    await ChannelSession.RefreshChannel();

                    await MixerAPIHandler.BotChatClient.SendMessage("Game: " + ChannelSession.Channel.type.name);
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    await ChannelSession.RefreshChannel();

                    await MixerAPIHandler.BotChatClient.SendMessage("Stream Title: " + ChannelSession.Channel.name);
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await MixerAPIHandler.BotChatClient.Whisper(username, "You have been timed out for 1 minute");
                        await MixerAPIHandler.ChatClient.TimeoutUser(username, 60);
                    }
                    else
                    {
                        await MixerAPIHandler.BotChatClient.Whisper(user.UserName, "Usage: !timeout @USERNAME");
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await MixerAPIHandler.ChatClient.PurgeUser(username);
                    }
                    else
                    {
                        await MixerAPIHandler.BotChatClient.Whisper(user.UserName, "Usage: !purge @USERNAME");
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    await MixerAPIHandler.BotChatClient.SendMessage(ChannelSession.Channel.user.GetMixerAge());
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    UserModel userModel = await MixerAPIHandler.MixerConnection.Users.GetUser(user.ID);
                    await MixerAPIHandler.BotChatClient.Whisper(userModel.username, userModel.GetMixerAge());
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    DateTimeOffset? followDate = await MixerAPIHandler.MixerConnection.Channels.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                    if (followDate != null)
                    {
                        await MixerAPIHandler.BotChatClient.Whisper(user.UserName, user.GetModel().GetFollowAge(followDate.GetValueOrDefault()));
                    }
                    else
                    {
                        await MixerAPIHandler.BotChatClient.Whisper(user.UserName, "You must follow the channel to use this");
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
                if (MixerAPIHandler.ChatClient != null)
                {
                    UserWithChannelModel userModel = await MixerAPIHandler.MixerConnection.Users.GetUser(user.ID);
                    await MixerAPIHandler.BotChatClient.Whisper(userModel.username, "Sparks: " + userModel.sparks);
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
                if (MixerAPIHandler.ChatClient != null && ChannelSession.Settings.Quotes.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(ChannelSession.Settings.Quotes.Count);
                    string quote = ChannelSession.Settings.Quotes[index];

                    await MixerAPIHandler.BotChatClient.SendMessage("Quote #" + (index + 1) + ": \"" + quote + "\"");
                }
            }));
        }
    }
}
