using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
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
    public class DonorDriveCharity
    {
        [DataMember]
        public string charityName { get; set; }
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public bool isDemo { get; set; }
        [DataMember]
        public string programID { get; set; }
        [DataMember]
        public string programImageURL { get; set; }
        [DataMember]
        public string programName { get; set; }
        [DataMember]
        public string programURL { get; set; }
        [DataMember]
        public bool streamingIsEnabled { get; set; }
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

    /// <summary>
    /// https://github.com/DonorDrive/PublicAPI/tree/master
    /// </summary>
    public class DonorDriveService : IExternalService
    {
        private const string APIVersion = "1.3";

        public static readonly DonorDriveCharity CustomCharity = new DonorDriveCharity()
        {
            displayName = Resources.Custom,
            programURL = string.Empty
        };

        public string Name { get { return Resources.DonorDrive; }}

        public bool IsConnected { get; private set; }

        public string BaseAddress { get; protected set; }

        public DonorDriveParticipant Participant { get; private set; }
        public DonorDriveTeam Team { get; private set; }
        public DonorDriveEvent Event { get; private set; }
        public IEnumerable<DonorDriveIncentive> Incentives { get; private set; }
        public IEnumerable<DonorDriveIncentive> TeamIncentives { get; private set; }

        private CancellationTokenSource cancellationTokenSource;

        private DateTime startTime;
        private Dictionary<string, DonorDriveDonation> donationsProcessed = new Dictionary<string, DonorDriveDonation>();
        private Dictionary<string, DonorDriveMilestone> milestonesNotCompleted = new Dictionary<string, DonorDriveMilestone>();
        private Dictionary<string, DonorDriveMilestone> teamMilestonesNotCompleted = new Dictionary<string, DonorDriveMilestone>();

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

                        IEnumerable<DonorDriveMilestone> milestones = await this.GetParticipantMilestones(this.Participant.participantID.ToString());
                        if (milestones == null)
                        {
                            return new Result(Resources.DonorDriveFailedToGetData);
                        }

                        foreach (DonorDriveMilestone milestone in milestones.Where(m => m.isActive && !m.isComplete))
                        {
                            milestonesNotCompleted[milestone.milestoneID] = milestone;
                        }
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

                    if (this.Team != null && ChannelSession.Settings.DonorDriveIncludeTeamDonations)
                    {
                        await Task.Delay(1000);

                        this.TeamIncentives = await this.GetTeamIncentives(this.Team.teamID.ToString());
                    }

                    this.startTime = DateTime.UtcNow;
                    this.lastRefresh = DateTimeOffset.Now;

                    this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    ServiceManager.Get<ITelemetryService>().TrackService("DonorDrive");

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
            this.milestonesNotCompleted.Clear();

            this.IsConnected = false;

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<DonorDriveCharity>> GetCharities()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    List<DonorDriveCharity> charities = await client.GetAsync<List<DonorDriveCharity>>("https://api.donordrive.com/programs?orderBy=displayName%20ASC");
                    if (charities != null)
                    {
                        charities.Add(DonorDriveService.CustomCharity);

                        if (ChannelSession.IsDebug())
                        {
                            charities.Add(new DonorDriveCharity()
                            {
                                displayName = "Integration",
                                programURL = "https://integrations.donordrive.com"
                            });
                        }

                        return charities.Where(c => !c.isDemo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task RefreshData()
        {
            try
            {
                if ((DateTimeOffset.Now - this.lastRefresh).TotalMinutes >= 10)
                {
                    DonorDriveParticipant participant = await this.GetParticipant(this.Participant.participantID.ToString());
                    if (participant != null)
                    {
                        this.Participant = participant;
                    }

                    await Task.Delay(500);

                    DonorDriveEvent ddEvent = await this.GetEvent(this.Event.eventID.ToString());
                    if (ddEvent != null)
                    {
                        this.Event = ddEvent;
                    }

                    if (this.Team != null)
                    {
                        await Task.Delay(500);

                        DonorDriveTeam team = await this.GetTeam(this.Team.teamID.ToString());
                        if (team != null)
                        {
                            this.Team = team;
                        }
                    }

                    if (this.Participant.numIncentives > 0)
                    {
                        await Task.Delay(500);

                        IEnumerable<DonorDriveIncentive> incentives = await this.GetParticipantIncentives(this.Participant.participantID.ToString());
                        if (incentives != null)
                        {
                            this.Incentives = incentives;
                        }
                    }

                    if (this.Team != null && ChannelSession.Settings.DonorDriveIncludeTeamDonations)
                    {
                        await Task.Delay(500);

                        this.TeamIncentives = await this.GetTeamIncentives(this.Team.teamID.ToString());
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

        public async Task<IEnumerable<DonorDriveIncentive>> GetTeamIncentives(string teamID)
        {
            try
            {
                return await this.GetAsync<IEnumerable<DonorDriveIncentive>>($"teams/{teamID}/incentives");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<IEnumerable<DonorDriveMilestone>> GetTeamMilestones(string teamID)
        {
            try
            {
                return await this.GetAsync<IEnumerable<DonorDriveMilestone>>($"teams/{teamID}/milestones");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<DonorDriveEvent> GetEvent(string eventID) { return await this.GetAsync<DonorDriveEvent>($"events/{eventID}"); }

        public async Task<IEnumerable<DonorDriveActivity>> GetEventActivity(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveActivity>>($"events/{eventID}/activity"); }

        public async Task<IEnumerable<DonorDriveDonation>> GetEventDonations(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveDonation>>($"events/{eventID}/donations"); }

        public async Task<IEnumerable<DonorDriveDonor>> GetEventDonors(string eventID) { return await this.GetAsync<IEnumerable<DonorDriveDonor>>($"events/{eventID}/donors"); }

        private async Task<T> GetAsync<T>(string path)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient(this.BaseAddress))
            {
                return await client.GetAsync<T>($"{BaseAddress}api/{path}?version={APIVersion}&limit=20");
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
                    bool newDonations = false;
                    foreach (DonorDriveDonation donation in donations)
                    {
                        if (!donationsProcessed.ContainsKey(donation.donationID))
                        {
                            newDonations = true;
                            donationsProcessed[donation.donationID] = donation;
                            if (donation.createdDateUTC > startTime)
                            {
                                UserDonationModel genericDonation = donation.ToGenericDonation();

                                Dictionary<string, string> donationSpecialIdentifiers = new Dictionary<string, string>();
                                donationSpecialIdentifiers["donordriverecipientname"] = donation.recipientName;
                                donationSpecialIdentifiers["donordriverecipientimageurl"] = donation.recipientImageURL;

                                await EventService.ProcessDonationEvent(EventTypeEnum.DonorDriveDonation, genericDonation, additionalSpecialIdentifiers: donationSpecialIdentifiers);

                                await this.RefreshData();

                                if (!string.IsNullOrEmpty(donation.incentiveID))
                                {
                                    if (this.Incentives != null)
                                    {
                                        DonorDriveIncentive incentive = this.Incentives.ToList().FirstOrDefault(i => string.Equals(i.incentiveID, donation.incentiveID, StringComparison.OrdinalIgnoreCase));
                                        if (incentive != null)
                                        {
                                            CommandParametersModel parameters = new CommandParametersModel(genericDonation.User, genericDonation.Platform, genericDonation.GetSpecialIdentifiers());
                                            parameters.SpecialIdentifiers["donordriveincentivedescription"] = incentive.description;

                                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationIncentive, parameters);
                                        }
                                    }

                                    if (ChannelSession.Settings.DonorDriveIncludeTeamDonations && this.Team != null && this.TeamIncentives != null)
                                    {
                                        DonorDriveIncentive incentive = this.TeamIncentives.ToList().FirstOrDefault(i => string.Equals(i.incentiveID, donation.incentiveID, StringComparison.OrdinalIgnoreCase));
                                        if (incentive != null)
                                        {
                                            CommandParametersModel parameters = new CommandParametersModel(genericDonation.User, genericDonation.Platform, genericDonation.GetSpecialIdentifiers());
                                            parameters.SpecialIdentifiers["donordriveincentivedescription"] = incentive.description;

                                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationTeamIncentive, parameters);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (newDonations)
                    {
                        if (this.Participant.numMilestones > 0)
                        {
                            IEnumerable<DonorDriveMilestone> milestones = await this.GetParticipantMilestones(this.Participant.participantID.ToString());
                            if (milestones != null)
                            {
                                foreach (DonorDriveMilestone milestone in milestones.Where(m => m.isActive && m.endDateUTC > DateTime.UtcNow))
                                {
                                    if (!milestone.isComplete)
                                    {
                                        milestonesNotCompleted[milestone.milestoneID] = milestone;
                                    }
                                    else if (milestonesNotCompleted.ContainsKey(milestone.milestoneID))
                                    {
                                        milestonesNotCompleted.Remove(milestone.milestoneID);

                                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                        specialIdentifiers["donordrivemilestonedescription"] = milestone.description;
                                        specialIdentifiers["donordrivemilestoneamountnumber"] = milestone.fundraisingGoal.ToString();
                                        specialIdentifiers["donordrivemilestoneamount"] = CurrencyHelper.ToCurrencyString(milestone.fundraisingGoal);
                                        CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.None, specialIdentifiers);

                                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationMilestone, parameters);
                                    }
                                }
                            }
                        }

                        if (ChannelSession.Settings.DonorDriveIncludeTeamDonations && this.Team != null)
                        {
                            IEnumerable<DonorDriveMilestone> milestones = await this.GetTeamMilestones(this.Team.teamID.ToString());
                            if (milestones != null)
                            {
                                foreach (DonorDriveMilestone milestone in milestones.Where(m => m.isActive && m.endDateUTC > DateTime.UtcNow))
                                {
                                    if (!milestone.isComplete)
                                    {
                                        teamMilestonesNotCompleted[milestone.milestoneID] = milestone;
                                    }
                                    else if (teamMilestonesNotCompleted.ContainsKey(milestone.milestoneID))
                                    {
                                        teamMilestonesNotCompleted.Remove(milestone.milestoneID);

                                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                        specialIdentifiers["donordrivemilestonedescription"] = milestone.description;
                                        specialIdentifiers["donordrivemilestoneamountnumber"] = milestone.fundraisingGoal.ToString();
                                        specialIdentifiers["donordrivemilestoneamount"] = CurrencyHelper.ToCurrencyString(milestone.fundraisingGoal);
                                        CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.None, specialIdentifiers);

                                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.DonorDriveDonationTeamMilestone, parameters);
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
