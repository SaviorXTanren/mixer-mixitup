using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Clients;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.NewAPI.Users;
using Twitch.Base.Models.V5.Emotes;
using Twitch.Base.Models.V5.Streams;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

namespace MixItUp.Base.Services.Twitch
{
    public class BetterTTVEmoteModel
    {
        public string id { get; set; }
        public string channel { get; set; }
        public string code { get; set; }
        public string imageType { get; set; }

        public string url { get { return string.Format("https://cdn.betterttv.net/emote/{0}/1x", this.id); } }
    }

    public interface ITwitchChatService
    {
        IDictionary<string, EmoteModel> Emotes { get; }
        IDictionary<string, BetterTTVEmoteModel> BetterTTVEmotes { get; }

        event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred;
        event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred;

        event EventHandler<TwitchChatMessageViewModel> OnMessageOccurred;

        bool IsUserConnected { get; }
        bool IsBotConnected { get; }

        Task<Result> ConnectUser();
        Task DisconnectUser();

        Task<Result> ConnectBot();
        Task DisconnectBot();

        Task Initialize();

        Task SendMessage(string message, bool sendAsStreamer = false);

        Task SendWhisperMessage(UserViewModel user, string message, bool sendAsStreamer = false);

        Task DeleteMessage(ChatMessageViewModel message);

        Task ClearMessages();

        Task ModUser(UserViewModel user);

        Task UnmodUser(UserViewModel user);

        Task TimeoutUser(UserViewModel user, int lengthInSeconds);

        Task BanUser(UserViewModel user);

        Task UnbanUser(UserViewModel user);

        Task RunCommercial(int lengthInSeconds);
    }

    public class TwitchChatService : PlatformServiceBase, ITwitchChatService
    {
        private static List<string> ExcludedDiagnosticPacketLogging = new List<string>() { "PING", ChatMessagePacketModel.CommandID, ChatUserJoinPacketModel.CommandID, ChatUserLeavePacketModel.CommandID };

        private const string HostChatMessageRegexPattern = "^\\w+ is now hosting you.$";

        public IDictionary<string, EmoteModel> Emotes { get { return this.emotes; } }
        private Dictionary<string, EmoteModel> emotes = new Dictionary<string, EmoteModel>();

        public IDictionary<string, BetterTTVEmoteModel> BetterTTVEmotes { get { return this.betterTTVEmotes; } }
        private Dictionary<string, BetterTTVEmoteModel> betterTTVEmotes = new Dictionary<string, BetterTTVEmoteModel>();

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

        private bool streamStartDetected = false;

        public TwitchChatService() { }

        public bool IsUserConnected { get { return this.userClient != null && this.userClient.IsOpen(); } }
        public bool IsBotConnected { get { return this.botClient != null && this.botClient.IsOpen(); } }

        public async Task<Result> ConnectUser()
        {
            if (ChannelSession.TwitchUserConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    try
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

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                });
            }
            return new Result("Twitch connection has not been established");
        }

        public async Task DisconnectUser()
        {
            try
            {
                if (this.userClient != null)
                {
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.userClient.OnSentOccurred -= Client_OnSentOccurred;
                    }
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

        public async Task<Result> ConnectBot()
        {
            if (ChannelSession.TwitchUserConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    try
                    {
                        this.cancellationTokenSource = new CancellationTokenSource();

                        this.botClient = new ChatClient(ChannelSession.TwitchBotConnection.Connection);

                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.botClient.OnSentOccurred += Client_OnSentOccurred;
                        }
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;
                        this.botClient.OnPingReceived += BotClient_OnPingReceived;

                        await this.botClient.Connect();

                        await Task.Delay(1000);

                        await this.botClient.AddCommandsCapability();
                        await this.botClient.AddTagsCapability();
                        await this.botClient.AddMembershipCapability();

                        await Task.Delay(1000);

                        await this.botClient.Join(ChannelSession.TwitchChannelNewAPI);

                        await Task.Delay(3000);

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                });
            }
            return new Result("Twitch connection has not been established");
        }

        public async Task DisconnectBot()
        {
            try
            {
                if (this.botClient != null)
                {
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.botClient.OnSentOccurred -= Client_OnSentOccurred;
                    }
                    this.botClient.OnDisconnectOccurred -= BotClient_OnDisconnectOccurred;
                    this.botClient.OnPingReceived -= BotClient_OnPingReceived;

                    await this.botClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.botClient = null;
        }

        public async Task Initialize()
        {
            List<Task> emoteTasks = new List<Task>();

            emoteTasks.Add(Task.Run(async() =>
            {
                foreach (EmoteModel emote in await ChannelSession.TwitchUserConnection.GetEmotesForUserV5(ChannelSession.TwitchUserV5))
                {
                    this.emotes[emote.code] = emote;
                }
            }));

            if (ChannelSession.Settings.ShowBetterTTVEmotes)
            {
                emoteTasks.Add(this.DownloadBetterTTVEmotes());
                emoteTasks.Add(this.DownloadBetterTTVEmotes(ChannelSession.TwitchChannelNewAPI.login));
            }

            await Task.WhenAll(emoteTasks);

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

        public async Task SendWhisperMessage(UserViewModel user, string message, bool sendAsStreamer = false)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    await client.SendWhisperMessage(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel(), message);
                }
            });
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.DeleteMessage(ChannelSession.TwitchChannelNewAPI, message.ID);
                }
            });
        }

        public async Task ClearMessages()
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.ClearChat(ChannelSession.TwitchChannelNewAPI);
                }
            });
        }

        public async Task ModUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.ModUser(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel());
                }
            });
        }

        public async Task UnmodUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.UnmodUser(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel());
                }
            });
        }

        public async Task TimeoutUser(UserViewModel user, int lengthInSeconds)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.TimeoutUser(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel(), lengthInSeconds);
                }
            });
        }

        public async Task BanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.BanUser(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel());
                }
            });
        }

        public async Task UnbanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.UnbanUser(ChannelSession.TwitchChannelNewAPI, user.GetTwitchNewAPIUserModel());
                }
            });
        }

        public async Task RunCommercial(int lengthInSeconds)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient();
                if (client != null)
                {
                    await client.RunCommercial(ChannelSession.TwitchChannelNewAPI, lengthInSeconds);
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

        private async void BotClient_OnPingReceived(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Twitch Bot Client - Ping");
            await this.botClient.Pong();
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

        private async void UserClient_OnMessageReceived(object sender, ChatMessagePacketModel message)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message))
            {
                if (!string.IsNullOrEmpty(message.UserLogin) && message.UserLogin.Equals("jtv"))
                {
                    if (Regex.IsMatch(message.Message, TwitchChatService.HostChatMessageRegexPattern))
                    {
                        string hoster = message.Message.Substring(0, message.Message.IndexOf(' '));
                        UserViewModel user = ChannelSession.Services.User.GetUserByUsername(hoster);
                        if (user == null)
                        {
                            UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(hoster);
                            if (twitchUser != null)
                            {
                                user = await ChannelSession.Services.User.AddOrUpdateUser(twitchUser);
                            }
                        }

                        if (user != null)
                        {
                            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelHosted, user);
                            if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                            {
                                foreach (UserCurrencyModel currency in ChannelSession.Settings.Currencies.Values)
                                {
                                    currency.AddAmount(user.Data, currency.OnHostBonus);
                                }

                                GlobalEvents.HostOccurred(new Tuple<UserViewModel, int>(user, 0));

                                await ChannelSession.Services.Events.PerformEvent(trigger);

                                await this.AddAlertChatMessage(user, string.Format("{0} hosted the channel", user.Username));
                            }
                        }
                    }
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
                    Logger.Log(LogLevel.Debug, string.Format("Twitch Client Packet Received: {0}", JSONSerializerHelper.SerializeToString(packet)));
                }

                if (packet.Command.Equals("USERNOTICE"))
                {
                    if (packet.Tags.ContainsKey("msg-id") && packet.Tags["msg-id"].Equals("raid"))
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

                        EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelRaided, user);
                        if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestHostUserData] = user.Data;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestHostViewerCountData] = viewerCount;

                            foreach (UserCurrencyModel currency in ChannelSession.Settings.Currencies.Values)
                            {
                                currency.AddAmount(user.Data, currency.OnHostBonus);
                            }

                            GlobalEvents.HostOccurred(new Tuple<UserViewModel, int>(user, viewerCount));

                            trigger.SpecialIdentifiers["hostviewercount"] = viewerCount.ToString();
                            await ChannelSession.Services.Events.PerformEvent(trigger);

                            await this.AddAlertChatMessage(user, string.Format("{0} raided with {1} viewers", user.Username, viewerCount));
                        }
                    }
                }
                else if (packet.Command.Equals("HOSTTARGET"))
                {
                    string[] splits = packet.Get1SkippedParameterText.Split(new char[] { ' ' });
                    if (splits.Length > 0)
                    {
                        bool isUnhost = splits[0].Equals("-");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            await Task.Delay(30000);

                            StreamModel stream = await ChannelSession.TwitchUserConnection.GetV5LiveStream(ChannelSession.TwitchChannelV5);

                            EventTrigger trigger = null;
                            if (isUnhost)
                            {
                                if (stream != null && stream.id > 0 && !stream.is_playlist)
                                {
                                    this.streamStartDetected = true;
                                    trigger = new EventTrigger(EventTypeEnum.TwitchChannelStreamStart, ChannelSession.GetCurrentUser());
                                }
                            }
                            else if (this.streamStartDetected)
                            {
                                if (stream == null || stream.id == 0)
                                {
                                    trigger = new EventTrigger(EventTypeEnum.TwitchChannelStreamStop, ChannelSession.GetCurrentUser());
                                }
                            }

                            if (trigger != null && ChannelSession.Services.Events.CanPerformEvent(trigger))
                            {
                                await ChannelSession.Services.Events.PerformEvent(trigger);
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, message, ChannelSession.Settings.ChatEventAlertsColorScheme));
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred("Twitch User Chat");

            Result result;
            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Twitch User Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred("Twitch Bot Chat");

            Result result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Twitch Bot Chat");
        }

        private async Task DownloadBetterTTVEmotes(string channelName = null)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    JObject jobj = await client.GetJObjectAsync((!string.IsNullOrEmpty(channelName)) ? "https://api.betterttv.net/2/channels/" + channelName : "https://api.betterttv.net/2/emotes");
                    if (jobj != null && jobj.ContainsKey("emotes"))
                    {
                        JArray array = (JArray)jobj["emotes"];
                        foreach (BetterTTVEmoteModel emote in array.ToTypedArray<BetterTTVEmoteModel>())
                        {
                            this.betterTTVEmotes[emote.code] = emote;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}