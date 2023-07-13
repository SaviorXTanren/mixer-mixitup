using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class DonorDriveServiceControlViewModel : ServiceControlViewModelBase
    {
        private const string ParticipantURLFragment = "/participant/";

        public ObservableCollection<DonorDriveCharity> Charities { get; private set; } = new ObservableCollection<DonorDriveCharity>();

        public DonorDriveCharity SelectedCharity
        {
            get { return this.selectedCharity; }
            set
            {
                this.selectedCharity = value;
                this.NotifyPropertyChanged();
            }
        }
        private DonorDriveCharity selectedCharity;

        public string ParticipantID
        {
            get { return this.participantID; }
            set
            {
                this.participantID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string participantID;

        public bool IncludeTeamDonations
        {
            get { return ChannelSession.Settings.DonorDriveIncludeTeamDonations; }
            set
            {
                ChannelSession.Settings.DonorDriveIncludeTeamDonations = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsPartOfTeam { get { return ServiceManager.Get<DonorDriveService>().IsConnected && ServiceManager.Get<DonorDriveService>().Team != null; } }

        public string TeamName
        {
            get
            {
                if (ServiceManager.Get<DonorDriveService>().IsConnected && ServiceManager.Get<DonorDriveService>().Team != null)
                {
                    return $"{Resources.TeamName}: {ServiceManager.Get<DonorDriveService>().Team.name}";
                }
                return string.Empty;
            }
        }

        public string EventName
        {
            get
            {
                if (ServiceManager.Get<DonorDriveService>().IsConnected && ServiceManager.Get<DonorDriveService>().Event != null)
                {
                    return $"{Resources.EventName}: {ServiceManager.Get<DonorDriveService>().Event.name}";
                }
                return string.Empty;
            }
        }

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "donor-drive"; } }

        public DonorDriveServiceControlViewModel()
            : base(Resources.DonorDrive)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.DonorDriveCharityURL = null;
                ChannelSession.Settings.DonorDriveParticipantID = null;

                if (this.SelectedCharity == null)
                {
                    await DialogHelper.ShowMessage(Resources.DonorDriveMissingCharity);
                }
                ChannelSession.Settings.DonorDriveCharityURL = this.SelectedCharity.programURL;

                if (!string.IsNullOrWhiteSpace(this.ParticipantID))
                {
                    string[] splits = this.ParticipantID.Split(new string[] { ParticipantURLFragment }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length == 1)
                    {
                        ChannelSession.Settings.DonorDriveParticipantID = splits[0];
                    }
                    else if (splits.Length == 2)
                    {
                        ChannelSession.Settings.DonorDriveParticipantID = splits[1];
                        if (this.SelectedCharity == DonorDriveService.CustomCharity)
                        {
                            ChannelSession.Settings.DonorDriveCharityURL = splits[0];
                        }
                    }
                }

                if (ChannelSession.Settings.DonorDriveParticipantID == null)
                {
                    await DialogHelper.ShowMessage(Resources.DonorDriveMissingInvalidParticipantID);
                    return;
                }

                Result result = await ServiceManager.Get<DonorDriveService>().Connect();
                if (result.Success)
                {
                    this.NotifyPropertyChanged(nameof(this.IsPartOfTeam));
                    this.NotifyPropertyChanged(nameof(this.TeamName));
                    this.NotifyPropertyChanged(nameof(this.EventName));

                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<DonorDriveService>().Disconnect();

                ChannelSession.Settings.DonorDriveCharityURL = null;
                ChannelSession.Settings.DonorDriveParticipantID = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<DonorDriveService>().IsConnected;
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            IEnumerable<DonorDriveCharity> charities = await ServiceManager.Get<DonorDriveService>().GetCharities();
            if (charities != null)
            {
                this.Charities.AddRange(charities);
                this.SelectedCharity = charities.FirstOrDefault();
            }
        }
    }
}
