using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class TiltifyService : OAuthServiceBase, ITiltifyService
    {
        private const string BaseAddress = "https://tiltify.com/api/v3/";

        public const string ClientID = "aa6b19e3f472808a632fe5a1b26b8ab37e852c123f60fb431c8a15c40df07f25";

        public const string ListeningURL = "https://localhost:8919/";
        public const string AuthorizationURL = "https://tiltify.com/oauth/authorize?client_id={0}&redirect_uri={1}&response_type=code";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private string authorizationToken;

        private TiltifyUser user;

        public TiltifyService(string authorizationToken) : base(TiltifyService.BaseAddress) { this.authorizationToken = authorizationToken; }

        public TiltifyService(OAuthTokenModel token) : base(TiltifyService.BaseAddress, token) { }

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
                    payload["client_secret"] = "3fd28348789f22af38bc140ea094bad57863cdc28794ed8ffb28ec6c6c3f6598";
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TiltifyService.ListeningURL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://tiltify.com/oauth/token", this.CreateContentFromObject(payload), autoRefreshToken: false);
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

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<TiltifyUser> GetUser()
        {
            try
            {
                TiltifyResult result = await this.GetAsync<TiltifyResult>("user");
                return result.Data.ToObject<TiltifyUser>();
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetCampaigns(TiltifyUser user)
        {
            List<TiltifyCampaign> results = new List<TiltifyCampaign>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("users/" + user.ID.ToString() + "/campaigns");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyCampaign>());
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return results;
        }

        public async Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign)
        {
            List<TiltifyDonation> results = new List<TiltifyDonation>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("campaigns/" + campaign.ID.ToString() + "/donations");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyDonation>());
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return results;
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.FromResult(0);
        }

        private async Task InitializeInternal()
        {
            this.user = await this.GetUser();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task BackgroundDonationCheck()
        {
            int currentCampaign = 0;
            TiltifyCampaign campaign = null;
            Dictionary<int, TiltifyDonation> donationsReceived = new Dictionary<int, TiltifyDonation>();

            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (ChannelSession.Settings.TiltifyCampaign != currentCampaign)
                    {
                        currentCampaign = ChannelSession.Settings.TiltifyCampaign;
                        donationsReceived.Clear();

                        IEnumerable<TiltifyCampaign> campaigns = await this.GetCampaigns(this.user);
                        campaign = campaigns.FirstOrDefault(c => c.ID.Equals(currentCampaign));
                        if (campaign != null)
                        {
                            foreach (TiltifyDonation donation in await this.GetCampaignDonations(campaign))
                            {
                                donationsReceived.Add(donation.ID, donation);
                            }
                        }
                    }

                    if (campaign != null)
                    {
                        foreach (TiltifyDonation tDonation in await this.GetCampaignDonations(campaign))
                        {
                            if (!donationsReceived.ContainsKey(tDonation.ID))
                            {
                                UserDonationModel donation = tDonation.ToGenericDonation();
                                GlobalEvents.DonationOccurred(donation);

                                UserViewModel user = new UserViewModel(0, donation.UserName);

                                UserModel userModel = await ChannelSession.Connection.GetUser(user.UserName);
                                if (userModel != null)
                                {
                                    user = new UserViewModel(userModel);
                                }

                                EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.TiltifyDonation));
                                if (command != null)
                                {
                                    command.AddSpecialIdentifier("donationsource", EnumHelper.GetEnumName(donation.Source));
                                    command.AddSpecialIdentifier("donationamount", donation.AmountText);
                                    command.AddSpecialIdentifier("donationmessage", donation.Message);
                                    command.AddSpecialIdentifier("donationimage", donation.ImageLink);
                                    await command.Perform(user);
                                }
                            }
                            donationsReceived[tDonation.ID] = tDonation;
                        }
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

                await Task.Delay(10000);
            }
        }
    }
}
