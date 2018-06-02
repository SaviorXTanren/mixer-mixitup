using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Desktop.Services
{
    public class StreamlabsWebSocketService : SocketIOService
    {
        [DataContract]
        private class StreamlabsWebSocketEvent
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("message")]
            public JArray Data { get; set; }

            [JsonProperty("for")]
            public string For { get; set; }
        }

        private StreamlabsService service;
        private string socketToken;

        public StreamlabsWebSocketService(StreamlabsService service, string socketToken)
            : base(string.Format("https://sockets.streamlabs.com?token={0}", socketToken))
        {
            this.service = service;
            this.socketToken = socketToken;
        }

        public override async Task Connect()
        {
            await base.Connect();

            this.SocketReceiveWrapper("disconnect", (errorData) =>
            {
                MixItUp.Base.Util.Logger.Log(errorData.ToString());
                this.service.WebSocketDisconnectedOccurred();
            });

            this.SocketEventReceiverWrapper<StreamlabsWebSocketEvent>("event", (eventData) =>
            {
                if (eventData.Type.Equals("donation"))
                {
                    Task.Run(async () =>
                    {
                        EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.StreamlabsDonation));
                        if (command != null)
                        {
                            foreach (JToken token in eventData.Data)
                            {
                                StreamlabsDonationEvent slDonation = token.ToObject<StreamlabsDonationEvent>();
                                UserDonationModel donation = slDonation.ToGenericDonation();

                                UserViewModel user = new UserViewModel(0, donation.UserName);
                                UserModel userModel = await ChannelSession.Connection.GetUser(donation.UserName);
                                if (userModel != null)
                                {
                                    user = new UserViewModel(userModel);
                                }

                                command.AddSpecialIdentifier("donationsource", EnumHelper.GetEnumName(donation.Source));
                                command.AddSpecialIdentifier("donationamount", donation.AmountText);
                                command.AddSpecialIdentifier("donationmessage", donation.Message);
                                await command.Perform(user);
                            }
                        }
                    });
                }
            });
        }

        public override async Task Disconnect()
        {
            await base.Disconnect();
            this.socketToken = null;
        }
    }

    [DataContract]
    public class StreamlabsService : OAuthServiceBase, IStreamlabsService
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write";

        public event EventHandler OnWebSocketConnectedOccurred;
        public event EventHandler OnWebSocketDisconnectedOccurred;

        private StreamlabsWebSocketService websocketService;

        public StreamlabsService() : base(StreamlabsService.BaseAddress) { }

        public StreamlabsService(OAuthTokenModel token) : base(StreamlabsService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    await this.InitializeInternal();

                    return true;
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamlabsService.AuthorizationUrl, StreamlabsService.ClientID));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                JObject payload = new JObject();
                payload["grant_type"] = "authorization_code";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("StreamlabsSecret");
                payload["code"] = authorizationCode;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", this.CreateContentFromObject(payload), autoRefreshToken: false);
                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

                    await this.InitializeInternal();

                    return true;
                }
            }

            return false;
        }

        public async Task Disconnect()
        {
            this.token = null;
            if (this.websocketService != null)
            {
                await this.websocketService.Disconnect();
            }
        }

        public void WebSocketConnectedOccurred()
        {
            if (this.OnWebSocketConnectedOccurred != null)
            {
                this.OnWebSocketConnectedOccurred(this, new EventArgs());
            }
        }

        public async void WebSocketDisconnectedOccurred()
        {
            if (this.OnWebSocketDisconnectedOccurred != null)
            {
                this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
            }
            await this.ReconnectWebSocket();
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("StreamlabsSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", this.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        private async Task InitializeInternal()
        {
            HttpResponseMessage result = await this.GetAsync("socket/token");
            string resultJson = await result.Content.ReadAsStringAsync();
            JObject jobj = JObject.Parse(resultJson);

            this.websocketService = new StreamlabsWebSocketService(this, jobj["socket_token"].ToString());
            await this.websocketService.Connect();
        }

        private async Task ReconnectWebSocket()
        {
            await this.websocketService.Disconnect();

            await Task.Delay(1000);

            await this.websocketService.Connect();
        }
    }
}
