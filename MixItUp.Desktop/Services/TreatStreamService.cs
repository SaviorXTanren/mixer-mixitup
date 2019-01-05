using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class TreatStreamWebSocketService : SocketIOService
    {
        public bool Connected { get; private set; }

        private TreatStreamService service;
        private string socketToken;

        public TreatStreamWebSocketService(TreatStreamService service, string socketToken)
            : base("https://nodeapi.treatstream.com/", "token=" + socketToken)
        {
            this.service = service;
            this.socketToken = socketToken;
        }

        public override async Task Connect()
        {
            await base.Connect();

            this.SocketReceiveWrapper("connect", (data) =>
            {
                this.Connected = true;
            });

            this.SocketReceiveWrapper("realTimeTreat", (data) =>
            {
                if (data != null)
                {
                    TreatStreamEvent tsEvent = SerializerHelper.DeserializeFromString<TreatStreamEvent>(data.ToString());
                    if (tsEvent != null)
                    {
                        this.service.DonationOccurred(tsEvent);
                        Task.Run(async () =>
                        {
                            UserDonationModel donation = tsEvent.ToGenericDonation();
                            await EventCommand.ProcessDonationEventCommand(donation, OtherEventTypeEnum.TreatStreamDonation);
                        });
                    }
                }
            });

            this.SocketReceiveWrapper("error", (errorData) =>
            {
                MixItUp.Base.Util.Logger.Log(errorData.ToString());
                this.service.WebSocketDisconnectedOccurred();
            });

            this.SocketReceiveWrapper("disconnect", (errorData) =>
            {
                MixItUp.Base.Util.Logger.Log(errorData.ToString());
                this.service.WebSocketDisconnectedOccurred();
            });

            for (int i = 0; i < 10 && !this.Connected; i++)
            {
                await Task.Delay(1000);
            }

            if (this.Connected)
            {
                this.service.WebSocketConnectedOccurred();
            }
        }

        public override async Task Disconnect()
        {
            this.Connected = false;

            if (this.socket != null)
            {
                this.SocketSendWrapper("disconnect", null);
            }

            await base.Disconnect();
        }
    }

    public class TreatStreamService : OAuthServiceBase, ITreatStreamService
    {
        private const string BaseAddress = "https://treatstream.com/api/";

        public const string ClientID = "xr7qxfpymxsdabivcit0sjv64qv2u5x34058pzvw";

        public const string ListeningURL = "http://localhost:8919";

        public const string AuthorizationURL = "https://treatstream.com/Oauth2/Authorize?response_type=code&client_id={0}&redirect_uri={1}&state=xyz&scope=";
        public const string OAuthTokenURL = "https://treatstream.com/Oauth2/Authorize/token";
        public const string RefreshTokenURL = "https://treatstream.com/Oauth2/RefreshToken";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<TreatStreamEvent> OnDonationOccurred = delegate { };

        public bool WebSocketConnectedAndAuthenticated { get; private set; }

        private string authorizationToken;

        private TreatStreamWebSocketService socket;

        public TreatStreamService() : base(TreatStreamService.BaseAddress) { }

        public TreatStreamService(OAuthTokenModel token) : base(TreatStreamService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    if (await this.InitializeInternal())
                    {
                        return true;
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }

            this.authorizationToken = await this.ConnectViaOAuthRedirect(string.Format(TreatStreamService.AuthorizationURL, TreatStreamService.ClientID, TreatStreamService.ListeningURL));
            if (!string.IsNullOrEmpty(this.authorizationToken))
            {
                try
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TreatStreamService.ClientID;
                    payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TreatStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TreatStreamService.ListeningURL;
                    payload["scope"] = "userinfo";

                    this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.OAuthTokenURL, this.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = this.authorizationToken;
                        token.AcquiredDateTime = DateTimeOffset.Now;
                        token.expiresIn = int.MaxValue;

                        return await this.InitializeInternal();
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }
            return false;
        }

        public async Task Disconnect()
        {
            this.token = null;
            if (this.socket != null)
            {
                await this.socket.Disconnect();
                this.socket = null;
            }
        }

        public async Task<string> GetSocketToken()
        {
            try
            {
                JObject payload = new JObject();
                payload["client_id"] = TreatStreamService.ClientID;
                payload["access_token"] = this.token.accessToken;

                JObject jobj = await this.PostAsync<JObject>("https://treatstream.com/Oauth2/Authorize/socketToken", this.CreateContentFromObject(payload));
                if (jobj != null && jobj.ContainsKey("socket_token"))
                {
                    return jobj["socket_token"].ToString();
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task GetTreats()
        {
            try
            {
                string result = await this.GetStringAsync(string.Format("getMonthTreats/{0}", this.token.accessToken));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["client_id"] = TipeeeStreamService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["grant_type"] = "refresh_token";

                this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.RefreshTokenURL, this.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected async Task<bool> InitializeInternal()
        {
            string socketToken = await this.GetSocketToken();
            if (!string.IsNullOrEmpty(socketToken))
            {
                this.socket = new TreatStreamWebSocketService(this, socketToken);
                await this.socket.Connect();
                return this.socket.Connected;
            }
            return false;
        }

        public void WebSocketConnectedOccurred()
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TreatStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }
    }
}
