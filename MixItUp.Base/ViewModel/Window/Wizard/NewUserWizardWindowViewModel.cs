using MixItUp.Base.Model;
using MixItUp.Base.Model.Import.ScorpBot;
using MixItUp.Base.Model.Import.Streamlabs;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Accounts;
using StreamingClient.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Wizard
{
    public class NewUserWizardWindowViewModel : WindowViewModelBase
    {
        public bool WizardComplete { get; private set; }

        public ScorpBotDataModel ScorpBot { get; private set; }

        public StreamlabsChatBotDataModel StreamlabsChatBot { get; private set; }

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

        public StreamingPlatformAccountControlViewModel Mixer { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Mixer);

        #endregion Accounts Page

        #region ScorpBot Page

        public bool ScorpBotPageVisible
        {
            get { return this.scorpBotPageVisible; }
            set
            {
                this.scorpBotPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool scorpBotPageVisible;

        public string ScorpBotDirectory
        {
            get { return this.scorpBotDirectory; }
            set
            {
                this.scorpBotDirectory = value;
                this.NotifyPropertyChanged();
            }
        }
        private string scorpBotDirectory;

        public ICommand ScorpBotDirectoryBrowseCommand { get; set; }

        #endregion ScorpBot Page

        #region Streamlabs Chatbot Page

        public bool StreamlabsChatbotPageVisible
        {
            get { return this.streamlabsChatbotPageVisible; }
            set
            {
                this.streamlabsChatbotPageVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool streamlabsChatbotPageVisible;

        public string StreamlabsChatbotDirectory
        {
            get { return this.streamlabsChatbotDirectory; }
            set
            {
                this.streamlabsChatbotDirectory = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamlabsChatbotDirectory;

        public ICommand StreamlabsChatbotDirectoryBrowseCommand { get; set; }

        #endregion Streamlabs Chatbot Page

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
            this.DiscordCommand = this.CreateCommand((parameter) => { ProcessHelper.LaunchLink("https://discord.gg/geA33sW"); return Task.FromResult(0); });
            this.TwitterCommand = this.CreateCommand((parameter) => { ProcessHelper.LaunchLink("https://twitter.com/MixItUpApp"); return Task.FromResult(0); });
            this.YouTubeCommand = this.CreateCommand((parameter) => { ProcessHelper.LaunchLink("https://www.youtube.com/channel/UCcY0vKI9yqcMTgh8OzSnRSA"); return Task.FromResult(0); });
            this.WikiCommand = this.CreateCommand((parameter) => { ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki"); return Task.FromResult(0); });

            this.Mixer.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Mixer.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };

            this.ScorpBotDirectoryBrowseCommand = this.CreateCommand((parameter) =>
            {
                string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
                if (!string.IsNullOrEmpty(folderPath))
                {
                    this.ScorpBotDirectory = folderPath;
                }
                return Task.FromResult(0);
            });

            this.StreamlabsChatbotDirectoryBrowseCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Excel File|*.xlsx");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.StreamlabsChatbotDirectory = filePath;
                }
                return Task.FromResult(0);
            });

            this.SetBackupLocationCommand = this.CreateCommand((parameter) =>
            {
                string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    this.SettingsBackupLocation = folderPath;
                }

                if (this.SelectedSettingsBackupOption == SettingsBackupRateEnum.None)
                {
                    this.SelectedSettingsBackupOption = SettingsBackupRateEnum.Monthly;
                }

                this.NotifyPropertyChanged("IsBackupLocationSet");

                return Task.FromResult(0);
            });

            this.NextCommand = this.CreateCommand(async (parameter) =>
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
                    if (!this.Mixer.IsUserAccountConnected)
                    {
                        this.StatusMessage = "A Mixer Streamer account must be signed in.";
                        return;
                    }

                    this.StreamerAccountsPageVisible = false;
                    this.ScorpBotPageVisible = true;
                }
                else if (this.ScorpBotPageVisible)
                {
                    if (!string.IsNullOrEmpty(this.ScorpBotDirectory))
                    {
                        this.StatusMessage = "Gathering ScorpBot Data...";
                        this.ScorpBot = await ScorpBotDataModel.GatherScorpBotData(this.ScorpBotDirectory);
                        if (this.ScorpBot == null)
                        {
                            this.StatusMessage = "Failed to import ScorpBot data, please ensure that you have selected the correct directory. If this continues to fail, please contact Mix it Up support for assistance.";
                            return;
                        }
                    }

                    this.ScorpBotPageVisible = false;
                    this.StreamlabsChatbotPageVisible = true;
                }
                else if (this.StreamlabsChatbotPageVisible)
                {
                    if (!string.IsNullOrEmpty(this.StreamlabsChatbotDirectory))
                    {
                        this.StatusMessage = "Gathering Streamlabs ChatBot Data...";
                        this.StreamlabsChatBot = await StreamlabsChatBotDataModel.GatherStreamlabsChatBotSettings(StreamingPlatformTypeEnum.Mixer, this.StreamlabsChatbotDirectory);
                        if (this.StreamlabsChatBot == null)
                        {
                            this.StatusMessage = "Failed to import Streamlabs Chat Bot data, please ensure that you have selected the correct data file & have Microsoft Excel installed. If this continues to fail, please contact Mix it Up support for assistance.";
                            return;
                        }
                    }

                    this.StreamlabsChatbotPageVisible = false;
                    this.CommandActionsPageVisible = true;
                }
                else if (this.CommandActionsPageVisible)
                {
                    this.CommandActionsPageVisible = false;
                    this.FinalPageVisible = true;
                }
                else if (this.FinalPageVisible)
                {
                    if (!await ChannelSession.InitializeSession(modChannelName: null))
                    {
                        await DialogHelper.ShowMessage("Failed to initialize session. If this continues please, visit the Mix It Up Discord for assistance.");
                        return;
                    }

                    if (this.ScorpBot != null)
                    {
                        this.ScorpBot.ImportSettings();
                    }

                    if (this.StreamlabsChatBot != null)
                    {
                        await this.StreamlabsChatBot.ImportSettings();
                    }

                    ChannelSession.Settings.ReRunWizard = false;
                    ChannelSession.Settings.SettingsBackupLocation = this.SettingsBackupLocation;
                    ChannelSession.Settings.SettingsBackupRate = this.SelectedSettingsBackupOption;

                    this.WizardComplete = true;
                    this.WizardCompleteEvent(this, new EventArgs());
                }

                this.StatusMessage = string.Empty;
            });

            this.BackCommand = this.CreateCommand((parameter) =>
            {
                if (this.StreamerAccountsPageVisible)
                {
                    this.StreamerAccountsPageVisible = false;
                    this.IntroPageVisible = true;
                    this.CanBack = false;
                }
                else if (this.ScorpBotPageVisible)
                {
                    this.ScorpBotPageVisible = false;
                    this.StreamerAccountsPageVisible = true;
                }
                else if (this.StreamlabsChatbotPageVisible)
                {
                    this.StreamlabsChatbotPageVisible = false;
                    this.ScorpBotPageVisible = true;
                }
                else if (this.CommandActionsPageVisible)
                {
                    this.CommandActionsPageVisible = false;
                    this.StreamlabsChatbotPageVisible = true;
                }
                else if (this.FinalPageVisible)
                {
                    this.FinalPageVisible = false;
                    this.CommandActionsPageVisible = true;
                }

                this.StatusMessage = string.Empty;
                return Task.FromResult(0);
            });
        }
    }
}