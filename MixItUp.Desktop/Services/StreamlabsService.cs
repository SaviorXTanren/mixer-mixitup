using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class StreamlabsWebSocketService
    {
        private Socket socket;

        public StreamlabsWebSocketService(string socketToken)
        {
            this.socket = IO.Socket(string.Format("https://sockets.streamlabs.com?token=${0}", socketToken));

            this.socket.On("event", (eventData) =>
            {
                try
                {
                    if (eventData != null)
                    {
                        //StreamlabsEventPacket packet = JsonConvert.DeserializeObject<StreamlabsEventPacket>(eventData.ToString());
                        //if (packet.type.Equals("donation"))
                        //{
                            //StreamlabsDonation donation = new StreamlabsDonation(packet.message);
                            //GlobalEvents.DonationOccurred(donation.ToGenericDonation());
                        //}
                    }
                }
                catch (Exception ex)
                {
                    MixItUp.Base.Util.Logger.Log(ex);
                }
            });
        }

        public void Disconnect()
        {
            this.socket.Close();
        }
    }

    [DataContract]
    public class StreamlabsService : OAuthServiceBase, IStreamlabsService
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        //private StreamlabsWebSocketService websocketService;

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

        public Task Disconnect()
        {
            this.token = null;
            //this.websocketService.Disconnect();
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<StreamlabsDonation>> GetDonations()
        {
            List<StreamlabsDonation> results = new List<StreamlabsDonation>();
            try
            {
                HttpResponseMessage response = await this.GetAsync("donations");
                JObject jobj = await this.ProcessJObjectResponse(response);
                foreach (var donation in (JArray)jobj["data"])
                {
                    results.Add(donation.ToObject<StreamlabsDonation>());
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return results;
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

            //this.websocketService = new StreamlabsWebSocketService(jobj["socket_token"].ToString());

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task BackgroundDonationCheck()
        {
            Dictionary<int, StreamlabsDonation> donationsReceived = new Dictionary<int, StreamlabsDonation>();

            IEnumerable<StreamlabsDonation> donations = await this.GetDonations();
            foreach (StreamlabsDonation donation in donations)
            {
                donationsReceived[donation.ID] = donation;
            }

            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (StreamlabsDonation slDonation in await this.GetDonations())
                    {
                        if (!donationsReceived.ContainsKey(slDonation.ID))
                        {
                            UserDonationModel donation = slDonation.ToGenericDonation();
                            GlobalEvents.DonationOccurred(donation);

                            UserViewModel user = new UserViewModel(0, donation.Username);

                            UserModel userModel = await ChannelSession.Connection.GetUser(user.UserName);
                            if (userModel != null)
                            {
                                user = new UserViewModel(userModel);
                            }

                            EventCommand command = ChannelSession.Settings.EventCommands.ToList().FirstOrDefault(c => c.MatchesEvent(EnumHelper.GetEnumName(OtherEventTypeEnum.GawkBoxDonation)));
                            if (command != null)
                            {
                                command.AddSpecialIdentifier("donationsource", donation.Source);
                                command.AddSpecialIdentifier("donationamount", donation.AmountText);
                                command.AddSpecialIdentifier("donationmessage", donation.Message);
                                command.AddSpecialIdentifier("donationimage", donation.ImageLink);
                                await command.Perform(user);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MixItUp.Base.Util.Logger.Log(ex);
                }
                await Task.Delay(10000);
            }
        }
    }
}
