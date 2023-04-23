using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class CrowdControlEffectUpdateMessage
    {
        public string channelID { get; set; }
        public string id { get; set; }
        public int state { get; set; }
        public object effect { get; set; }
        public int completedAt { get; set; }
        public string type { get; set; }
    }

    public class CrowdControlCoinExchangeMessage
    {
        public string channelID { get; set; }
        public bool playSFX { get; set; }
        public bool anonymous { get; set; }
        public int timestamp { get; set; }
        public string type { get; set; }
        public int amount { get; set; }
        public CrowdControlViewer viewer { get; set; }
    }

    public class CrowdControlViewer
    {
        public string name { get; set; }
        public string twitchID { get; set; }
        public string twitchAvatarURL { get; set; }
    }

    public class CrowdControlService : IExternalService
    {
        private const string ConnectionURL = "wss://overlay-socket.crowdcontrol.live";

        public string Name { get { return Resources.CrowdControl; } }

        public bool IsConnected { get; private set; }

        private ISocketIOConnection socket;

        public CrowdControlService(ISocketIOConnection socket)
        {
            this.socket = socket;
        }

        public async Task<Result> Connect()
        {
            if (!ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                return new Result(Resources.CrowdControlTwitchAccountMustBeConnected);
            }

            try
            {
                this.socket.Listen("effect-initial", (data) =>
                {
                    try
                    {
                        if (data != null)
                        {
                            CrowdControlEffectUpdateMessage effectUpdate = JSONSerializerHelper.DeserializeFromString<CrowdControlEffectUpdateMessage>(data.ToString());
                            if (effectUpdate != null)
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                });

                this.socket.Listen("coin-message", (data) =>
                {
                    try
                    {
                        if (data != null)
                        {
                            CrowdControlCoinExchangeMessage coinExchange = JSONSerializerHelper.DeserializeFromString<CrowdControlCoinExchangeMessage>(data.ToString());
                            if (coinExchange != null)
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                });

                await this.socket.Connect(CrowdControlService.ConnectionURL);

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.CrowdControlFailedToConnectToService);
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
