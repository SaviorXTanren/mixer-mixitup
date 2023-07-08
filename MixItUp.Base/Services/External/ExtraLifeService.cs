using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class ExtraLifeTeam
    {
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public bool isInviteOnly { get; set; }
        [DataMember]
        public string captainDisplayName { get; set; }
        [DataMember]
        public string eventName { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public string createdDateUTC { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public double numDonations { get; set; }
    }

    [DataContract]
    public class ExtraLifeTeamParticipant
    {
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public double fundraisingGoal { get; set; }
        [DataMember]
        public string eventName { get; set; }
        [DataMember]
        public string createdDateUTC { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public double participantID { get; set; }
        [DataMember]
        public string teamName { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public bool isTeamCaptain { get; set; }
        [DataMember]
        public double sumPledges { get; set; }
        [DataMember]
        public double numDonations { get; set; }
    }

    [DataContract]
    public class ExtraLifeDonation
    {
        [DataMember]
        public string donationID { get; set; }

        [DataMember]
        public string displayName { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public double? amount { get; set; }

        [DataMember]
        public string createdDateUTC { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreatedDate
        {
            get
            {
                DateTimeOffset datetime = DateTimeOffset.Now;
                if (!string.IsNullOrEmpty(this.createdDateUTC) && DateTimeOffset.TryParse(this.createdDateUTC, out datetime))
                {
                    datetime = datetime.ToCorrectLocalTime();
                }
                return datetime;
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            double amount = 0.0;
            if (this.amount.HasValue)
            {
                amount = this.amount.GetValueOrDefault();
            }

            bool isAnonymous = false;
            string username = this.displayName;
            if (string.IsNullOrEmpty(username))
            {
                username = MixItUp.Base.Resources.Anonymous;
                isAnonymous = true;
            }

            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.ExtraLife,
                IsAnonymous = isAnonymous,

                ID = this.donationID,
                Username = username,
                Message = this.message,

                Amount = Math.Round(amount, 2),

                DateTime = this.CreatedDate,
            };
        }
    }

    public class ExtraLifeService : OAuthExternalServiceBase, IDisposable
    {
        private const string BaseAddress = "https://www.extra-life.org/api/";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private ExtraLifeTeam team;
        private ExtraLifeTeamParticipant participant;

        private DateTimeOffset startTime = DateTimeOffset.Now;
        private Dictionary<string, ExtraLifeDonation> donationsReceived = new Dictionary<string, ExtraLifeDonation>();

        public ExtraLifeService() : base(ExtraLifeService.BaseAddress) { }

        public override string Name { get { return MixItUp.Base.Resources.ExtraLife; } }

        public override bool IsConnected { get { return ChannelSession.Settings.ExtraLifeParticipantID > 0; } }

        public override async Task<Result> Connect()
        {
            if (this.IsConnected)
            {
                return await this.InitializeInternal();
            }
            return new Result(Resources.ExtraLifeTeamNotSet);
        }

        public override Task<Result> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result(false));
        }

        public override Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public async Task<ExtraLifeTeam> GetTeam() { return await this.GetTeam(ChannelSession.Settings.ExtraLifeTeamID); }

        public async Task<ExtraLifeTeam> GetTeam(int teamID)
        {
            try
            {
                return await this.GetAsync<ExtraLifeTeam>(string.Format("teams/{0}", teamID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants() { return await this.GetTeamParticipants(ChannelSession.Settings.ExtraLifeTeamID); }

        public async Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants(int teamID)
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeTeamParticipant>>(string.Format("teams/{0}/participants", teamID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<ExtraLifeTeamParticipant>();
        }

        public async Task<ExtraLifeTeamParticipant> GetParticipant()
        {
            try
            {
                return await this.GetAsync<ExtraLifeTeamParticipant>(string.Format("participants/{0}", ChannelSession.Settings.ExtraLifeParticipantID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<ExtraLifeDonation>> GetParticipantDonations()
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeDonation>>(string.Format("participants/{0}/donations?limit=10", ChannelSession.Settings.ExtraLifeParticipantID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<ExtraLifeDonation>();
        }

        public async Task<IEnumerable<ExtraLifeDonation>> GetTeamDonations()
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeDonation>>(string.Format("teams/{0}/donations?limit=10", ChannelSession.Settings.ExtraLifeTeamID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<ExtraLifeDonation>();
        }

        protected override Task RefreshOAuthToken()
        {
            this.token = new OAuthTokenModel() { expiresIn = int.MaxValue };
            return Task.CompletedTask;
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            if (ChannelSession.Settings.ExtraLifeTeamID > 0)
            {
                this.team = await this.GetTeam();
            }

            this.participant = await this.GetParticipant();

            if (this.participant != null)
            {
                IEnumerable<ExtraLifeDonation> donations = await this.GetDonations();
                if (donations != null)
                {
                    foreach (ExtraLifeDonation donation in donations)
                    {
                        if (!string.IsNullOrEmpty(donation.donationID))
                        {
                            donationsReceived[donation.donationID] = donation;
                        }
                    }
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.TrackServiceTelemetry("ExtraLife");
                return new Result();
            }
            return new Result(Resources.ExtraLifeTeamDataFailed);
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            IEnumerable<ExtraLifeDonation> donations = await this.GetDonations();
            if (donations != null)
            {
                foreach (ExtraLifeDonation elDonation in donations)
                {
                    if (!string.IsNullOrEmpty(elDonation.donationID) && !donationsReceived.ContainsKey(elDonation.donationID) && elDonation.CreatedDate > this.startTime)
                    {
                        donationsReceived[elDonation.donationID] = elDonation;
                        UserDonationModel donation = elDonation.ToGenericDonation();
                        await EventService.ProcessDonationEvent(EventTypeEnum.ExtraLifeDonation, donation);
                    }
                }
            }
        }

        private async Task<IEnumerable<ExtraLifeDonation>> GetDonations()
        {
            if (ChannelSession.Settings.ExtraLifeIncludeTeamDonations && this.team != null)
            {
                return await this.GetTeamDonations();
            }
            else
            {
                return await this.GetParticipantDonations();
            }
        }
    }
}
