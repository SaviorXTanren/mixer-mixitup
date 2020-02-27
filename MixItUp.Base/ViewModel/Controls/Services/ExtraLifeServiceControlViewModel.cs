using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class ExtraLifeServiceControlViewModel : ServiceControlViewModelBase
    {
        public ObservableCollection<ExtraLifeTeamParticipant> Participants { get; set; } = new ObservableCollection<ExtraLifeTeamParticipant>();

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
                    ChannelSession.Settings.ExtraLifeParticipantID = value.participantID;
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
            : base("Extra Life")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                if (this.ExtraLifeTeamID <= 0)
                {
                    await DialogHelper.ShowMessage("Please enter a valid Extra Life team ID.");
                    return;
                }

                if (this.ExtraLifeParticipant == null)
                {
                    await DialogHelper.ShowMessage("Please select a valid Extra Life participant");
                    return;
                }

                Result result = await ChannelSession.Services.ExtraLife.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.ExtraLife.Disconnect();

                ChannelSession.Settings.ExtraLifeTeamID = 0;
                ChannelSession.Settings.ExtraLifeParticipantID = 0;

                this.IsConnected = false;
            });

            this.GetTeamParticipantsCommand = this.CreateCommand(async (parameter) =>
            {
                await this.GetTeamParticipants();
            });

            this.IsConnected = ChannelSession.Services.ExtraLife.IsConnected;
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
                await DialogHelper.ShowMessage("Please enter a valid Extra Life team ID");
            }
            else
            {
                this.Participants.Clear();

                ExtraLifeTeam team = await ChannelSession.Services.ExtraLife.GetTeam(this.ExtraLifeTeamID);
                if (team != null)
                {
                    IEnumerable<ExtraLifeTeamParticipant> ps = await ChannelSession.Services.ExtraLife.GetTeamParticipants(this.ExtraLifeTeamID);
                    foreach (ExtraLifeTeamParticipant participant in ps.OrderBy(p => p.displayName))
                    {
                        this.Participants.Add(participant);
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage("The Extra Life team ID you entered could not be found");
                }
            }
            this.NotifyPropertyChanged("ParticipantsAvailable");
        }
    }
}
