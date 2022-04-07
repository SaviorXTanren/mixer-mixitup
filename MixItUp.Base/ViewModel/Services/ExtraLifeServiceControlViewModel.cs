using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public enum ExtraLifeServiceParticipantTypes
    {
        Team,
        Individual
    }

    public class ExtraLifeServiceControlViewModel : ServiceControlViewModelBase
    {
        public IEnumerable<ExtraLifeServiceParticipantTypes> ParticipantTypes { get { return EnumHelper.GetEnumList<ExtraLifeServiceParticipantTypes>(); } }

        public ExtraLifeServiceParticipantTypes SelectedParticipantType
        {
            get { return this.selectedParticipantType; }
            set
            {
                this.selectedParticipantType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsTeam");
                this.NotifyPropertyChanged("IsIndividual");
            }
        }
        private ExtraLifeServiceParticipantTypes selectedParticipantType;

        public bool IsTeam { get { return this.SelectedParticipantType == ExtraLifeServiceParticipantTypes.Team; } }

        public bool IsIndividual { get { return this.SelectedParticipantType == ExtraLifeServiceParticipantTypes.Individual; } }

        public int TeamID
        {
            get { return this.teamId; }
            set
            {
                this.teamId = value;
                this.NotifyPropertyChanged();
            }
        }
        private int teamId;

        public ThreadSafeObservableCollection<ExtraLifeTeamParticipant> Participants { get; set; } = new ThreadSafeObservableCollection<ExtraLifeTeamParticipant>();

        public ExtraLifeTeamParticipant Participant
        {
            get { return this.participant; }
            set
            {
                this.participant = value;
                this.NotifyPropertyChanged();

                if (this.Participant != null)
                {
                    this.ParticipantID = (int)value.participantID;
                }
                else
                {
                    this.ParticipantID = 0;
                }
            }
        }
        private ExtraLifeTeamParticipant participant;

        public bool ParticipantsAvailable { get { return this.Participants.Count > 0; } }

        public bool IncludeTeamDonations
        {
            get { return ChannelSession.Settings.ExtraLifeIncludeTeamDonations; }
            set
            {
                ChannelSession.Settings.ExtraLifeIncludeTeamDonations = value;
                this.NotifyPropertyChanged();
            }
        }

        public int ParticipantID
        {
            get { return this.participantID; }
            set
            {
                this.participantID = value;
                this.NotifyPropertyChanged();
            }
        }
        private int participantID;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }
        public ICommand GetTeamParticipantsCommand { get; set; }

        public override string WikiPageName { get { return "extra-life"; } }

        public ExtraLifeServiceControlViewModel()
            : base(Resources.ExtraLife)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.ExtraLifeTeamID = 0;
                ChannelSession.Settings.ExtraLifeParticipantID = 0;

                if (this.SelectedParticipantType == ExtraLifeServiceParticipantTypes.Team)
                {
                    if (this.TeamID <= 0)
                    {
                        await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidTeamId);
                        return;
                    }

                    if (this.Participant == null)
                    {
                        await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidParticipant);
                        return;
                    }

                    ChannelSession.Settings.ExtraLifeTeamID = this.TeamID;
                    ChannelSession.Settings.ExtraLifeParticipantID = this.ParticipantID;
                }
                else if (this.SelectedParticipantType == ExtraLifeServiceParticipantTypes.Individual)
                {
                    if (this.ParticipantID <= 0)
                    {
                        await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidParticipantId);
                        return;
                    }

                    ChannelSession.Settings.ExtraLifeParticipantID = this.ParticipantID;
                }

                Result result = await ServiceManager.Get<ExtraLifeService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<ExtraLifeService>().Disconnect();

                ChannelSession.Settings.ExtraLifeTeamID = 0;
                ChannelSession.Settings.ExtraLifeParticipantID = 0;

                this.IsConnected = false;
            });

            this.GetTeamParticipantsCommand = this.CreateCommand(async () =>
            {
                await this.GetTeamParticipants();
            });

            this.IsConnected = ServiceManager.Get<ExtraLifeService>().IsConnected;
        }

        protected override async Task OnOpenInternal()
        {
            if (this.IsConnected)
            {
                await this.GetTeamParticipants();
                this.Participant = this.Participants.FirstOrDefault(p => p.participantID == ChannelSession.Settings.ExtraLifeParticipantID);
            }
        }

        private async Task GetTeamParticipants()
        {
            if (this.TeamID <= 0)
            {
                await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidTeamId);
            }
            else
            {
                List<ExtraLifeTeamParticipant> participants = new List<ExtraLifeTeamParticipant>();
                ExtraLifeTeam team = await ServiceManager.Get<ExtraLifeService>().GetTeam(this.TeamID);
                if (team != null)
                {
                    IEnumerable<ExtraLifeTeamParticipant> ps = await ServiceManager.Get<ExtraLifeService>().GetTeamParticipants(this.TeamID);
                    foreach (ExtraLifeTeamParticipant participant in ps.OrderBy(p => p.displayName))
                    {
                        participants.Add(participant);
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.ExtraLifeTeamNotFound);
                }
                this.Participants.ClearAndAddRange(participants);
            }
            this.NotifyPropertyChanged("ParticipantsAvailable");
        }
    }
}
