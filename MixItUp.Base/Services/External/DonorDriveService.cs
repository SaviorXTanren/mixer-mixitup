using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class DonorDriveCharity
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class DonorDriveParticipant
    {
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public DateTime campaignDate { get; set; }
        [DataMember]
        public string campaignName { get; set; }
        [DataMember]
        public DateTime createdDateUTC { get; set; }
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public int eventID { get; set; }
        [DataMember]
        public string eventName { get; set; }
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public bool hasActivityTracking { get; set; }
        [DataMember]
        public bool isCustomAvatarImage { get; set; }
        [DataMember]
        public bool isTeamCaptain { get; set; }
        [DataMember]
        public bool isTeamCoCaptain { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public int numAwardedBadges { get; set; }
        [DataMember]
        public int numDonations { get; set; }
        [DataMember]
        public int numIncentives { get; set; }
        [DataMember]
        public int numMilestones { get; set; }
        [DataMember]
        public int participantID { get; set; }
        [DataMember]
        public string role { get; set; }
        [DataMember]
        public string streamingChannel { get; set; }
        [DataMember]
        public string streamingPlatform { get; set; }
        [DataMember]
        public bool streamIsEnabled { get; set; }
        [DataMember]
        public bool streamIsLive { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public double sumPledges { get; set; }
        [DataMember]
        public int teamID { get; set; }
        [DataMember]
        public string teamName { get; set; }
    }

    [DataContract]
    public class DonorDriveTeam
    {
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public string captainDisplayName { get; set; }
        [DataMember]
        public DateTime createdDateUTC { get; set; }
        [DataMember]
        public int eventID { get; set; }
        [DataMember]
        public string eventName { get; set; }
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public bool hasActivityTracking { get; set; }
        [DataMember]
        public bool hasExclusiveCode { get; set; }
        [DataMember]
        public bool hasTeamOnlyDonations { get; set; }
        [DataMember]
        public bool isCustomAvatarImage { get; set; }
        [DataMember]
        public bool isInviteOnly { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int numAwardedBadges { get; set; }
        [DataMember]
        public int numDonations { get; set; }
        [DataMember]
        public int numParticipants { get; set; }
        [DataMember]
        public int sourceTeamID { get; set; }
        [DataMember]
        public bool streamIsLive { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public double sumPledges { get; set; }
        [DataMember]
        public int teamID { get; set; }
    }

    [DataContract]
    public class DonorDriveEvent
    {
        [DataMember]
        public string avatarImage { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public DateTime donCutoffDateUTC { get; set; }
        [DataMember]
        public string expressRegistrationParticipantTypeID { get; set; }
        [DataMember]
        public JObject features { get; set; }
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public DateTime endDateUTC { get; set; }
        [DataMember]
        public int eventID { get; set; }
        [DataMember]
        public bool hasExpressRegistration { get; set; }
        [DataMember]
        public bool hasDates { get; set; }
        [DataMember]
        public bool hasLocation { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int numDonations { get; set; }
        [DataMember]
        public int numParticipants { get; set; }
        [DataMember]
        public int numTeams { get; set; }
        [DataMember]
        public DateTime publishDateUTC { get; set; }
        [DataMember]
        public DateTime regCutoffDateUTC { get; set; }
        [DataMember]
        public DateTime startDateUTC { get; set; }
        [DataMember]
        public bool streamIsLive { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public string timeZone { get; set; }
        [DataMember]
        public string type { get; set; }
    }

    [DataContract]
    public class DonorDriveActivity
    {
        [DataMember]
        public double amount { get; set; }
        [DataMember]
        public DateTime createdDateUTC { get; set; }
        [DataMember]
        public string imageURL { get; set; }
        [DataMember]
        public bool isIncentive { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string type { get; set; }
    }

    [DataContract]
    public class DonorDriveDonation
    {
        [DataMember]
        public double amount { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public DateTime createdDateUTC { get; set; }
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public string donationID { get; set; }
        [DataMember]
        public string donorID { get; set; }
        [DataMember]
        public bool donorIsRecipient { get; set; }
        [DataMember]
        public int eventID { get; set; }
        [DataMember]
        public string incentiveID { get; set; }
        [DataMember]
        public bool isRegFee { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public int participantID { get; set; }
        [DataMember]
        public string recipientImageURL { get; set; }
        [DataMember]
        public string recipientName { get; set; }
        [DataMember]
        public string teamID { get; set; }

        public UserDonationModel ToGenericDonation()
        {
            bool isAnonymous = false;
            string username = this.displayName;
            if (string.IsNullOrWhiteSpace(username))
            {
                username = MixItUp.Base.Resources.Anonymous;
                isAnonymous = true;
            }

            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.DonorDrive,
                IsAnonymous = isAnonymous,

                ID = this.donationID,
                Username = username,
                Message = this.message,

                Amount = Math.Round(this.amount, 2),

                DateTime = new DateTimeOffset(this.createdDateUTC, TimeSpan.Zero),
            };
        }
    }

    [DataContract]
    public class DonorDriveIncentive
    {
        [DataMember]
        public double amount { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public DateTime endDateUTC { get; set; }
        [DataMember]
        public object fulfillmentNote { get; set; }
        [DataMember]
        public string incentiveID { get; set; }
        [DataMember]
        public object incentiveImageURL { get; set; }
        [DataMember]
        public bool isActive { get; set; }
        [DataMember]
        public bool isExternalIncentive { get; set; }
        [DataMember]
        public bool isScheduled { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public int quantity { get; set; }
        [DataMember]
        public int quantityClaimed { get; set; }
        [DataMember]
        public DateTime startDateUTC { get; set; }
    }

    [DataContract]
    public class DonorDriveDonor
    {
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public string donorID { get; set; }
        [DataMember]
        public DateTime modifiedDateUTC { get; set; }
        [DataMember]
        public int numDonations { get; set; }
        [DataMember]
        public string recipientImageURL { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
    }

    [DataContract]
    public class DonorDriveMilestone
    {
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public DateTime endDateUTC { get; set; }
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public DonorDriveLinks links { get; set; }
        [DataMember]
        public bool isActive { get; set; }
        [DataMember]
        public bool isComplete { get; set; }
        [DataMember]
        public string milestoneID { get; set; }
        [DataMember]
        public DateTime startDateUTC { get; set; }
    }

    [DataContract]
    public class DonorDriveLinks
    {
        [DataMember]
        public string donate { get; set; }
        [DataMember]
        public string page { get; set; }
        [DataMember]
        public string recipient { get; set; }
        [DataMember]
        public string register { get; set; }
        [DataMember]
        public string stream { get; set; }
    }

    public class DonorDriveService : IExternalService
    {
        private const string APIVersion = "1.3";

        public static readonly DonorDriveCharity CustomCharity = new DonorDriveCharity()
        {
            name = Resources.Custom,
            url = string.Empty
        };

        public string Name { get { return Resources.DonorDrive; }}

        public bool IsConnected { get; private set; }

        public string BaseAddress { get; protected set; }

        public DonorDriveParticipant Participant { get; private set; }
        public DonorDriveTeam Team { get; private set; }
        public DonorDriveEvent Event { get; private set; }
        public IEnumerable<DonorDriveMilestone> Milestones { get; private set; }
        public IEnumerable<DonorDriveIncentive> Incentives { get; private set; }

        private CancellationTokenSource cancellationTokenSource;

        private DateTime startTime;
        private Dictionary<string, DonorDriveDonation> donationsProcessed = new Dictionary<string, DonorDriveDonation>();
        private double donationTotal;

        private DateTimeOffset lastRefresh = DateTimeOffset.MinValue;

        public DonorDriveService() { }

        public async Task<Result> Connect()
        {
            try
            {
                if (!string.IsNullOrEmpty(ChannelSession.Settings.DonorDriveCharityURL) && !string.IsNullOrEmpty(ChannelSession.Settings.DonorDriveParticipantID))
                {
                    this.BaseAddress = ChannelSession.Settings.DonorDriveCharityURL;

                    this.Participant = await this.GetParticipant(ChannelSession.Settings.DonorDriveParticipantID);
                    if (this.Participant == null)
                    {
                        return new Result(Resources.DonorDriveFailedToGetData);
                    }
                    this.donationTotal = this.Participant.sumDonations;

                    await Task.Delay(1000);

                    this.Event = await this.GetEvent(this.Participant.eventID.ToString());
                    if (this.Event == null)
                    {
                        return new Result(Resources.DonorDriveFailedToGetData);
                    }

                    if (this.Participant.teamID > 0)
                    {
                        await Task.Delay(1000);

                        this.Team = await this.GetTeam(this.Participant.teamID.ToString());
                        if (this.Team == null)
                        {
                            return new Result(Resources.DonorDriveFailedToGetData);
                        }
                    }

                    if (this.Participant.numMilestones > 0)
                    {
                        await Task.Delay(1000);

                        this.Milestones = await this.GetParticipantMilestones(this.Participant.participantID.ToString());
                        if (this.Milestones == null)
                        {
                            return new Result(Resources.DonorDriveFailedToGetData);
                        }

                        this.Milestones = this.Milestones.Where(m => m.isActive && !m.isComplete);
                    }

                    if (this.Participant.numIncentives > 0)
                    {
                        await Task.Delay(1000);

                        this.Incentives = await this.GetParticipantIncentives(this.Participant.participantID.ToString());
                        if (this.Incentives == null)
                        {
                            return new Result(Resources.DonorDriveFailedToGetData);
                        }
                    }

                    this.startTime = DateTime.UtcNow;

                    this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    this.IsConnected = true;
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.DonorDriveFailedToGetData);
        }

        public Task Disconnect()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            this.Participant = null;
            this.Team = null;
            this.Event = null;
            this.donationsProcessed.Clear();
            this.donationTotal = 0.0;

            this.IsConnected = false;

            return Task.CompletedTask;
        }

        public Task<IEnumerable<DonorDriveCharity>> GetCharities()
        {
            IEnumerable<DonorDriveCharity> charities = new List<DonorDriveCharity>()
            {
                new DonorDriveCharity()
                {
                    name = "Integration",
                    url = "https://integrations.donordrive.com"
                },
                new DonorDriveCharity()
                {
                    name = "Extra Life",
                    url = "https://www.extra-life.org"
                },
            };

            if (charities != null)
            {
                List<DonorDriveCharity> results = new List<DonorDriveCharity>(charities.OrderBy(c => c.name));
                results.Add(DonorDriveService.CustomCharity);

                return Task.FromResult<IEnumerable<DonorDriveCharity>>(results);
            }
            return Task.FromResult<IEnumerable<DonorDriveCharity>>(null);
        }

        public async Task RefreshData()
        {
            try
            {
                if ((DateTimeOffset.Now - this.lastRefresh).TotalMinutes >= 5)
                {
                    DonorDriveParticipant participant = await this.GetParticipant(this.Participant.participantID.ToString());
                    if (participant != null)
                    {
                        this.Participant = participant;
                    }

                    DonorDriveEvent ddEvent = await this.GetEvent(this.Event.eventID.ToString());
                    if (ddEvent != null)
                    {
                        this.Event = ddEvent;
                    }

                    if (this.Team != null)
                    {
                        DonorDriveTeam team = await this.GetTeam(this.Team.teamID.ToString());
                        if (team != null)
                        {
                            this.Team = team;
                        }
                    }

                    if (this.Participant.numMilestones > 0)
                    {
                        IEnumerable<DonorDriveMilestone> milestones = await this.GetParticipantMilestones(this.Participant.participantID.ToString());
                        if (milestones != null)
                        {
                            this.Milestones = milestones.Where(m => m.isActive && !m.isComplete);
                        }
                    }

                    if (this.Participant.numIncentives > 0)
                    {
                        IEnumerable<DonorDriveIncentive> incentives = await this.GetParticipantIncentives(this.Participant.participantID.ToString());
                        if (incentives != null)
                        {
                            this.Incentives = incentives;
                        }
                    }

                    this.lastRefresh = DateTimeOffset.Now;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<DonorDriveParticipant> GetParticipant(string participantID) { return await this.GetAsync<DonorDriveParticipant>($"participants/{participantID}"); }

        public async Task<IEnumerable<DonorDriveActivity>> GetParticipantActivity(string participantID) { return await this.GetAsync<IEnumerable<DonorDriveActivity>>($"participants/{participantID}/activity"); }

        public async Task<IEnumerable<DonorDriveDonation>> GetParticipantDonations(string participantID) { return await this.GetAsync<IEnumerable<DonorDriveDonation>>($"participants/{participantID}/donations"); }

        public async Task<IEnumerable<DonorDriveDonor>> GetParticipantDonors(string participantID) { return await this.GetAsync<IEnumerable<DonorDriveDonor>>($"participants/{participantID}/donors"); }

        public async Task<IEnumerable<DonorDriveIncentive>> GetParticipantIncentives(string participantID) { return await this.GetAsync<IEnumerable<DonorDriveIncentive>>($"participants/{participantID}/incentives"); }

        public async Task<IEnumerable<DonorDriveMilestone>> GetParticipantMilestones(string participantID) { return await this.GetAsync<IEnumerable<DonorDriveMilestone>>($"participants/{participantID}/milestones"); }

        public async Task<DonorDriveTeam> GetTeam(string teamID) { return await this.GetAsync<DonorDriveTeam>($"teams/{teamID}"); }

        public async Task<IEnumerable<DonorDriveActivity>> GetTeamActivity(string teamID) { return await this.GetAsync<IEnumerable<DonorDriveActivity>>($"teams/{teamID}/activity"); }

        public async Task<IEnumerable<DonorDriveDonation>> GetTeamDonations(string teamID) { return await this.GetAsync<IEnumerable<DonorDriveDonation>>($"teams/{teamID}/donations"); }

        public async Task<IEnumerable<DonorDriveDonor>> GetTeamDonors(string teamID) { return await this.GetAsync<IEnumerable<DonorDriveDonor>>($"teams/{teamID}/donors"); }

        public async Task<DonorDriveEvent> GetEvent(string eventID) { return await this.GetAsync<DonorDriveEvent>($"events/{eventID}"); }

        public async Task<IEnumerable<DonorDriveActivity>> GetEventActivity(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveActivity>>($"events/{eventID}/activity"); }

        public async Task<IEnumerable<DonorDriveDonation>> GetEventDonations(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveDonation>>($"events/{eventID}/donations"); }

        public async Task<IEnumerable<DonorDriveDonor>> GetEventDonors(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveDonor>>($"events/{eventID}/donors"); }

        private async Task<T> GetAsync<T>(string path)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient(this.BaseAddress))
            {
                return await client.GetAsync<T>($"{BaseAddress}/api/{path}?version={APIVersion}&limit=20");
            }
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            try
            {
                IEnumerable<DonorDriveDonation> donations = null;
                if (ChannelSession.Settings.DonorDriveIncludeTeamDonations && this.Team != null)
                {
                    donations = await this.GetTeamDonations(this.Team.teamID.ToString());
                }
                else
                {
                    donations = await this.GetParticipantDonations(this.Participant.participantID.ToString());
                }

                if (donations != null)
                {
                    foreach (DonorDriveDonation donation in donations)
                    {
                        if (!donationsProcessed.ContainsKey(donation.donationID))
                        {
                            donationsProcessed[donation.donationID] = donation;
                            if (donation.createdDateUTC > startTime)
                            {
                                UserDonationModel genericDonation = donation.ToGenericDonation();
                                await EventService.ProcessDonationEvent(EventTypeEnum.DonorDriveDonation, genericDonation);

                                if (this.Incentives != null && !string.IsNullOrEmpty(donation.incentiveID))
                                {
                                    DonorDriveIncentive incentive = this.Incentives.ToList().FirstOrDefault(i => string.Equals(i.incentiveID, donation.incentiveID, StringComparison.OrdinalIgnoreCase));
                                    if (incentive != null)
                                    {
                                        CommandParametersModel parameters = new CommandParametersModel(genericDonation.User, genericDonation.Platform, genericDonation.GetSpecialIdentifiers());
                                        parameters.SpecialIdentifiers["donordriveincentivedescription"] = incentive.description;

                                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationIncentive, parameters);
                                    }
                                }

                                if (this.Participant.participantID == donation.participantID)
                                {
                                    this.donationTotal += donation.amount;

                                    if (this.Milestones != null)
                                    {
                                        foreach (DonorDriveMilestone milestone in this.Milestones.ToList())
                                        {
                                            if (milestone.isActive && !milestone.isComplete && milestone.fundraisingGoal <= this.donationTotal && milestone.endDateUTC > DateTime.UtcNow)
                                            {
                                                milestone.isComplete = true;

                                                CommandParametersModel parameters = new CommandParametersModel(genericDonation.User, genericDonation.Platform, genericDonation.GetSpecialIdentifiers());
                                                parameters.SpecialIdentifiers["donordrivemilestonedescription"] = milestone.description;
                                                parameters.SpecialIdentifiers["donordrivemilestoneamountnumber"] = milestone.fundraisingGoal.ToString();
                                                parameters.SpecialIdentifiers["donordrivemilestoneamount"] = milestone.fundraisingGoal.ToCurrencyString();

                                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationMilestone, parameters);
                                            }
                                        }
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
            }
        }
    }
}
