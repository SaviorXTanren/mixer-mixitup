using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class JustGivingUser
    {
        public Guid userId { get; set; }
        public uint accountId { get; set; }
        public JArray profileImageUrls { get; set; }
    }

    public class JustGivingFundraiserSummary
    {
        public uint charityId { get; set; }
        public uint pageId { get; set; }

        public string pageShortName { get; set; }

        public string pageStatus { get; set; }

        public bool IsActive { get { return (!string.IsNullOrEmpty(this.pageStatus) && this.pageStatus.Equals("Active")); } }
    }

    public class JustGivingCharity
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string logoAbsoluteUrl { get; set; }
    }

    public class JustGivingFundraiser
    {
        public string pageId { get; set; }
        public string pageShortName { get; set; }
        public string pageGuid { get; set; }

        public string activityId { get; set; }

        public long eventId { get; set; }
        public string eventName { get; set; }
        public string eventCategory { get; set; }

        public string title { get; set; }
        public string status { get; set; }

        public string fundraisingTarget { get; set; }
        public string totalRaisedPercentageOfFundraisingTarget { get; set; }
        public string totalRaisedOffline { get; set; }
        public string totalRaisedOnline { get; set; }
        public string totalRaisedSms { get; set; }
        public string grandTotalRaisedExcludingGiftAid { get; set; }
        public string totalEstimatedGiftAid { get; set; }

        public JustGivingCharity charity { get; set; }

        public bool IsActive { get { return (!string.IsNullOrEmpty(this.status) && this.status.Equals("Active")); } }
    }

    public class JustGivingDonationGroup
    {
        public string id { get; set; }
        public string pageShortName { get; set; }
        public List<JustGivingDonation> donations { get; set; }
        public JObject pagination { get; set; }
    }

    public class JustGivingDonation
    {
        public uint id { get; set; }

        public string donorDisplayName { get; set; }
        public string donorRealName { get; set; }

        public string image { get; set; }
        public string message { get; set; }
        public string donationDate { get; set; }

        public string amount { get; set; }
        public string currencyCode { get; set; }
        public string donorLocalAmount { get; set; }
        public string donorLocalCurrencyCode { get; set; }

        public DateTimeOffset DateTime
        {
            get
            {
                if (!string.IsNullOrEmpty(this.donationDate))
                {
                    string date = this.donationDate.Replace("/Date(", string.Empty);
                    date = date.Replace(")/", string.Empty);
                    if (long.TryParse(date, out long dateLong))
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(dateLong);
                    }
                }
                return DateTimeOffset.MinValue;
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.JustGiving,

                ID = this.id.ToString(),
                Username = this.donorDisplayName,
                Message = this.message,

                Amount = double.Parse(this.donorLocalAmount),

                DateTime = this.DateTime,
            };
        }
    }

    public class JustGivingService : IExternalService
    {
        private const string BaseAddress = "https://api.justgiving.com/v1/";

        private const string ClientID = "1e30b383";

        public string Name { get { return MixItUp.Base.Resources.JustGiving; } }
        public bool IsConnected { get; private set; }

        public JustGivingFundraiser Fundraiser { get; private set; }


        private Dictionary<uint, JustGivingDonation> donationsReceived = new Dictionary<uint, JustGivingDonation>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DateTimeOffset startTime;

        public JustGivingService() { }

        public async Task<Result> Connect()
        {
            try
            {
                if (!string.IsNullOrEmpty(ChannelSession.Settings.JustGivingPageShortName))
                {
                    this.Fundraiser = await this.GetFundraiser(ChannelSession.Settings.JustGivingPageShortName);
                    if (this.Fundraiser != null)
                    {
                        this.cancellationTokenSource = new CancellationTokenSource();

                        this.startTime = DateTimeOffset.Now;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        ServiceManager.Get<ITelemetryService>().TrackService("JustGiving");
                        this.IsConnected = true;
                        return new Result();
                    }
                }
                return new Result(Resources.JustGivingUserDataFailed);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public Task Disconnect()
        {
            this.Fundraiser = null;
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }
            this.IsConnected = false;
            return Task.CompletedTask;
        }

        public async Task<JustGivingFundraiser> GetFundraiser(string pageShortName)
        {
            try
            {
                using (AdvancedHttpClient client = this.GetHttpClient())
                {
                    return await client.GetAsync<JustGivingFundraiser>($"fundraising/pages/{pageShortName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<IEnumerable<JustGivingDonation>> GetRecentDonations(JustGivingFundraiser fundraiser)
        {
            try
            {
                using (AdvancedHttpClient client = this.GetHttpClient())
                {
                    JustGivingDonationGroup group = await client.GetAsync<JustGivingDonationGroup>($"fundraising/pages/{fundraiser.pageShortName}/donations");
                    if (group != null && group.donations != null)
                    {
                        return group.donations;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<JustGivingDonation>();
        }

        private AdvancedHttpClient GetHttpClient()
        {
            AdvancedHttpClient client = new AdvancedHttpClient(JustGivingService.BaseAddress);
            client.DefaultRequestHeaders.Add("x-app-id", JustGivingService.ClientID);
            client.DefaultRequestHeaders.Add("x-application-key", ServiceManager.Get<SecretsService>().GetSecret("JustGivingSecret"));
            return client;
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                if (this.Fundraiser != null)
                {
                    foreach (JustGivingDonation jgDonation in await this.GetRecentDonations(this.Fundraiser))
                    {
                        if (!donationsReceived.ContainsKey(jgDonation.id))
                        {
                            donationsReceived[jgDonation.id] = jgDonation;
                            UserDonationModel donation = jgDonation.ToGenericDonation();
                            if (donation.DateTime > this.startTime)
                            {
                                await EventService.ProcessDonationEvent(EventTypeEnum.JustGivingDonation, donation);
                            }
                        }
                    }
                }
            }
        }
    }
}
