using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base;
using Twitch.Base.Clients;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Users;

namespace TwitchLogger
{
    public class Program
    {
        public const string clientID = "xm067k6ffrsvt8jjngyc9qnaelt7oo";
        public const string clientSecret = "jtzezlc6iuc18vh9dktywdgdgtu44b";

        private const string FileName = "TwitchLogger.txt";

        private static SemaphoreSlim fileLock = new SemaphoreSlim(1);

        public static readonly List<OAuthClientScopeEnum> scopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_commercial,
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,
            OAuthClientScopeEnum.channel_subscriptions,

            OAuthClientScopeEnum.user_read,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        private static TwitchConnection connection;

        private static ChatClient chat;
        private static PubSubClient pubSub;

        public static void Main(string[] args)
        {
            Logger.SetLogLevel(LogLevel.Debug);
            Logger.LogOccurred += Logger_LogOccurred;

            Task.Run(async () =>
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(File.Open(FileName, FileMode.Create)))
                    {
                        await writer.WriteLineAsync();
                        await writer.FlushAsync();
                    }

                    Logger.Log("Connecting to Twitch...");

                    connection = TwitchConnection.ConnectViaLocalhostOAuthBrowser(clientID, clientSecret, scopes).Result;
                    if (connection != null)
                    {
                        Logger.Log("Twitch connection successful!");

                        UserModel user = await connection.NewAPI.Users.GetCurrentUser();
                        if (user != null)
                        {
                            Logger.Log("Logged in as: " + user.display_name);

                            Logger.Log("Connecting to Chat...");

                            chat = new ChatClient(connection);

                            chat.OnDisconnectOccurred += Chat_OnDisconnectOccurred;
                            chat.OnSentOccurred += Chat_OnSentOccurred;
                            chat.OnPacketReceived += Chat_OnPacketReceived;

                            chat.OnPingReceived += Chat_OnPingReceived;
                            chat.OnUserJoinReceived += Chat_OnUserJoinReceived;
                            chat.OnUserLeaveReceived += Chat_OnUserLeaveReceived;
                            chat.OnMessageReceived += Chat_OnMessageReceived;

                            await chat.Connect();

                            await Task.Delay(1000);

                            await chat.AddCommandsCapability();
                            await chat.AddTagsCapability();
                            await chat.AddMembershipCapability();

                            await Task.Delay(1000);

                            UserModel broadcaster = await connection.NewAPI.Users.GetCurrentUser();

                            await chat.Join(broadcaster);

                            await Task.Delay(2000);

                            Logger.Log("Connecting to PubSub...");

                            pubSub = new PubSubClient(connection);

                            pubSub.OnDisconnectOccurred += PubSub_OnDisconnectOccurred;
                            pubSub.OnSentOccurred += PubSub_OnSentOccurred;
                            pubSub.OnReconnectReceived += PubSub_OnReconnectReceived;
                            pubSub.OnResponseReceived += PubSub_OnResponseReceived;
                            pubSub.OnMessageReceived += PubSub_OnMessageReceived;
                            pubSub.OnWhisperReceived += PubSub_OnWhisperReceived;
                            pubSub.OnPongReceived += PubSub_OnPongReceived;

                            await pubSub.Connect();

                            await Task.Delay(1000);

                            List<PubSubListenTopicModel> topics = new List<PubSubListenTopicModel>();
                            foreach (PubSubTopicsEnum topic in EnumHelper.GetEnumList<PubSubTopicsEnum>())
                            {
                                topics.Add(new PubSubListenTopicModel(topic, user.id));
                            }

                            await pubSub.Listen(topics);

                            await Task.Delay(1000);

                            await pubSub.Ping();

                            while (true)
                            {
                                System.Console.ReadLine();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }).Wait();
        }

        private static void Chat_OnUserJoinReceived(object sender, ChatUserJoinPacketModel packet)
        {
            Logger.Log(string.Format("User Joined: {0}", packet.UserLogin));
        }

        private static void Chat_OnUserLeaveReceived(object sender, ChatUserLeavePacketModel packet)
        {
            Logger.Log(string.Format("User Left: {0}", packet.UserLogin));
        }

        private static void Chat_OnMessageReceived(object sender, ChatMessagePacketModel packet)
        {
            Logger.Log(string.Format("{0}: {1}", packet.UserDisplayName, packet.Message));
        }

        private static async void Chat_OnPingReceived(object sender, EventArgs e)
        {
            await chat.Pong();
        }

        private static void Chat_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            Logger.Log("DISCONNECTED");
        }

        private static void Chat_OnSentOccurred(object sender, string packet)
        {
            Logger.Log("CHAT SEND: " + packet);
        }

        private static void Chat_OnPacketReceived(object sender, ChatRawPacketModel packet)
        {
            if (!packet.Command.Equals("PING") && !packet.Command.Equals(ChatMessagePacketModel.CommandID) && !packet.Command.Equals(ChatUserJoinPacketModel.CommandID)
                 && !packet.Command.Equals(ChatUserLeavePacketModel.CommandID))
            {
                Logger.Log("CHAT PACKET: " + packet.Command);

                Logger.Log(JSONSerializerHelper.SerializeToString(packet));
            }
        }

        private static void PubSub_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            Logger.Log("DISCONNECTED");
        }

        private static void PubSub_OnSentOccurred(object sender, string packet)
        {
            Logger.Log("PUB SUB SEND: " + packet);
        }

        private static void PubSub_OnReconnectReceived(object sender, System.EventArgs e)
        {
            Logger.Log("RECONNECT");
        }

        private static void PubSub_OnResponseReceived(object sender, PubSubResponsePacketModel packet)
        {
            Logger.Log("PUB SUB RESPONSE: " + packet.error);
        }

        private static void PubSub_OnMessageReceived(object sender, PubSubMessagePacketModel packet)
        {
            Logger.Log(string.Format("PUB SUB MESSAGE: {0} {1} ", packet.type, packet.message));

            Logger.Log(JSONSerializerHelper.SerializeToString(packet));
        }

        private static void PubSub_OnWhisperReceived(object sender, PubSubWhisperEventModel whisper)
        {
            Logger.Log("WHISPER: " + whisper.body);
        }

        private static void PubSub_OnPongReceived(object sender, System.EventArgs e)
        {
            Logger.Log("PONG");
            Task.Run(async () =>
            {
                await Task.Delay(1000 * 60 * 3);
                await pubSub.Ping();
            });
        }

        private static async void Logger_LogOccurred(object sender, Log log)
        {
            await fileLock.WaitAsync();
            try
            {
                System.Console.WriteLine(log.Message);
                System.Console.WriteLine();

                using (StreamWriter writer = new StreamWriter(File.Open(FileName, FileMode.Append)))
                {
                    await writer.WriteLineAsync(log.Message);
                    await writer.WriteLineAsync();
                    await writer.FlushAsync();
                }
            }
            catch (Exception) { }
            finally
            {
                fileLock.Release();
            }
        }
    }
}
