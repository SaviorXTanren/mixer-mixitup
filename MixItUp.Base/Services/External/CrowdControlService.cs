using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class CrowdControlGame
    {
        public string gameID { get; set; }
        public JToken name { get; set; }
        public List<string> packets { get; set; } = new List<string>();

        public string Name { get { return (this.name is JObject) ? this.name["public"].ToString() : this.name.ToString(); } }
    }

    public class CrowdControlGamePack
    {
        public CrowdControlGame game { get; set; }
        public string gamePackID { get; set; }
        public CrowdControlGamePackMetadata meta { get; set; }
        public Dictionary<string, Dictionary<string, CrowdControlGamePackEffect>> effects { get; set; } = new Dictionary<string, Dictionary<string, CrowdControlGamePackEffect>>();

        public string Name { get { return this.meta?.name; } }

        public IEnumerable<CrowdControlGamePackEffect> GameEffects
        {
            get
            {
                if (this.effects.ContainsKey("game"))
                {
                    return this.effects["game"].Values;
                }
                return new List<CrowdControlGamePackEffect>();
            }
        }
    }

    public class CrowdControlGamePackMetadata
    {
        public string visibility { get; set; }
        public string name { get; set; }
    }

    public class CrowdControlGamePackEffect
    {
        public string id { get; set; }
        public JToken name { get; set; }
        public string note { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public JObject quantity { get; set; } = new JObject();
        public JObject duration { get; set; } = new JObject();
        public List<string> category { get; set; } = new List<string>();
        public int moral { get; set; }

        public string Name { get { return (this.name is JObject) ? this.name["public"].ToString() : this.name.ToString(); } }

        public string FullName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.note))
                {
                    return $"{this.Name} - {this.note}";
                }
                return this.Name;
            }
        }
    }

    public class CrowdControlWebSocketPacket
    {
        public string domain { get; set; }
        public string type { get; set; }
        public JObject payload { get; set; }
    }

    public class CrowdControlWebSocketSubscriptionResultModel
    {
        public List<string> success { get; set; } = new List<string>();
        public List<string> failure { get; set; } = new List<string>();
    }

    public class CrowdControlWebSocketEffectSuccessModel
    {
        public string requestID { get; set; }
        public int quantity { get; set; }
        public CrowdControlWebSocketEffectDetailsModel effect { get; set; }
        public CrowdControlWebSocketGameModel game { get; set; }
        public CrowdControlWebSocketGamePackModel gamePack { get; set; }
        public CrowdControlWebSocketUserModel target { get; set; }
        public CrowdControlWebSocketUserModel requester { get; set; }
    }

    public class CrowdControlWebSocketEffectDetailsModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public string effectID { get; set; }
        public string type { get; set; }
        public string image { get; set; }
    }

    public class CrowdControlWebSocketGameModel
    {
        public string gameID { get; set; }
        public string name { get; set; }
    }

    public class CrowdControlWebSocketGamePackModel
    {
        public string name { get; set; }
        public string platform { get; set; }
        public string gamePackID { get; set; }
    }

    public class CrowdControlWebSocketUserModel
    {
        public string ccUID { get; set; }
        public string image { get; set; }
        public string originID { get; set; }
        public string profile { get; set; }
        public string name { get; set; }
    }

    public class CrowdControlWebSocket : ClientWebSocketBase
    {
        public string SubscriptionTopic { get; private set; }
        public bool Subscribed { get; private set; }
        public List<string> ConnectionFailures { get; private set; } = new List<string>();

        public async Task SendPublicConnect(string id)
        {
            this.SubscriptionTopic = "pub/" + id;
            this.Subscribed = false;
            this.ConnectionFailures.Clear();

            JObject jobj = new JObject();
            jobj["action"] = "subscribe";
            jobj["data"] = "{\"topics\":[\"" + this.SubscriptionTopic + "\"]}";
            await this.Send(JSONSerializerHelper.SerializeToString(jobj, includeObjectType: false));
        }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                CrowdControlWebSocketPacket packet = JSONSerializerHelper.DeserializeFromString<CrowdControlWebSocketPacket>(packetJSON);
                if (packet != null)
                {
                    if (string.Equals(packet.type, "subscription-result", StringComparison.OrdinalIgnoreCase))
                    {
                        CrowdControlWebSocketSubscriptionResultModel subscription = packet.payload.ToObject<CrowdControlWebSocketSubscriptionResultModel>();
                        if (subscription != null)
                        {
                            if (subscription.failure != null && subscription.failure.Count > 0)
                            {
                                this.ConnectionFailures.AddRange(subscription.failure);
                            }
                            else if (subscription.success != null && subscription.success.Contains(this.SubscriptionTopic))
                            {
                                this.Subscribed = true;
                            }
                        }
                    }
                    else if (string.Equals(packet.type, "effect-success", StringComparison.OrdinalIgnoreCase))
                    {
                        CrowdControlWebSocketEffectSuccessModel effect = packet.payload.ToObject<CrowdControlWebSocketEffectSuccessModel>();
                        if (effect != null)
                        {
                            UserV2ViewModel requester = null;
                            if (string.Equals(effect.requester.profile, StreamingPlatformTypeEnum.Twitch.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                requester = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: effect.requester.originID, platformUsername: effect.requester.name);
                            }
                            else if (string.Equals(effect.requester.profile, StreamingPlatformTypeEnum.YouTube.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                requester = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: effect.requester.originID, platformUsername: effect.requester.name);
                            }

                            if (requester == null)
                            {
                                requester = UserV2ViewModel.CreateUnassociated(effect.requester.name);
                            }

                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                            specialIdentifiers["crowdcontroleffectid"] = effect.effect.effectID;
                            specialIdentifiers["crowdcontroleffectname"] = effect.effect.name;
                            specialIdentifiers["crowdcontroleffectdescription"] = effect.effect.description;
                            specialIdentifiers["crowdcontroleffectimage"] = effect.effect.image;
                            specialIdentifiers["crowdcontrolgamename"] = effect.game.name;

                            CommandParametersModel parameters = new CommandParametersModel(requester, specialIdentifiers);

                            CrowdControlEffectCommandModel command = ServiceManager.Get<CommandService>().CrowdControlEffectCommands.FirstOrDefault(c =>
                                string.Equals(c.GameID, effect.game.gameID, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(c.PackID, effect.gamePack.gamePackID, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(c.EffectID, effect.effect.effectID, StringComparison.OrdinalIgnoreCase));

                            if (effect.quantity == 0)
                            {
                                effect.quantity = 1;
                            }

                            for (int i = 0; i < effect.quantity; i++)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.CrowdControlEffectRedeemed, parameters);
                                if (command != null)
                                {
                                    await ServiceManager.Get<CommandService>().Queue(command, parameters);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log("CrowdControl Service - Failed Packet Processing: " + packetJSON);
            }
        }
    }

    public class CrowdControlService : IExternalService
    {
        private const string BaseURL = "https://openapi.crowdcontrol.live";
        private const string WebsocketURL = "wss://pubsub.crowdcontrol.live";

        public string Name { get { return Resources.CrowdControl; } }

        public bool IsConnected { get; private set; }

        private CrowdControlWebSocket socket;

        private string id;

        private IEnumerable<CrowdControlGame> gamesCache = new List<CrowdControlGame>();

        private Dictionary<string, IEnumerable<CrowdControlGamePack>> gamePacks = new Dictionary<string, IEnumerable<CrowdControlGamePack>>();

        public CrowdControlService()
        {
            this.socket = new CrowdControlWebSocket();
        }

        public async Task<Result> Connect()
        {
            try
            {
                this.id = null;
                this.IsConnected = false;
                await this.Disconnect();

                if (ServiceManager.Get<TwitchSession>().IsConnected)
                {
                    this.id = await this.GetCrowdControlID(StreamingPlatformTypeEnum.Twitch.ToString(), ServiceManager.Get<TwitchSession>().StreamerID);
                }
                else if (ServiceManager.Get<YouTubeSession>().IsConnected)
                {
                    this.id = await this.GetCrowdControlID(StreamingPlatformTypeEnum.YouTube.ToString(), ServiceManager.Get<YouTubeSession>().StreamerID);
                }

                if (string.IsNullOrEmpty(this.id))
                {
                    return new Result(Resources.CrowdControlTwitchAccountMustBeConnected);
                }

                if (this.gamesCache.Count() == 0)
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient(BaseURL))
                    {
                        this.gamesCache = await client.GetAsync<IEnumerable<CrowdControlGame>>("games");
                        this.gamesCache = this.gamesCache.OrderBy(g => g.Name);
                    }
                }

                if (ChannelSession.IsDebug())
                {
                    this.socket.OnSentOccurred += Socket_OnSentOccurred;
                    this.socket.OnTextReceivedOccurred += Socket_OnTextReceivedOccurred;
                }
                this.socket.OnDisconnectOccurred += Socket_OnDisconnectOccurred;
                await this.socket.Connect(CrowdControlService.WebsocketURL);

                await this.socket.SendPublicConnect(this.id);

                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(1000);
                    if (this.socket.Subscribed)
                    {
                        this.IsConnected = true;

                        ServiceManager.Get<ITelemetryService>().TrackService("CrowdControl");

                        return new Result();
                    }
                    else if (this.socket.ConnectionFailures.Count > 0)
                    {
                        await this.Disconnect();
                        return new Result(string.Join(" - ", this.socket.ConnectionFailures));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.CrowdControlFailedToConnectToService);
        }

        public async Task Disconnect()
        {
            this.IsConnected = false;
            if (ChannelSession.IsDebug())
            {
                this.socket.OnSentOccurred -= Socket_OnSentOccurred;
                this.socket.OnTextReceivedOccurred -= Socket_OnTextReceivedOccurred;
            }
            this.socket.OnDisconnectOccurred -= Socket_OnDisconnectOccurred;
            await this.socket.Disconnect();
        }

        public IEnumerable<CrowdControlGame> GetGames() { return this.gamesCache; }

        public async Task<IEnumerable<CrowdControlGamePack>> GetGamePacks(CrowdControlGame game)
        {
            if (this.gamePacks.ContainsKey(game.gameID))
            {
                return this.gamePacks[game.gameID];
            }

            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(BaseURL))
                {
                    IEnumerable<CrowdControlGamePack> gamePacks = await client.GetAsync<IEnumerable<CrowdControlGamePack>>($"games/{game.gameID}/packs");
                    if (gamePacks != null)
                    {
                        foreach (var gamePack in gamePacks)
                        {
                            foreach (var effectKVP in gamePack.effects)
                            {
                                foreach (var effect in effectKVP.Value)
                                {
                                    effect.Value.id = effect.Key;
                                }
                            }
                        }

                        this.gamePacks[game.gameID] = gamePacks;
                        return gamePacks;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<CrowdControlGamePack>();
        }

        private async Task<string> GetCrowdControlID(string platform, string platformID)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient(BaseURL))
            {
                JObject jobj = await client.GetJObjectAsync($"user/{platform.ToLower()}/{platformID}/id");
                if (jobj != null && jobj.TryGetValue("ccUID", out JToken id))
                {
                    return id.ToString();
                }
            }
            return null;
        }

        private async void Socket_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            await this.Disconnect();
        }

        private void Socket_OnSentOccurred(object sender, string e)
        {
            Logger.Log("CrowdControl Service - Packet Sent: " + e);
        }

        private void Socket_OnTextReceivedOccurred(object sender, string e)
        {
            Logger.Log("CrowdControl Service - Packet Received: " + e);
        }
    }
}
