using Mixer.Base;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class PatreonService : OAuthServiceBase, IPatreonService, IDisposable
    {
        private const string BaseAddress = "https://www.patreon.com/api/oauth2/v2/";

        private const string ClientID = "SmP5OMjSF0JA2HAa14-so3it_vrs37MBdkd6AQOB8P8PFswXONRwLpzgDDzriTYZ";
        private const string AuthorizationUrl = "https://www.patreon.com/oauth2/authorize?response_type=code&client_id={0}&redirect_uri={1}&scope=identity%20campaigns%20campaigns.members";
        private const string TokenUrl = "https://www.patreon.com/api/oauth2/token";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private PatreonUser user;

        public PatreonCampaign Campaign { get; private set; }

        public IEnumerable<PatreonCampaignMember> CampaignMembers { get { return this.members; } }
        private List<PatreonCampaignMember> members = new List<PatreonCampaignMember>();
        private Dictionary<string, string> currentMembersAndTiers = new Dictionary<string, string>();

        public PatreonService() : base(PatreonService.BaseAddress) { }

        public PatreonService(OAuthTokenModel token) : base(PatreonService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    if (await this.InitializeInternal())
                    {
                        return true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
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
                JObject jobj = await this.GetJObjectAsync("identity?fields%5Buser%5D=created,first_name,full_name,last_name,url,vanity");
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
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<PatreonCampaign> GetCampaign()
        {
            PatreonCampaign campaign = null;
            try
            {
                JObject jobj = await this.GetAsync<JObject>("campaigns?include=tiers,benefits,tiers.benefits&fields%5Bcampaign%5D=created_at,creation_name,patron_count,published_at&fields%5Btier%5D=amount_cents,description,created_at,patron_count,title,image_url,published,published_at&fields%5Bbenefit%5D=title,benefit_type,rule_type,created_at,is_deleted,is_published");
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
                            if (included.ContainsKey("id") && int.TryParse(included["id"].ToString(), out int id) && id > 0)
                            {
                                if (included.ContainsKey("attributes"))
                                {
                                    JObject attributes = (JObject)included["attributes"];
                                    if (included.ContainsKey("type") && included["type"].ToString().Equals("tier"))
                                    {
                                        PatreonTier tier = attributes.ToObject<PatreonTier>();
                                        tier.ID = id.ToString();
                                        campaign.Tiers[tier.ID] = tier;

                                        if (included.ContainsKey("relationships"))
                                        {
                                            JObject relationships = (JObject)included["relationships"];
                                            if (relationships.ContainsKey("benefits"))
                                            {
                                                JObject benefits = (JObject)relationships["benefits"];
                                                if (benefits.ContainsKey("data"))
                                                {
                                                    JArray benefitsDataArray = (JArray)benefits["data"];
                                                    foreach (JObject benefitData in benefitsDataArray)
                                                    {
                                                        tier.BenefitIDs.Add(benefitData["id"].ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (included.ContainsKey("type") && included["type"].ToString().Equals("benefit"))
                                    {
                                        PatreonBenefit benefit = attributes.ToObject<PatreonBenefit>();
                                        benefit.ID = id.ToString();
                                        campaign.Benefits[benefit.ID] = benefit;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
            return campaign;
        }

        public async Task<IEnumerable<PatreonCampaignMember>> GetCampaignMembers()
        {
            List<PatreonCampaignMember> results = new List<PatreonCampaignMember>();
            string next = string.Format("campaigns/{0}/members?include=user,currently_entitled_tiers&fields%5Bmember%5D=patron_status,full_name,will_pay_amount_cents,currently_entitled_amount_cents,lifetime_support_cents&fields%5Buser%5D=created,first_name,full_name,last_name,url,vanity", this.Campaign.ID);
            try
            {
                do
                {
                    Dictionary<string, PatreonCampaignMember> currentResults = new Dictionary<string, PatreonCampaignMember>();

                    JObject jobj = await this.GetAsync<JObject>(next);
                    next = null;

                    if (jobj != null && jobj.ContainsKey("data"))
                    {
                        JArray dataArray = (JArray)jobj["data"];
                        foreach (JObject data in dataArray)
                        {
                            PatreonCampaignMember pledge = new PatreonCampaignMember();
                            pledge.ID = data["id"].ToString();

                            if (data.ContainsKey("attributes"))
                            {
                                JObject attributes = (JObject)data["attributes"];
                                if (attributes.ContainsKey("will_pay_amount_cents"))
                                {
                                    pledge.AmountToPay = (int)attributes["will_pay_amount_cents"];
                                    pledge.CurrentAmountPaying = (int)attributes["currently_entitled_amount_cents"];
                                    pledge.LifetimeAmountPaid = (int)attributes["lifetime_support_cents"];
                                    pledge.PatronStatus = attributes["patron_status"].ToString();
                                }
                            }

                            if (data.ContainsKey("relationships"))
                            {
                                JObject relationships = (JObject)data["relationships"];

                                if (relationships.ContainsKey("currently_entitled_tiers"))
                                {
                                    JObject entitledTiers = (JObject)relationships["currently_entitled_tiers"];
                                    if (entitledTiers.ContainsKey("data"))
                                    {
                                        JArray entitledTiersData = (JArray)entitledTiers["data"];
                                        if (entitledTiersData.Count > 0)
                                        {
                                            JObject entitledTierData = (JObject)entitledTiersData.First;
                                            pledge.TierID = entitledTierData["id"].ToString();
                                        }
                                    }
                                }

                                if (relationships.ContainsKey("user"))
                                {
                                    JObject user = (JObject)relationships["user"];
                                    if (user.ContainsKey("data"))
                                    {
                                        JObject userData = (JObject)user["data"];
                                        pledge.UserID = userData["id"].ToString();
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(pledge.ID) && !string.IsNullOrEmpty(pledge.UserID))
                            {
                                if (string.IsNullOrEmpty(pledge.TierID) && pledge.CurrentAmountPaying > 0)
                                {
                                    PatreonTier tier = this.Campaign.ActiveTiers.OrderByDescending(t => t.AmountCents).FirstOrDefault(t => pledge.CurrentAmountPaying >= t.AmountCents);
                                    if (tier != null)
                                    {
                                        pledge.TierID = tier.ID;
                                    }
                                }

                                if (!string.IsNullOrEmpty(pledge.TierID))
                                {
                                    currentResults[pledge.UserID] = pledge;
                                }
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
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
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
                this.Campaign = await this.GetCampaign();
                if (this.Campaign != null)
                {
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
            try
            {
                this.members = new List<PatreonCampaignMember>(await this.GetCampaignMembers());
                foreach (PatreonCampaignMember member in this.members)
                {
                    this.currentMembersAndTiers[member.UserID] = member.TierID;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(60000);

                try
                {
                    IEnumerable<PatreonCampaignMember> pledges = await this.GetCampaignMembers();
                    if (pledges.Count() > 0)
                    {
                        this.members = pledges.ToList();
                        foreach (PatreonCampaignMember member in this.members)
                        {
                            if (!this.currentMembersAndTiers.ContainsKey(member.UserID) || !this.currentMembersAndTiers[member.UserID].Equals(member.TierID))
                            {
                                PatreonTier tier = this.Campaign.GetTier(member.TierID);
                                if (tier != null)
                                {
                                    UserViewModel user = new UserViewModel(0, member.User.LookupName);

                                    UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(user.UserName);
                                    if (userModel != null)
                                    {
                                        user = await ChannelSession.ActiveUsers.GetUserByID(userModel.id);
                                        if (user == null)
                                        {
                                            user = new UserViewModel(userModel);
                                        }
                                        user.Data.PatreonUserID = member.UserID;
                                        await user.RefreshDetails(force: true);
                                    }

                                    EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.PatreonSubscribed));
                                    if (command != null)
                                    {
                                        Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                                        extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierNameSpecialIdentifier] = tier.Title;
                                        extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierAmountSpecialIdentifier] = tier.Amount.ToString();
                                        extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierImageSpecialIdentifier] = tier.ImageUrl;
                                        await command.Perform(user, arguments: null, extraSpecialIdentifiers: extraSpecialIdentifiers);
                                    }
                                }
                            }
                            this.currentMembersAndTiers[member.UserID] = member.TierID;
                        }
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
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
