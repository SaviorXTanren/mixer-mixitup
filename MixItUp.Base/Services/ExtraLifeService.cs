using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
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
        public int eventID { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public int teamID { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int numDonations { get; set; }
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
        public int eventID { get; set; }
        [DataMember]
        public double sumDonations { get; set; }
        [DataMember]
        public int participantID { get; set; }
        [DataMember]
        public string teamName { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public int teamID { get; set; }
        [DataMember]
        public bool isTeamCaptain { get; set; }
        [DataMember]
        public double sumPledges { get; set; }
        [DataMember]
        public int numDonations { get; set; }
    }

    [DataContract]
    public class ExtraLifeDonation
    {
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public bool isFulfilled { get; set; }
        [DataMember]
        public int? participantID { get; set; }
        [DataMember]
        public double? amount { get; set; }
        [DataMember]
        public string donorID { get; set; }
        [DataMember]
        public string avatarImageURL { get; set; }
        [DataMember]
        public string createdDateUTC { get; set; }
        [DataMember]
        public int teamID { get; set; }
        [DataMember]
        public string thankYouSent { get; set; }

        public UserDonationModel ToGenericDonation()
        {
            double amount = 0.0;
            if (this.amount.HasValue)
            {
                amount = this.amount.GetValueOrDefault();
            }

            DateTimeOffset datetime = DateTimeOffset.Now;
            if (!string.IsNullOrEmpty(this.createdDateUTC) && DateTimeOffset.TryParse(this.createdDateUTC, out datetime))
            {
                datetime = datetime.ToLocalTime();
            }

            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.ExtraLife,

                ID = this.donorID,
                UserName = this.displayName,
                Message = this.message,
                ImageLink = this.avatarImageURL,

                Amount = Math.Round(amount, 2),

                DateTime = datetime,
            };
        }
    }

    public interface IExtraLifeService
    {
        bool IsConnected();

        Task<bool> Connect(int teamID, int participantID, bool includeTeamDonations);
        Task Disconnect();

        Task<ExtraLifeTeam> GetTeam();
        Task<ExtraLifeTeam> GetTeam(int teamID);
        Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants();
        Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants(int teamID);
        Task<ExtraLifeTeamParticipant> GetParticipant();

        Task<IEnumerable<ExtraLifeDonation>> GetParticipantDonations();
        Task<IEnumerable<ExtraLifeDonation>> GetTeamDonations();
    }

    public class ExtraLifeService : OAuthServiceBase, IExtraLifeService, IDisposable
    {
        private const string BaseAddress = "https://www.extra-life.org/api/";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private int teamID;
        private int participantID;
        private bool includeTeamDonations;

        private ExtraLifeTeam team;
        private ExtraLifeTeamParticipant participant;

        public ExtraLifeService() : base(ExtraLifeService.BaseAddress) { this.token = new OAuthTokenModel() { expiresIn = int.MaxValue }; }

        public bool IsConnected() { return this.team != null && this.participant != null; }

        public async Task<bool> Connect(int teamID, int participantID, bool includeTeamDonations)
        {
            this.teamID = teamID;
            this.participantID = participantID;
            this.includeTeamDonations = includeTeamDonations;

            return await this.InitializeInternal();
        }

        public Task Disconnect()
        {
            this.teamID = 0;
            this.participantID = 0;
            this.includeTeamDonations = false;

            this.team = null;
            this.participant = null;

            this.cancellationTokenSource.Cancel();

            return Task.FromResult(0);
        }

        public async Task<ExtraLifeTeam> GetTeam() { return await this.GetTeam(this.teamID); }

        public async Task<ExtraLifeTeam> GetTeam(int teamID)
        {
            try
            {
                return await this.GetAsync<ExtraLifeTeam>(string.Format("teams/{0}", teamID));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants() { return await this.GetTeamParticipants(this.teamID); }

        public async Task<IEnumerable<ExtraLifeTeamParticipant>> GetTeamParticipants(int teamID)
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeTeamParticipant>>(string.Format("teams/{0}/participants", teamID));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return new List<ExtraLifeTeamParticipant>();
        }

        public async Task<ExtraLifeTeamParticipant> GetParticipant()
        {
            try
            {
                return await this.GetAsync<ExtraLifeTeamParticipant>(string.Format("participants/{0}", participantID));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<ExtraLifeDonation>> GetParticipantDonations()
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeDonation>>(string.Format("participants/{0}/donations?limit=10", participantID));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return new List<ExtraLifeDonation>();
        }

        public async Task<IEnumerable<ExtraLifeDonation>> GetTeamDonations()
        {
            try
            {
                return await this.GetAsync<IEnumerable<ExtraLifeDonation>>(string.Format("teams/{0}/donations?limit=10", teamID));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return new List<ExtraLifeDonation>();
        }

        protected override Task RefreshOAuthToken()
        {
            this.token = new OAuthTokenModel() { expiresIn = int.MaxValue };
            return Task.FromResult(0);
        }

        private async Task<bool> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.team = await this.GetTeam();
            this.participant = await this.GetParticipant();

            if (this.team != null && this.participant != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return true;
            }
            return false;
        }

        private async Task BackgroundDonationCheck()
        {
            Dictionary<string, ExtraLifeDonation> donationsReceived = new Dictionary<string, ExtraLifeDonation>();

            IEnumerable<ExtraLifeDonation> donations = (this.includeTeamDonations) ? await this.GetTeamDonations() : await this.GetParticipantDonations();
            foreach (ExtraLifeDonation donation in donations)
            {
                if (!string.IsNullOrEmpty(donation.donorID))
                {
                    donationsReceived[donation.donorID] = donation;
                }
            }

            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    donations = (this.includeTeamDonations) ? await this.GetTeamDonations() : await this.GetParticipantDonations();
                    foreach (ExtraLifeDonation elDonation in donations)
                    {
                        if (!string.IsNullOrEmpty(elDonation.donorID) && !donationsReceived.ContainsKey(elDonation.donorID))
                        {
                            donationsReceived[elDonation.donorID] = elDonation;

                            UserDonationModel donation = elDonation.ToGenericDonation();
                            GlobalEvents.DonationOccurred(donation);

                            UserViewModel user = new UserViewModel(0, donation.UserName);

                            UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(user.UserName);
                            if (userModel != null)
                            {
                                user = new UserViewModel(userModel);
                            }

                            EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.ExtraLifeDonation));
                            if (command != null)
                            {
                                await command.Perform(user, arguments: null, extraSpecialIdentifiers: donation.GetSpecialIdentifiers());
                            }
                        }
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

                await Task.Delay(20000);
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
