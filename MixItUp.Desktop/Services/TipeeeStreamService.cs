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
    public class TipeeeStreamWebSocketService : SocketIOService
    {
        public bool Connected { get; private set; }

        private TipeeeStreamService service;
        private string username;
        private string apiKey;

        public TipeeeStreamWebSocketService(TipeeeStreamService service, string username, string apiKey)
            : base("https://sso.tipeeestream.com:4242")
        {
            this.service = service;
            this.username = username;
            this.apiKey = apiKey;
        }

        public override async Task Connect()
        {
            await base.Connect();

            this.SocketReceiveWrapper("connect", (data) =>
            {
                this.Connected = true;
            });

            this.SocketReceiveWrapper("new-event", (data) =>
            {
                if (data != null)
                {
                    TipeeeStreamResponse response = SerializerHelper.DeserializeFromString<TipeeeStreamResponse>(data.ToString());
                    if (response.Event.Equals("donation"))
                    {
                        this.service.DonationOccurred(response.Event);
                        Task.Run(async () =>
                        {
                            UserDonationModel donation = response.Event.ToGenericDonation();
                            GlobalEvents.DonationOccurred(donation);

                            UserViewModel user = new UserViewModel(0, donation.UserName);

                            UserModel userModel = await ChannelSession.Connection.GetUser(user.UserName);
                            if (userModel != null)
                            {
                                user = new UserViewModel(userModel);
                            }

                            EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.TipeeeStreamDonation));
                            if (command != null)
                            {
                                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                specialIdentifiers["donationsource"] = EnumHelper.GetEnumName(donation.Source);
                                specialIdentifiers["donationamount"] = donation.AmountText;
                                specialIdentifiers["donationmessage"] = donation.Message;
                                specialIdentifiers["donationimage"] = donation.ImageLink;
                                await command.Perform(user, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
                            }
                        });
                    }
                }
            });

            JObject joinRoomJObj = new JObject();
            joinRoomJObj["room"] = this.apiKey;
            joinRoomJObj["username"] = this.username;
            this.SocketSendWrapper("join-room", joinRoomJObj);

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

    public class TipeeeStreamService : OAuthServiceBase, ITipeeeStreamService, IDisposable
    {
        private const string BaseAddress = "https://api.tipeeestream.com/v1.0/";

        public const string ClientID = "9611_u5i668t3urk0wcksc84kcgsgckc04wk4ookw0so04kkwgw0cg";

        public const string ListeningURL = "https://mixitupapp.com";

        public const string AuthorizationURL = "https://api.tipeeestream.com/oauth/v2/auth?client_id={0}&response_type=code&redirect_uri={1}";
        public const string OAuthTokenURL = "https://api.tipeeestream.com/oauth/v2/token";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<TipeeeStreamEvent> OnDonationOccurred = delegate { };

        public bool WebSocketConnectedAndAuthenticated { get; private set; }

        private string authorizationToken;

        private TipeeeStreamWebSocketService socket;

        public TipeeeStreamService(string authorizationToken) : base(TipeeeStreamService.BaseAddress) { this.authorizationToken = authorizationToken; }

        public TipeeeStreamService(OAuthTokenModel token) : base(TipeeeStreamService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.InitializeInternal();

                    return true;
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }

            if (!string.IsNullOrEmpty(this.authorizationToken))
            {
                try
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TiltifyService.ClientID;
                    payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TipeeeStreamService.ListeningURL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/token", this.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = this.authorizationToken;
                        token.AcquiredDateTime = DateTimeOffset.Now;
                        token.expiresIn = int.MaxValue;

                        await this.InitializeInternal();

                        return true;
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

        public async Task<TipeeeStreamUser> GetUser()
        {
            try
            {
                return await this.GetAsync<TipeeeStreamUser>("me");
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task<string> GetAPIKey()
        {
            try
            {
                JObject jobj = await this.GetAsync<JObject>("me/api");
                if (jobj != null)
                {
                    return jobj["apiKey"].ToString();
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["client_id"] = TipeeeStreamService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;

                this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/refresh-token", this.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected async Task InitializeInternal()
        {
            TipeeeStreamUser user = await this.GetUser();
            if (user != null)
            {
                string apiKey = await this.GetAPIKey();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    this.socket = new TipeeeStreamWebSocketService(this, user.Username, apiKey);
                    await this.socket.Connect();
                    if (this.socket.Connected)
                    {

                    }
                }
            }
        }

        public void WebSocketConnectedOccurred()
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TipeeeStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    //this.cancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
