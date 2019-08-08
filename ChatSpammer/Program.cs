using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatSpammer
{
    public class Program
    {
        private static ChatClient chatClient;

        public static void Main(string[] args)
        {
            List<OAuthClientScopeEnum> scopes = new List<OAuthClientScopeEnum>()
            {
                OAuthClientScopeEnum.chat__bypass_links,
                OAuthClientScopeEnum.chat__bypass_slowchat,
                OAuthClientScopeEnum.chat__change_ban,
                OAuthClientScopeEnum.chat__change_role,
                OAuthClientScopeEnum.chat__chat,
                OAuthClientScopeEnum.chat__connect,
                OAuthClientScopeEnum.chat__clear_messages,
                OAuthClientScopeEnum.chat__edit_options,
                OAuthClientScopeEnum.chat__giveaway_start,
                OAuthClientScopeEnum.chat__poll_start,
                OAuthClientScopeEnum.chat__poll_vote,
                OAuthClientScopeEnum.chat__purge,
                OAuthClientScopeEnum.chat__remove_message,
                OAuthClientScopeEnum.chat__timeout,
                OAuthClientScopeEnum.chat__view_deleted,
                OAuthClientScopeEnum.chat__whisper,

                OAuthClientScopeEnum.channel__details__self,
                OAuthClientScopeEnum.channel__update__self,

                OAuthClientScopeEnum.user__details__self,
                OAuthClientScopeEnum.user__log__self,
                OAuthClientScopeEnum.user__notification__self,
                OAuthClientScopeEnum.user__update__self,
            };

            System.Console.WriteLine("Connecting to Mixer...");

            MixerConnection connection = MixerConnection.ConnectViaLocalhostOAuthBrowser("a95a8520f369fdd46d08aba183953c5c8a4c3822affec476", scopes).Result;

            if (connection != null)
            {
                System.Console.WriteLine("Mixer connection successful!");

                UserModel user = connection.Users.GetCurrentUser().Result;
                ExpandedChannelModel channel = connection.Channels.GetChannel(user.username).Result;
                System.Console.WriteLine(string.Format("Logged in as: {0}", user.username));

                System.Console.WriteLine();
                System.Console.WriteLine("Connecting to channel chat...");

                chatClient = ChatClient.CreateFromChannel(connection, channel).Result;

                chatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;
                chatClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
                chatClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                chatClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;

                if (chatClient.Connect().Result && chatClient.Authenticate().Result)
                {
                    System.Console.WriteLine("Chat connection successful!");

                    IEnumerable<ChatUserModel> users = connection.Chats.GetUsers(chatClient.Channel).Result;
                    System.Console.WriteLine(string.Format("There are {0} users currently in chat", users.Count()));
                    System.Console.WriteLine();

                    long messageNumber = 0;
                    while (true)
                    {
                        Task.Delay(50).Wait();

                        chatClient.SendMessage(string.Format("Message #{0}", messageNumber));

                        messageNumber++;
                    }
                }
            }
        }

        private static async void ChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            System.Console.WriteLine("Disconnection Occurred, attempting reconnection...");

            do
            {
                await Task.Delay(2500);
            }
            while (!await Program.chatClient.Connect() && !await Program.chatClient.Authenticate());

            System.Console.WriteLine("Reconnection successful");
        }

        private static void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            string message = "";
            foreach (ChatMessageDataModel m in e.message.message)
            {
                message += m.text;
            }
            System.Console.WriteLine(string.Format("{0}: {1}", e.user_name, message));
        }

        private static void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            System.Console.WriteLine(string.Format("{0} has joined chat", e.username));
        }

        private static void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            System.Console.WriteLine(string.Format("{0} has left chat", e.username));
        }
    }
}
