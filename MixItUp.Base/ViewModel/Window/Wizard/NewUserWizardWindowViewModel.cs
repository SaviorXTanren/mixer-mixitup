using MixItUp.Base.Model.Import.ScorpBot;
using MixItUp.Base.Model.Import.Streamlabs;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Wizard
{
    public class NewUserWizardWindowViewModel : WindowViewModelBase
    {
        public bool WizardComplete { get; private set; }

        public ScorpBotData ScorpBot { get; private set; }

        public StreamlabsChatBotData StreamlabsChatBot { get; private set; }

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

        public string MixerUserAccountAvatar
        {
            get { return this.mixerUserAccountAvatar; }
            set
            {
                this.mixerUserAccountAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private string mixerUserAccountAvatar;
        public string MixerUserAccountUsername
        {
            get { return this.mixerUserAccountUsername; }
            set
            {
                this.mixerUserAccountUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string mixerUserAccountUsername;

        public ICommand MixerUserAccountCommand { get; set; }
        public string MixerUserAccountButtonContent { get { return this.IsMixerUserAccountConnected ? "Log Out" : "Log In"; } }
        public bool IsMixerUserAccountConnected { get { return ChannelSession.MixerStreamerConnection != null; } }

        public string MixerBotAccountAvatar
        {
            get { return this.mixerBotAccountAvatar; }
            set
            {
                this.mixerBotAccountAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private string mixerBotAccountAvatar;
        public string MixerBotAccountUsername
        {
            get { return this.mixerBotAccountUsername; }
            set
            {
                this.mixerBotAccountUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string mixerBotAccountUsername;

        public ICommand MixerBotAccountCommand { get; set; }
        public string MixerBotAccountButtonContent { get { return this.IsMixerBotAccountConnected ? "Log Out" : "Log In"; } }
        public bool IsMixerBotAccountConnected { get { return ChannelSession.MixerBotConnection != null; } }

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

            if (this.IsMixerUserAccountConnected)
            {
                this.MixerUserAccountAvatar = ChannelSession.MixerStreamerUser.avatarUrl;
                this.MixerUserAccountUsername = ChannelSession.MixerStreamerUser.username;
            }
            if (this.IsMixerBotAccountConnected)
            {
                this.MixerBotAccountAvatar = ChannelSession.MixerBotUser.avatarUrl;
                this.MixerBotAccountUsername = ChannelSession.MixerBotUser.username;
            }

            this.MixerUserAccountCommand = this.CreateCommand(async (parameter) =>
            {
                this.StartLoadingOperation();

                if (this.IsMixerUserAccountConnected)
                {
                    //ChannelSession.Disconnect();
                    //this.MixerUserAccountAvatar = null;
                    //this.MixerUserAccountUsername = null;
                }
                else
                {
                    bool result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes);
                    if (result)
                    {
                        this.MixerUserAccountAvatar = ChannelSession.MixerStreamerUser.avatarUrl;
                        this.MixerUserAccountUsername = ChannelSession.MixerStreamerUser.username;
                    }
                    else
                    {
                        this.MixerUserAccountAvatar = null;
                        this.MixerUserAccountUsername = null;
                    }
                }
                this.NotifyPropertyChanged("IsMixerUserAccountConnected");
                this.NotifyPropertyChanged("IsMixerUserAccountNotConnected");
                this.NotifyPropertyChanged("MixerUserAccountButtonContent");
                this.NotifyPropertyChanged("CanConnectMixerBotAccount");

                this.EndLoadingOperation();
            });

            this.MixerBotAccountCommand = this.CreateCommand(async (parameter) =>
            {
                this.StartLoadingOperation();

                if (this.IsMixerBotAccountConnected)
                {
                    await ChannelSession.DisconnectBot();
                    this.MixerBotAccountAvatar = null;
                    this.MixerBotAccountUsername = null;
                }
                else
                {
                    bool result = await ChannelSession.ConnectBot();
                    if (result)
                    {
                        this.MixerBotAccountAvatar = ChannelSession.MixerBotUser.avatarUrl;
                        this.MixerBotAccountUsername = ChannelSession.MixerBotUser.username;
                    }
                    else
                    {
                        this.MixerBotAccountAvatar = null;
                        this.MixerBotAccountUsername = null;
                    }
                }
                this.NotifyPropertyChanged("IsMixerBotAccountConnected");
                this.NotifyPropertyChanged("IsMixerBotAccountNotConnected");
                this.NotifyPropertyChanged("MixerBotAccountButtonContent");

                this.EndLoadingOperation();
            });

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

            this.NextCommand = this.CreateCommand(async (parameter) =>
            {
                this.StartLoadingOperation();

                this.StatusMessage = string.Empty;

                if (this.IntroPageVisible)
                {
                    this.IntroPageVisible = false;
                    this.StreamerAccountsPageVisible = true;
                    this.CanBack = true;
                }
                else if (this.StreamerAccountsPageVisible)
                {
                    if (!this.IsMixerUserAccountConnected)
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
                        this.ScorpBot = await ScorpBotData.GatherScorpBotData(this.ScorpBotDirectory);
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
                        this.StreamlabsChatBot = await StreamlabsChatBotData.GatherStreamlabsChatBotSettings(this.StreamlabsChatbotDirectory);
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
                    this.WizardComplete = true;
                    this.WizardCompleteEvent(this, new EventArgs());
                }

                this.StatusMessage = string.Empty;

                this.EndLoadingOperation();
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

                return Task.FromResult(0);
            });
        }
    }
}