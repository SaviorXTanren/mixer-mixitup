using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class PatreonUser : IEquatable<PatreonUser>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("vanity")]
        public string Vanity { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset? Created { get; set; }

        [JsonProperty("social_connections")]
        public JObject SocialConnections { get; set; }

        public PatreonUser() { }

        [JsonIgnore]
        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TwitchUserID))
                {
                    return StreamingPlatformTypeEnum.Twitch;
                }
                return StreamingPlatformTypeEnum.None;
            }
        }

        [JsonIgnore]
        public string PlatformUserID
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TwitchUserID))
                {
                    return this.TwitchUserID;
                }
                return null;
            }
        }

        [JsonIgnore]
        public string PlatformUsername
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TwitchUsername))
                {
                    return this.TwitchUsername;
                }
                else if (!string.IsNullOrEmpty(this.Vanity))
                {
                    return this.Vanity;
                }
                return this.FullName;
            }
        }

        [JsonIgnore]
        public string TwitchUserID
        {
            get
            {
                JObject twitchData = this.TwitchData;
                if (twitchData != null && twitchData.ContainsKey("user_id"))
                {
                    return twitchData["user_id"].ToString();
                }
                return null;
            }
        }

        [JsonIgnore]
        public string TwitchUsername
        {
            get
            {
                JObject twitchData = this.TwitchData;
                if (twitchData != null && twitchData.ContainsKey("url"))
                {
                    return twitchData["url"].ToString().Replace("https://twitch.tv/", "");
                }
                return null;
            }
        }

        [JsonIgnore]
        public JObject TwitchData
        {
            get
            {
                if (this.SocialConnections != null && this.SocialConnections.ContainsKey("twitch") && this.SocialConnections["twitch"] != null && this.SocialConnections["twitch"] is JObject)
                {
                    return (JObject)this.SocialConnections["twitch"];
                }
                return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PatreonUser)
            {
                return this.Equals((PatreonUser)obj);
            }
            return false;
        }

        public bool Equals(PatreonUser other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonCampaign : IEquatable<PatreonCampaign>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("creation_name")]
        public string CreationName { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [DataMember]
        public Dictionary<string, PatreonTier> Tiers { get; set; } = new Dictionary<string, PatreonTier>();

        [DataMember]
        public Dictionary<string, PatreonBenefit> Benefits { get; set; } = new Dictionary<string, PatreonBenefit>();

        public PatreonCampaign()
        {
            this.Tiers = new Dictionary<string, PatreonTier>();
            this.Benefits = new Dictionary<string, PatreonBenefit>();
        }

        [JsonProperty]
        public IEnumerable<PatreonTier> ActiveTiers { get { return this.Tiers.Values.Where(t => t.Published); } }

        public PatreonTier GetTier(string tierID)
        {
            if (!string.IsNullOrEmpty(tierID) && this.Tiers.ContainsKey(tierID) && this.Tiers[tierID].Published)
            {
                return this.Tiers[tierID];
            }
            return null;
        }

        public PatreonBenefit GetBenefit(string benefitID)
        {
            if (!string.IsNullOrEmpty(benefitID) && this.Benefits.ContainsKey(benefitID) && this.Benefits[benefitID].Published && !this.Benefits[benefitID].Deleted)
            {
                return this.Benefits[benefitID];
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is PatreonCampaign)
            {
                return this.Equals((PatreonCampaign)obj);
            }
            return false;
        }

        public bool Equals(PatreonCampaign other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonTier : IEquatable<PatreonTier>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("amount_cents")]
        public int AmountCents { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [DataMember]
        public HashSet<string> BenefitIDs { get; set; } = new HashSet<string>();

        [JsonIgnore]
        public double Amount { get { return Math.Round(((double)this.AmountCents) / 100.0, 2); } }

        public PatreonTier()
        {
            this.BenefitIDs = new HashSet<string>();
        }

        public override bool Equals(object obj)
        {
            if (obj is PatreonTier)
            {
                return this.Equals((PatreonTier)obj);
            }
            return false;
        }

        public bool Equals(PatreonTier other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonBenefit : IEquatable<PatreonBenefit>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("benefit_type")]
        public string BenefitType { get; set; }

        [JsonProperty("rule_type")]
        public string RuleType { get; set; }

        [JsonProperty("is_published")]
        public bool Published { get; set; }

        [JsonProperty("is_deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        public PatreonBenefit() { }

        public override bool Equals(object obj)
        {
            if (obj is PatreonBenefit)
            {
                return this.Equals((PatreonBenefit)obj);
            }
            return false;
        }

        public bool Equals(PatreonBenefit other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonCampaignMember : IEquatable<PatreonCampaignMember>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("patron_status")]
        public string PatronStatus { get; set; }

        [JsonProperty("will_pay_amount_cents")]
        public int AmountToPay { get; set; }

        [JsonProperty("currently_entitled_amount_cents")]
        public int CurrentAmountPaying { get; set; }

        [JsonProperty("lifetime_support_cents")]
        public int LifetimeAmountPaid { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public PatreonUser User { get; set; }

        [DataMember]
        public string TierID { get; set; }

        [JsonIgnore]
        public double Amount { get { return Math.Round(((double)this.AmountToPay) / 100.0, 2); } }

        public PatreonCampaignMember() { }

        public override bool Equals(object obj)
        {
            if (obj is PatreonCampaignMember)
            {
                return this.Equals((PatreonCampaignMember)obj);
            }
            return false;
        }

        public bool Equals(PatreonCampaignMember other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class PatreonService : OAuthExternalServiceBase
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

        public override string Name { get { return MixItUp.Base.Resources.Patreon; } }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(PatreonService.AuthorizationUrl, PatreonService.ClientID, OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("client_id", PatreonService.ClientID),
                        new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PatreonSecret")),
                        new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL),
                        new KeyValuePair<string, string>("code", authorizationCode),
                    };

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(PatreonService.TokenUrl, PatreonService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("PatreonSecret"), body);
                    if (this.token != null)
                    {
                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(false);
        }

        public override Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.CompletedTask;
        }

        public async Task<PatreonUser> GetCurrentUser()
        {
            try
            {
                JObject jobj = await this.GetJObjectAsync("identity?fields%5Buser%5D=created,first_name,full_name,last_name,url,vanity,social_connections");
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
            string next = string.Format("campaigns/{0}/members?include=user,currently_entitled_tiers&fields%5Bmember%5D=patron_status,full_name,will_pay_amount_cents,currently_entitled_amount_cents,lifetime_support_cents&fields%5Buser%5D=created,first_name,full_name,last_name,url,vanity,social_connections", this.Campaign.ID);
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
                    new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PatreonSecret")),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(PatreonService.TokenUrl, PatreonService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("PatreonSecret"), body);
            }
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.user = await this.GetCurrentUser();
            if (this.user != null)
            {
                this.Campaign = await this.GetCampaign();
                if (this.Campaign != null)
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    this.TrackServiceTelemetry("Patreon");
                    return new Result();
                }
                return new Result(Resources.PatreonCampaignDataFailed);
            }
            return new Result(Resources.PatreonUserDataFailed);
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            IEnumerable<PatreonCampaignMember> pledges = await this.GetCampaignMembers();
            if (pledges != null && pledges.Count() > 0)
            {
                this.members = pledges.ToList();
                foreach (PatreonCampaignMember member in this.members)
                {
                    if (!this.currentMembersAndTiers.ContainsKey(member.UserID) || !this.currentMembersAndTiers[member.UserID].Equals(member.TierID))
                    {
                        PatreonTier tier = this.Campaign.GetTier(member.TierID);
                        if (tier != null)
                        {
                            CommandParametersModel parameters = new CommandParametersModel();

                            parameters.User = await ServiceManager.Get<UserService>().GetUserByPlatform(member.User.Platform, platformID: member.User.PlatformUserID);
                            if (parameters.User != null)
                            {
                                parameters.User.PatreonUser = member;
                            }
                            else
                            {
                                parameters.User = UserV2ViewModel.CreateUnassociated(member.User.PlatformUsername);
                            }

                            parameters.SpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierNameSpecialIdentifier] = tier.Title;
                            parameters.SpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierAmountSpecialIdentifier] = tier.Amount.ToString();
                            parameters.SpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierImageSpecialIdentifier] = tier.ImageUrl;
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.PatreonSubscribed, parameters);
                        }
                    }
                    this.currentMembersAndTiers[member.UserID] = member.TierID;
                }
            }
        }
    }
}
