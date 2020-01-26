using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Clients;
using Twitch.Base.Models.Clients.Chat;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

namespace MixItUp.Base.Services.Twitch
{
    public interface ITwitchChatService
    {
        event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred;
        event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred;

        event EventHandler<TwitchChatMessageViewModel> OnMessageOccurred;

        Task<bool> ConnectUser();
        Task DisconnectUser();

        Task Initialize();

        Task SendMessage(string message, bool sendAsStreamer = false);
    }

    public class TwitchChatService : PlatformServiceBase, ITwitchChatService
    {
        private static List<string> ExcludedDiagnosticPacketLogging = new List<string>() { "PING", ChatMessagePacketModel.CommandID, ChatUserJoinPacketModel.CommandID, ChatUserLeavePacketModel.CommandID };

        public event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred = delegate { };
        public event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred = delegate { };

        public event EventHandler<TwitchChatMessageViewModel> OnMessageOccurred = delegate { };

        private ChatClient userClient;
        private ChatClient botClient;

        private CancellationTokenSource cancellationTokenSource;

        private const int userJoinLeaveEventsTotalToProcess = 25;
        private SemaphoreSlim userJoinLeaveEventsSemaphore = new SemaphoreSlim(1);
        private HashSet<string> userJoinEvents = new HashSet<string>();
        private HashSet<string> userLeaveEvents = new HashSet<string>();

        private List<string> initialUserLogins = new List<string>();

        public TwitchChatService() { }

        public async Task<bool> ConnectUser()
        {
            return await this.AttemptConnect(async () =>
            {
                try
                {
                    if (ChannelSession.TwitchUserConnection != null)
                    {
                        this.cancellationTokenSource = new CancellationTokenSource();

                        this.userClient = new ChatClient(ChannelSession.TwitchUserConnection.Connection);

                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred += Client_OnSentOccurred;
                        }
                        this.userClient.OnPacketReceived += Client_OnPacketReceived;
                        this.userClient.OnDisconnectOccurred += UserClient_OnDisconnectOccurred;
                        this.userClient.OnPingReceived += UserClient_OnPingReceived;
                        this.userClient.OnUserJoinReceived += UserClient_OnUserJoinReceived;
                        this.userClient.OnUserLeaveReceived += UserClient_OnUserLeaveReceived;
                        this.userClient.OnMessageReceived += UserClient_OnMessageReceived;

                        this.initialUserLogins.Clear();
                        this.userClient.OnUserListReceived += UserClient_OnUserListReceived;
                        await this.userClient.Connect();

                        await Task.Delay(1000);

                        await this.userClient.AddCommandsCapability();
                        await this.userClient.AddTagsCapability();
                        await this.userClient.AddMembershipCapability();

                        await Task.Delay(1000);

                        await this.userClient.Join(ChannelSession.TwitchChannelNewAPI);

                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 2500, this.ChatterJoinLeaveBackground);

                        await Task.Delay(3000);
                        this.userClient.OnUserListReceived -= UserClient_OnUserListReceived;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                return false;
            });
        }

        public async Task DisconnectUser()
        {
            try
            {
                if (this.userClient != null)
                {
                    this.userClient.OnSentOccurred -= Client_OnSentOccurred;
                    this.userClient.OnPacketReceived -= Client_OnPacketReceived;
                    this.userClient.OnDisconnectOccurred -= UserClient_OnDisconnectOccurred;
                    this.userClient.OnPingReceived -= UserClient_OnPingReceived;
                    this.userClient.OnUserListReceived -= UserClient_OnUserListReceived;
                    this.userClient.OnUserJoinReceived -= UserClient_OnUserJoinReceived;
                    this.userClient.OnUserLeaveReceived -= UserClient_OnUserLeaveReceived;
                    this.userClient.OnMessageReceived -= UserClient_OnMessageReceived;

                    await this.userClient.Disconnect();
                }

                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.userClient = null;
        }

        public async Task Initialize()
        {
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                foreach (string user in this.initialUserLogins)
                {
                    this.userJoinEvents.Add(user);
                }
                return Task.FromResult(0);
            });
            this.initialUserLogins.Clear();
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    await client.SendMessage(ChannelSession.TwitchChannelNewAPI, message);
                }
            });
        }

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private async Task ChatterJoinLeaveBackground(CancellationToken cancellationToken)
        {
            List<string> joinsToProcess = new List<string>();
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                for (int i = 0; i < userJoinLeaveEventsTotalToProcess && i < this.userJoinEvents.Count(); i++)
                {
                    string chatUser = this.userJoinEvents.First();
                    joinsToProcess.Add(chatUser);
                    this.userJoinEvents.Remove(chatUser);
                }
                return Task.FromResult(0);
            });

            if (joinsToProcess.Count > 0)
            {
                List<UserViewModel> processedUsers = new List<UserViewModel>();
                foreach (string chatUser in joinsToProcess)
                {
                    TwitchNewAPI.Users.UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(chatUser);
                    if (twitchUser != null)
                    {
                        UserViewModel user = await ChannelSession.Services.User.AddOrUpdateUser(twitchUser);
                        if (user != null)
                        {
                            processedUsers.Add(user);
                        }
                    }
                }
                this.OnUsersJoinOccurred(this, processedUsers);
            }

            List<string> leavesToProcess = new List<string>();
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                for (int i = 0; i < userJoinLeaveEventsTotalToProcess && i < this.userLeaveEvents.Count(); i++)
                {
                    string chatUser = this.userLeaveEvents.First();
                    leavesToProcess.Add(chatUser);
                    this.userLeaveEvents.Remove(chatUser);
                }
                return Task.FromResult(0);
            });

            if (leavesToProcess.Count > 0)
            {
                List<UserViewModel> processedUsers = new List<UserViewModel>();
                foreach (string chatUser in leavesToProcess)
                {
                    if (!string.IsNullOrEmpty(chatUser))
                    {
                        UserViewModel user = await ChannelSession.Services.User.RemoveUserByTwitchLogin(chatUser);
                        if (user != null)
                        {
                            processedUsers.Add(user);
                        }
                    }
                }
                this.OnUsersLeaveOccurred(this, processedUsers);
            }
        }

        private async void UserClient_OnPingReceived(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Twitch User Client - Ping");
            await this.userClient.Pong();
        }

        private async void UserClient_OnUserJoinReceived(object sender, ChatUserJoinPacketModel userJoin)
        {
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                if (!string.IsNullOrEmpty(userJoin.UserLogin))
                {
                    this.userJoinEvents.Add(userJoin.UserLogin);
                }
                return Task.FromResult(0);
            });
        }

        private async void UserClient_OnUserLeaveReceived(object sender, ChatUserLeavePacketModel userLeave)
        {
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                if (!string.IsNullOrEmpty(userLeave.UserLogin))
                {
                    this.userLeaveEvents.Add(userLeave.UserLogin);
                }
                return Task.FromResult(0);
            });
        }

        private void UserClient_OnMessageReceived(object sender, ChatMessagePacketModel message)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message))
            {
                if (string.IsNullOrEmpty(message.UserID) || message.UserLogin.Equals("jtv"))
                {
                    Logger.Log(SerializerHelper.SerializeToString(message));
                }
                else
                {
                    this.OnMessageOccurred(this, new TwitchChatMessageViewModel(message));
                }
            }
        }

        private void UserClient_OnUserListReceived(object sender, ChatUsersListPacketModel userList)
        {
            this.initialUserLogins.AddRange(userList.UserLogins);
        }

        private async void Client_OnPacketReceived(object sender, ChatRawPacketModel packet)
        {
            if (!TwitchChatService.ExcludedDiagnosticPacketLogging.Contains(packet.Command))
            {
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    Logger.Log(LogLevel.Debug, string.Format("Twitch Client Packet Received: {0} - {1} - {2} - {3} - {4}", packet.Command, packet.Prefix,
                        JSONSerializerHelper.SerializeToString(packet.Parameters), JSONSerializerHelper.SerializeToString(packet.Tags), packet.RawText));
                }

                if (packet.Command.Equals("USERNOTICE"))
                {
                    if (packet.Tags.ContainsKey("msg-id") && packet.Tags.Equals("raid"))
                    {
                        UserViewModel user = ChannelSession.Services.User.GetUserByUsername(packet.Tags["login"]);
                        if (user == null)
                        {
                            user = new UserViewModel(packet);
                        }

                        int viewerCount = 0;
                        if (packet.Tags.ContainsKey("msg-param-viewerCount"))
                        {
                            int.TryParse(packet.Tags["msg-param-viewerCount"], out viewerCount);
                        }

                        EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelHosted, user);
                        if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestHostUserData] = user;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestHostViewerCountData] = viewerCount;

                            foreach (UserCurrencyModel currency in ChannelSession.Settings.Currencies.Values)
                            {
                                currency.AddAmount(user.Data, currency.OnHostBonus);
                            }

                            GlobalEvents.HostOccurred(new Tuple<UserViewModel, int>(user, viewerCount));

                            trigger.SpecialIdentifiers["hostviewercount"] = viewerCount.ToString();
                            await ChannelSession.Services.Events.PerformEvent(trigger);
                        }
                        await this.AddAlertChatMessage(user, string.Format("{0} Hosted With {1} Viewers", user.Username, viewerCount));
                    }
                }
                else if (packet.Command.Equals("HOSTTARGET"))
                {
                    string[] splits = packet.Get1SkippedParameterText.Split(new char[] { ' ' });
                    if (splits.Length > 0)
                    {
                        string hostTarget = splits[0];
                    }
                }
            }
        }

        private void Client_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Twitch Chat Packet Sent: {0}", packet));
        }

        private async Task AddAlertChatMessage(UserViewModel user, string message)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, message, ChannelSession.Settings.ChatEventAlertsColorScheme));
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred("Twitch User Chat");

            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);
            }
            while (!await this.ConnectUser());

            ChannelSession.ReconnectionOccurred("Twitch User Chat");
        }
    }
}