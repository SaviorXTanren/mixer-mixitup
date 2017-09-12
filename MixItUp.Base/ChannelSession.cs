using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using MixItUp.Base.Models;
using MixItUp.Base.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static MixerConnection MixerConnection { get; private set; }
        public static MixerConnection BotConnection { get; private set; }

        public static PrivatePopulatedUserModel User { get; private set; }
        public static PrivatePopulatedUserModel BotUser { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static ChannelSettings Settings { get; private set; }

        public static ChatClient ChatClient { get; private set; }
        public static ChatClient BotChatClient { get; private set; }
        public static InteractiveClient InteractiveClient { get; private set; }
        public static ConstellationClient ConstellationClient { get; private set; }
        public static OverlayWebServer OverlayServer { get; private set; }

        public static LockedDictionary<uint, ChatUserViewModel> ChatUsers { get; private set; }
        public static LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public static GiveawayItemModel Giveaway { get; set; }

        public static async Task<bool> Initialize(string clientID, IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            ChannelSession.MixerConnection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(clientID, scopes);
            if (ChannelSession.MixerConnection != null)
            {
                return await ChannelSession.InitializeInternal(channelName);
            }
            return false;
        }

        public static async Task<bool> Initialize(ChannelSettings settings)
        {
            bool result = false;

            ChannelSession.Settings = settings;
            ChannelSession.Settings.Initialize();

            ChannelSession.MixerConnection = MixerConnection.ConnectViaOAuthToken(settings.OAuthToken);
            if (settings.BotOAuthToken != null)
            {
                ChannelSession.BotConnection = MixerConnection.ConnectViaOAuthToken(settings.BotOAuthToken);
            }

            if (ChannelSession.MixerConnection != null)
            {
                await ChannelSession.MixerConnection.RefreshOAuthToken();
                result = await ChannelSession.InitializeInternal();
                if (result && ChannelSession.BotConnection != null)
                {
                    await ChannelSession.BotConnection.RefreshOAuthToken();
                    result = await ChannelSession.InitializeBotInternal();
                }
            }
            return result;
        }

        public static async Task<bool> InitializeBot(string clientID, IEnumerable<OAuthClientScopeEnum> scopes, Action<OAuthShortCodeModel> callback)
        {
            ChannelSession.BotConnection = await MixerConnection.ConnectViaShortCode(clientID, scopes, callback);
            if (ChannelSession.BotConnection != null)
            {
                return (await ChannelSession.InitializeBotInternal() && await ChannelSession.InitializeBotChatClient());
            }
            return false;
        }

        public static async Task<bool> InitializeChatClient()
        {
            ChannelSession.CheckMixerConnection();

            if (ChannelSession.ChatClient != null)
            {
                await ChannelSession.ChatClient.Disconnect();
            }

            ChannelSession.ChatClient = ChannelSession.BotChatClient = await ChatClient.CreateFromChannel(ChannelSession.MixerConnection, ChannelSession.Channel);
            if (await ChannelSession.ChatClient.Connect() && await ChannelSession.ChatClient.Authenticate())
            {
                if (ChannelSession.BotConnection != null)
                {
                    return await ChannelSession.InitializeBotChatClient();
                }
                return true;
            }
            ChannelSession.ChatClient = ChannelSession.BotChatClient = null;
            return false;
        }

        public static async Task<bool> InitializeBotChatClient()
        {
            if (ChannelSession.BotChatClient != null && ChannelSession.BotChatClient != ChannelSession.ChatClient)
            {
                await ChannelSession.BotChatClient.Disconnect();
            }

            ChannelSession.BotChatClient = await ChatClient.CreateFromChannel(ChannelSession.BotConnection, ChannelSession.Channel);
            if (await ChannelSession.BotChatClient.Connect() && await ChannelSession.BotChatClient.Authenticate())
            {
                return true;
            }
            ChannelSession.BotChatClient = ChannelSession.ChatClient;
            return false;
        }

        public static async Task<bool> InitializeConstellationClient()
        {
            ChannelSession.CheckMixerConnection();

            ChannelSession.ConstellationClient = await ConstellationClient.Create(ChannelSession.MixerConnection);
            if (await ChannelSession.ConstellationClient.Connect())
            {
                return true;
            }

            ChannelSession.ConstellationClient = null;
            return false;
        }

        public static bool InitializeOverlayServer()
        {
            if (ChannelSession.OverlayServer == null)
            {
                ChannelSession.OverlayServer = new OverlayWebServer("http://localhost:8001/");
                ChannelSession.OverlayServer.Start();
            }
            return true;
        }

        public static async Task<bool> ConnectInteractiveClient(ChannelModel channel, InteractiveGameListingModel game)
        {
            ChannelSession.CheckMixerConnection();

            ChannelSession.InteractiveClient = await InteractiveClient.CreateFromChannel(ChannelSession.MixerConnection, channel, game);
            if (await ChannelSession.InteractiveClient.Connect() && await ChannelSession.InteractiveClient.Ready())
            {
                return true;
            }

            ChannelSession.InteractiveClient = null;
            return false;
        }

        public static void DisconnectBot() { ChannelSession.BotConnection = null; }

        public static async Task<bool> DisconnectInteractiveClient()
        {
            ChannelSession.CheckMixerConnection();

            if (ChannelSession.InteractiveClient == null)
            {
                throw new InvalidOperationException("Interactive client is not connected");
            }
            await ChannelSession.InteractiveClient.Disconnect();

            ChannelSession.InteractiveClient = null;
            return true;
        }

        public static void Close()
        {
            if (ChannelSession.OverlayServer != null)
            {
                ChannelSession.OverlayServer.End();
            }

            if (ChannelSession.ChatClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ChannelSession.ChatClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (ChannelSession.InteractiveClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ChannelSession.InteractiveClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (ChannelSession.ConstellationClient != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ChannelSession.ConstellationClient.Disconnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private static async Task<bool> InitializeInternal(string channelName = null)
        {
            PrivatePopulatedUserModel user = await ChannelSession.MixerConnection.Users.GetCurrentUser();
            if (user != null)
            {
                ExpandedChannelModel channel = await ChannelSession.MixerConnection.Channels.GetChannel((channelName == null) ? user.username : channelName);
                if (channel != null)
                {
                    ChannelSession.User = user;
                    ChannelSession.Channel = channel;

                    ChannelSession.ChatUsers = new LockedDictionary<uint, ChatUserViewModel>();
                    ChannelSession.InteractiveUsers = new LockedDictionary<string, InteractiveParticipantModel>();

                    ChannelSession.Giveaway = new GiveawayItemModel();
                    
                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = new ChannelSettings(channel);
                    }
                    await ChannelSession.SaveSettings();

                    return true;
                }
            }
            return false;
        }

        private static async Task<bool> InitializeBotInternal()
        {
            PrivatePopulatedUserModel user = await ChannelSession.BotConnection.Users.GetCurrentUser();
            if (user != null)
            {
                ChannelSession.BotUser = user;

                await ChannelSession.SaveSettings();

                return true;
            }
            return false;
        }

        private static void CheckMixerConnection()
        {
            if (ChannelSession.MixerConnection == null)
            {
                throw new InvalidOperationException("Mixer client has not been initialized");
            }
        }

        public static async Task SaveSettings() { await ChannelSession.Settings.Save(); }

        public static async Task RefreshUser()
        {
            if (ChannelSession.User != null)
            {
                ChannelSession.User = await ChannelSession.MixerConnection.Users.GetCurrentUser();
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.Channel != null)
            {
                ChannelSession.Channel = await ChannelSession.MixerConnection.Channels.GetChannel(ChannelSession.Channel.user.username);
            }
        }

        public static UserViewModel GetCurrentUser() { return new UserViewModel(User.id, User.username); }
    }
}
