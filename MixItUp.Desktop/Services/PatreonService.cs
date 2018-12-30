using Mixer.Base;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class PatreonService : OAuthServiceBase, IPatreonService, IDisposable
    {
        private const string BaseAddress = "https://www.patreon.com/api/oauth2/api/";

        private const string ClientID = "8TS-v-eGQQ7odSI6jup4A1-4mWkSarkup2Vn6Na402tfItwDEn-2pW30PVf442Nf";
        private const string AuthorizationUrl = "https://www.patreon.com/oauth2/authorize?response_type=code&client_id={0}&redirect_uri={1}";
        private const string TokenUrl = "https://www.patreon.com/api/oauth2/token";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private PatreonUser user;
        private PatreonCampaign campaign;

        public PatreonService() : base(PatreonService.BaseAddress) { }

        public PatreonService(OAuthTokenModel token) : base(PatreonService.BaseAddress, token) { }

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

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(PatreonService.AuthorizationUrl, PatreonService.ClientID, MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", PatreonService.ClientID),
                    new KeyValuePair<string, string>("client_secret", ChannelSession.SecretManager.GetSecret("PatreonSecret")),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("code", authorizationCode),
                };

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(PatreonService.TokenUrl, PatreonService.ClientID, ChannelSession.SecretManager.GetSecret("PatreonSecret"), body);
                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

                    return await this.InitializeInternal();
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.FromResult(0);
        }

        public async Task<PatreonUser> GetCurrentUser()
        {
            try
            {
                JObject jobj = await this.GetJObjectAsync("current_user");
                if (jobj != null && jobj.ContainsKey("data"))
                {
                    JObject data = (JObject)jobj["data"];
                    if (data != null && data.ContainsKey("attributes"))
                    {
                        JObject attributes = (JObject)data["attributes"];
                        PatreonUser user = attributes.ToObject<PatreonUser>();
                        user.ID = data["id"].ToString();
                        return user;
                    }
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task<PatreonCampaign> GetCampaign()
        {
            PatreonCampaign campaign = null;
            try
            {
                JObject jobj = await this.GetAsync<JObject>("current_user/campaigns?includes=rewards");
                if (jobj != null && jobj.ContainsKey("data"))
                {
                    JArray dataArray = (JArray)jobj["data"];
                    JObject data = (JObject)dataArray.First;
                    if (data != null && data.ContainsKey("attributes"))
                    {
                        JObject attributes = (JObject)data["attributes"];
                        campaign = attributes.ToObject<PatreonCampaign>();
                        campaign.ID = data["id"].ToString();
                    }

                    if (campaign != null && jobj.ContainsKey("included"))
                    {
                        JArray includedArray = (JArray)jobj["included"];
                        foreach (JObject included in includedArray)
                        {
                            if (included.ContainsKey("id") && int.TryParse(included["id"].ToString(), out int id) && id > 0 &&
                                included.ContainsKey("type") && included["type"].ToString().Equals("reward"))
                            {
                                if (included.ContainsKey("attributes"))
                                {
                                    JObject attributes = (JObject)included["attributes"];
                                    PatreonTier tier = attributes.ToObject<PatreonTier>();
                                    tier.ID = data["id"].ToString();
                                    campaign.Tiers.Add(tier);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return campaign;
        }

        public async Task<IEnumerable<PatreonPledge>> GetPledges()
        {
            List<PatreonPledge> results = new List<PatreonPledge>();
            string next = string.Format("campaigns/{0}/pledges?include=patron.null", this.campaign.ID);
            try
            {
                do
                {
                    Dictionary<string, PatreonPledge> currentResults = new Dictionary<string, PatreonPledge>();

                    JObject jobj = await this.GetAsync<JObject>(next);
                    next = null;

                    if (jobj != null && jobj.ContainsKey("data"))
                    {
                        JArray dataArray = (JArray)jobj["data"];
                        foreach (JObject data in dataArray)
                        {
                            PatreonPledge pledge = new PatreonPledge();
                            pledge.ID = data["id"].ToString();

                            if (data.ContainsKey("attributes"))
                            {
                                JObject attributes = (JObject)data["attributes"];
                                if (attributes.ContainsKey("amount_cents"))
                                {
                                    pledge.AmountCents = (int)data["attributes"]["amount_cents"];
                                }
                            }

                            if (data.ContainsKey("relationships"))
                            {
                                JObject relationships = (JObject)data["relationships"];

                                if (relationships.ContainsKey("patron"))
                                {
                                    JObject patron = (JObject)data["patron"];
                                    if (patron.ContainsKey("data"))
                                    {
                                        JObject patronData = (JObject)patron["data"];
                                        pledge.UserID = patronData["id"].ToString();
                                    }
                                }

                                if (relationships.ContainsKey("reward"))
                                {
                                    JObject reward = (JObject)data["reward"];
                                    if (reward.ContainsKey("data"))
                                    {
                                        JObject rewardData = (JObject)reward["data"];
                                        pledge.TierID = rewardData["id"].ToString();
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(pledge.ID) && !string.IsNullOrEmpty(pledge.UserID) && !string.IsNullOrEmpty(pledge.TierID))
                            {
                                currentResults[pledge.UserID] = pledge;
                            }
                        }

                        if (jobj.ContainsKey("included"))
                        {
                            JArray includedArray = (JArray)jobj["included"];
                            foreach (JObject included in includedArray)
                            {
                                if (included.ContainsKey("type") && included["type"].ToString().Equals("user") && included.ContainsKey("attributes"))
                                {
                                    JObject attributes = (JObject)included["attributes"];
                                    PatreonUser user = attributes.ToObject<PatreonUser>();
                                    user.ID = included["id"].ToString();
                                    if (currentResults.ContainsKey(user.ID))
                                    {
                                        currentResults[user.ID].User = user;
                                        results.Add(currentResults[user.ID]);
                                    }
                                }
                            }
                        }

                        if (jobj.ContainsKey("links"))
                        {
                            JObject links = (JObject)jobj["links"];
                            if (links.ContainsKey("next") && links["next"] != null)
                            {
                                next = links["next"].ToString();
                            }
                        }
                    }
                } while (!string.IsNullOrEmpty(next));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return results;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", PatreonService.ClientID),
                    new KeyValuePair<string, string>("client_secret", ChannelSession.SecretManager.GetSecret("PatreonSecret")),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(PatreonService.TokenUrl, PatreonService.ClientID, ChannelSession.SecretManager.GetSecret("PatreonSecret"), body);
            }
        }

        private async Task<bool> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.user = await this.GetCurrentUser();
            if (this.user != null)
            {
                this.campaign = await this.GetCampaign();
                if (this.campaign != null)
                {
                    IEnumerable<PatreonPledge> pledges = await this.GetPledges();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
            }
            return false;
        }

        private async Task BackgroundDonationCheck()
        {
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {

                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

                await Task.Delay(10000);
            }
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
                    this.cancellationTokenSource.Dispose();
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
