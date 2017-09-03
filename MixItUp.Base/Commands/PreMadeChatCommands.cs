using Mixer.Base.ViewModel.Chat;
using MixItUp.Base.Actions;
using System;
using System.Threading.Tasks;
using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Mixer.Base.Model.Channel;
using System.Text;
using MixItUp.Base.Util;
using Mixer.Base.Model.User;

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

                        await MixerAPIHandler.ChatClient.SendMessage("Start Time: " + session.dateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await MixerAPIHandler.ChatClient.SendMessage("Stream is currently offline");
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

                    await MixerAPIHandler.ChatClient.SendMessage("Game: " + ChannelSession.Channel.type.name);
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

                    await MixerAPIHandler.ChatClient.SendMessage("Stream Title: " + ChannelSession.Channel.name);
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

                        await MixerAPIHandler.ChatClient.Whisper(username, "You have been timed out for 1 minute");
                        await MixerAPIHandler.ChatClient.TimeoutUser(username, 60);
                    }
                    else
                    {
                        await MixerAPIHandler.ChatClient.Whisper(user.UserName, "Usage: !timeout @USERNAME");
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
                        await MixerAPIHandler.ChatClient.Whisper(user.UserName, "Usage: !purge @USERNAME");
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
                    await MixerAPIHandler.ChatClient.SendMessage(ChannelSession.Channel.user.GetMixerAge());
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
                    await MixerAPIHandler.ChatClient.Whisper(userModel.username, userModel.GetMixerAge());
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
                }
            }));
        }
    }
}
