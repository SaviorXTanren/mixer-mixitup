using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Accounts;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Wizard
{
    public class NewUserWizardWindowViewModel : UIViewModelBase
    {
        public bool WizardComplete { get; private set; }

        #region Intro Page

        public bool IntroPageVisible
        {
            get { return this.introPageVisible; }
            set
            {
                this.introPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool introPageVisible = true;

        public ICommand DiscordCommand { get; set; }
        public ICommand TwitterCommand { get; set; }
        public ICommand YouTubeCommand { get; set; }
        public ICommand WikiCommand { get; set; }

        #endregion Intro Page

        #region Accounts Page

        public bool StreamerAccountsPageVisible
        {
            get { return this.streamerAccountsPageVisible; }
            set
            {
                this.streamerAccountsPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool streamerAccountsPageVisible;

        public StreamingPlatformAccountControlViewModel Twitch { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Twitch);

        public StreamingPlatformAccountControlViewModel YouTube { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.YouTube);

        public StreamingPlatformAccountControlViewModel Trovo { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Trovo);

        #endregion Accounts Page

        #region Command & Actions Page

        public bool CommandActionsPageVisible
        {
            get { return this.commandActionsPageVisible; }
            set
            {
                this.commandActionsPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool commandActionsPageVisible;

        #endregion Command & Actions Page

        #region Final Page

        public bool FinalPageVisible
        {
            get { return this.finalPageVisible; }
            set
            {
                this.finalPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool finalPageVisible;

        public ICommand SetBackupLocationCommand { get; private set; }

        public string SettingsBackupLocation
        {
            get { return this.settingsBackupLocation; }
            set
            {
                this.settingsBackupLocation = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsBackupLocationSet");
            }
        }
        private string settingsBackupLocation;
        public bool IsBackupLocationSet { get { return !string.IsNullOrEmpty(this.SettingsBackupLocation); } }

        public ObservableCollection<SettingsBackupRateEnum> SettingsBackupOptions { get; private set; } = new ObservableCollection<SettingsBackupRateEnum>(EnumHelper.GetEnumList<SettingsBackupRateEnum>());
        public SettingsBackupRateEnum SelectedSettingsBackupOption
        {
            get { return this.selectedSettingsBackupOption; }
            set
            {
                this.selectedSettingsBackupOption = value;
                this.NotifyPropertyChanged();
            }
        }
        private SettingsBackupRateEnum selectedSettingsBackupOption;

        public event EventHandler WizardCompleteEvent = delegate { };

        #endregion Final Page

        public string StatusMessage
        {
            get { return this.statusMessage; }
            set
            {
                this.statusMessage = value;
                this.NotifyPropertyChanged();
            }
        }
        private string statusMessage;
        public bool CanNext
        {
            get { return this.canNext; }
            set
            {
                this.canNext = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool canNext = true;
        public ICommand NextCommand { get; set; }
        public bool CanBack
        {
            get { return this.canBack; }
            set
            {
                this.canBack = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool canBack = false;
        public ICommand BackCommand { get; set; }

        public NewUserWizardWindowViewModel()
        {
            this.DiscordCommand = this.CreateCommand(() => { ServiceManager.Get<IProcessService>().LaunchLink("https://mixitupapp.com/discord"); });
            this.TwitterCommand = this.CreateCommand(() => { ServiceManager.Get<IProcessService>().LaunchLink("https://twitter.com/MixItUpApp"); });
            this.YouTubeCommand = this.CreateCommand(() => { ServiceManager.Get<IProcessService>().LaunchLink("https://www.youtube.com/c/MixItUpApp"); });
            this.WikiCommand = this.CreateCommand(() => { ServiceManager.Get<IProcessService>().LaunchLink("https://wiki.mixitupapp.com/"); });

            this.Twitch.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Twitch.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.YouTube.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.YouTube.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.Trovo.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Trovo.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };

            this.SetBackupLocationCommand = this.CreateCommand(() =>
            {
                string folderPath = ServiceManager.Get<IFileService>().ShowOpenFolderDialog();
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    this.SettingsBackupLocation = folderPath;
                }

                if (this.SelectedSettingsBackupOption == SettingsBackupRateEnum.None)
                {
                    this.SelectedSettingsBackupOption = SettingsBackupRateEnum.Monthly;
                }

                this.NotifyPropertyChanged("IsBackupLocationSet");
            });

            this.NextCommand = this.CreateCommand(async () =>
            {
                this.StatusMessage = string.Empty;

                if (this.IntroPageVisible)
                {
                    this.IntroPageVisible = false;
                    this.StreamerAccountsPageVisible = true;
                    this.CanBack = true;
                }
                else if (this.StreamerAccountsPageVisible)
                {
                    if (!this.Twitch.IsStreamerAccountConnected && !this.YouTube.IsStreamerAccountConnected && !this.Trovo.IsStreamerAccountConnected)
                    {
                        this.StatusMessage = MixItUp.Base.Resources.NewUserWizardAtLeastOneAccountMustBeSignedIn;
                        return;
                    }

                    this.StreamerAccountsPageVisible = false;
                    this.CommandActionsPageVisible = true;
                }
                else if (this.CommandActionsPageVisible)
                {
                    this.CommandActionsPageVisible = false;
                    this.FinalPageVisible = true;
                }
                else if (this.FinalPageVisible)
                {
                    Result result = await ChannelSession.InitializeSession();
                    if (!result.Success)
                    {
                        await DialogHelper.ShowMessage(result.Message);
                        return;
                    }

                    ChannelSession.Settings.ReRunWizard = false;
                    ChannelSession.Settings.SettingsBackupLocation = this.SettingsBackupLocation;
                    ChannelSession.Settings.SettingsBackupRate = this.SelectedSettingsBackupOption;

                    this.WizardComplete = true;
                    this.WizardCompleteEvent(this, new EventArgs());
                }

                this.StatusMessage = string.Empty;
            });

            this.BackCommand = this.CreateCommand(() =>
            {
                if (this.StreamerAccountsPageVisible)
                {
                    this.StreamerAccountsPageVisible = false;
                    this.IntroPageVisible = true;
                    this.CanBack = false;
                }
                else if (this.CommandActionsPageVisible)
                {
                    this.CommandActionsPageVisible = false;
                    this.StreamerAccountsPageVisible = true;
                }
                else if (this.FinalPageVisible)
                {
                    this.FinalPageVisible = false;
                    this.CommandActionsPageVisible = true;
                }

                this.StatusMessage = string.Empty;
            });
        }

        protected override async Task OnOpenInternal()
        {
            if (ChannelSession.Settings == null)
            {
                await ChannelSession.Connect(new SettingsV3Model());
            }
            await base.OnOpenInternal();
        }
    }
}