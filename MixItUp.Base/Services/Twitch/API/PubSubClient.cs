using MixItUp.Base.Model.Twitch.Clients.PubSub;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// Web Socket client for interacting with PubSub service.
    /// </summary>
    public class PubSubClient : ClientWebSocketBase
    {
        /// <summary>
        /// The default PubSub connection url.
        /// </summary>
        public const string PUBSUB_CONNECTION_URL = "wss://pubsub-edge.twitch.tv";

        /// <summary>
        /// Invoked when a pong packet is received.
        /// </summary>
        public event EventHandler OnPongReceived = delegate { };

        /// <summary>
        /// Invoked when a reconnect packet is received.
        /// </summary>
        public event EventHandler OnReconnectReceived = delegate { };

        /// <summary>
        /// Invoked when a response packet is received.
        /// </summary>
        public event EventHandler<PubSubResponsePacketModel> OnResponseReceived = delegate { };
        /// <summary>
        /// Invoked when a message packet is received.
        /// </summary>
        public event EventHandler<PubSubMessagePacketModel> OnMessageReceived = delegate { };

        /// <summary>
        /// Invoked when a whisper event is received.
        /// </summary>
        public event EventHandler<PubSubWhisperEventModel> OnWhisperReceived = delegate { };
        /// <summary>
        /// Invoked whena a bits v1 event is received.
        /// </summary>
        public event EventHandler<PubSubBitsEventV1Model> OnBitsV1Received = delegate { };
        /// <summary>
        /// Invoked when a bits v2 event is received.
        /// </summary>
        public event EventHandler<PubSubBitsEventV2Model> OnBitsV2Received = delegate { };
        /// <summary>
        /// Invoked when a bits badge event is received.
        /// </summary>
        public event EventHandler<PubSubBitBadgeEventModel> OnBitsBadgeReceived = delegate { };
        /// <summary>
        /// Invoked when a subscription/resubscription event is received.
        /// </summary>
        public event EventHandler<PubSubSubscriptionsEventModel> OnSubscribedReceived = delegate { };
        /// <summary>
        /// Invoked when a subscription gifted event is received.
        /// </summary>
        public event EventHandler<PubSubSubscriptionsGiftEventModel> OnSubscriptionsGiftedReceived = delegate { };
        /// <summary>
        /// Invoked when a channel points redeemed event is received.
        /// </summary>
        public event EventHandler<PubSubChannelPointsRedemptionEventModel> OnChannelPointsRedeemed = delegate { };

        private TwitchConnection connection;

        /// <summary>
        /// Creates a new instance of the PubSubClient class.
        /// </summary>
        /// <param name="connection">The current connection</param>
        public PubSubClient(TwitchConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Connects to the default PubSub connection.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task Connect()
        {
            await base.Connect(PubSubClient.PUBSUB_CONNECTION_URL);
        }

        /// <summary>
        /// Sends a listen packet for a specified set of topics.
        /// </summary>
        /// <param name="topics">The topics to listen for</param>
        /// <returns>An awaitable Task</returns>
        public async Task Listen(IEnumerable<PubSubListenTopicModel> topics)
        {
            OAuthTokenModel oauthToken = await this.connection.GetOAuthToken();
            await this.Send(new PubSubPacketModel("LISTEN", new { topics = topics.Select(t => t.ToString()).ToList(), auth_token = oauthToken.accessToken }));
        }

        /// <summary>
        /// Sends a ping packet.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task Ping()
        {
            await this.Send(JSONSerializerHelper.SerializeToString(new { type = "PING" }));
        }

        /// <summary>
        /// Processes the received text packet.
        /// </summary>
        /// <param name="packetText">The receive text packet</param>
        /// <returns>An awaitable task</returns>
        protected override Task ProcessReceivedPacket(string packetText)
        {
            Logger.Log(LogLevel.Debug, "Twitch PubSub Packet Received: " + packetText);

            PubSubPacketModel packet = JSONSerializerHelper.DeserializeFromString<PubSubPacketModel>(packetText);
            if (packet != null)
            {
                switch (packet.type)
                {
                    case "RECONNECT":
                        this.OnReconnectReceived?.Invoke(this, new EventArgs());
                        break;
                    case "RESPONSE":
                        this.OnResponseReceived?.Invoke(this, JSONSerializerHelper.DeserializeFromString<PubSubResponsePacketModel>(packetText));
                        break;
                    case "MESSAGE":
                        PubSubMessagePacketModel messagePacket = JSONSerializerHelper.DeserializeFromString<PubSubMessagePacketModel>(packetText);
                        this.OnMessageReceived?.Invoke(this, messagePacket);
                        try
                        {
                            PubSubMessagePacketDataModel messageData = messagePacket.messageData;
                            if (messageData != null)
                            {
                                if (messagePacket.topicType == PubSubTopicsEnum.UserWhispers)
                                {
                                    this.OnWhisperReceived?.Invoke(this, messageData.data_object.ToObject<PubSubWhisperEventModel>());
                                }
                                else if (messagePacket.topicType == PubSubTopicsEnum.ChannelBitsEventsV1)
                                {
                                    this.OnBitsV1Received?.Invoke(this, messageData.data_object.ToObject<PubSubBitsEventV1Model>());
                                }
                                else if (messagePacket.topicType == PubSubTopicsEnum.ChannelBitsEventsV2)
                                {
                                    this.OnBitsV2Received?.Invoke(this, messageData.data_object.ToObject<PubSubBitsEventV2Model>());
                                }
                                else if (messagePacket.topicType == PubSubTopicsEnum.ChannelBitsBadgeUnlocks)
                                {
                                    this.OnBitsBadgeReceived?.Invoke(this, messageData.data_object.ToObject<PubSubBitBadgeEventModel>());
                                }
                                else if (messagePacket.topicType == PubSubTopicsEnum.ChannelSubscriptionsV1)
                                {
                                    PubSubSubscriptionsEventModel subscription = JSONSerializerHelper.DeserializeFromString<PubSubSubscriptionsEventModel>(messagePacket.message);
                                    if (subscription.IsGiftedSubscription || subscription.IsAnonymousGiftedSubscription)
                                    {
                                        this.OnSubscriptionsGiftedReceived?.Invoke(this, JSONSerializerHelper.DeserializeFromString<PubSubSubscriptionsGiftEventModel>(messagePacket.message));
                                    }
                                    else
                                    {
                                        this.OnSubscribedReceived?.Invoke(this, subscription);
                                    }
                                }
                                else if (messagePacket.topicType == PubSubTopicsEnum.ChannelPointsRedeemed)
                                {
                                    this.OnChannelPointsRedeemed?.Invoke(this, messageData.data_object.ToObject<PubSubChannelPointsRedemptionEventModel>());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                        break;
                    case "PONG":
                        this.OnPongReceived?.Invoke(this, new EventArgs());
                        break;
                }
            }
            return Task.FromResult(0);
        }

        private async Task Send(PubSubPacketModel packet)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(packet));
        }
    }
}
