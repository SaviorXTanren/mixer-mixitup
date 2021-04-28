using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class ExtraLifeServiceControlViewModel : ServiceControlViewModelBase
    {
        public ThreadSafeObservableCollection<ExtraLifeTeamParticipant> Participants { get; set; } = new ThreadSafeObservableCollection<ExtraLifeTeamParticipant>();

        public int ExtraLifeTeamID
        {
            get { return ChannelSession.Settings.ExtraLifeTeamID; }
            set
            {
                ChannelSession.Settings.ExtraLifeTeamID = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool ParticipantsAvailable { get { return this.Participants.Count > 0; } }

        public ExtraLifeTeamParticipant ExtraLifeParticipant
        {
            get { return this.extraLifeParticipant; }
            set
            {
                this.extraLifeParticipant = value;
                this.NotifyPropertyChanged();

                if (this.ExtraLifeParticipant != null)
                {
                    ChannelSession.Settings.ExtraLifeParticipantID = (int)value.participantID;
                }
                else
                {
                    ChannelSession.Settings.ExtraLifeParticipantID = 0;
                }
            }
        }
        private ExtraLifeTeamParticipant extraLifeParticipant;

        public bool ExtraLifeIncludeTeamDonations
        {
            get { return ChannelSession.Settings.ExtraLifeIncludeTeamDonations; }
            set
            {
                ChannelSession.Settings.ExtraLifeIncludeTeamDonations = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }
        public ICommand GetTeamParticipantsCommand { get; set; }

        public ExtraLifeServiceControlViewModel()
            : base(Resources.ExtraLife)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (this.ExtraLifeTeamID <= 0)
                {
                    await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidTeamId);
                    return;
                }

                if (this.ExtraLifeParticipant == null)
                {
                    await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidParticipant);
                    return;
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

        protected override async Task OnLoadedInternal()
        {
            if (this.IsConnected)
            {
                await this.GetTeamParticipants();
                this.ExtraLifeParticipant = this.Participants.FirstOrDefault(p => p.participantID == ChannelSession.Settings.ExtraLifeParticipantID);
            }
        }

        private async Task GetTeamParticipants()
        {
            if (this.ExtraLifeTeamID <= 0)
            {
                await DialogHelper.ShowMessage(Resources.ExtraLifeInvalidTeamId);
            }
            else
            {
                List<ExtraLifeTeamParticipant> participants = new List<ExtraLifeTeamParticipant>();
                ExtraLifeTeam team = await ServiceManager.Get<ExtraLifeService>().GetTeam(this.ExtraLifeTeamID);
                if (team != null)
                {
                    IEnumerable<ExtraLifeTeamParticipant> ps = await ServiceManager.Get<ExtraLifeService>().GetTeamParticipants(this.ExtraLifeTeamID);
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
