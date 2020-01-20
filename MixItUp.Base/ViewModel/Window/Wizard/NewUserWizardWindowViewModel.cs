using MixItUp.Base.Model.Import.ScorpBot;
using MixItUp.Base.Model.Import.Streamlabs;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Wizard
{
    public class NewUserWizardWindowViewModel : WindowViewModelBase
    {
        public bool WizardComplete { get; private set; }

        public ScorpBotData ScorpBotData { get; private set; }

        public StreamlabsChatBotData StreamlabsChatBotData { get; private set; }

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

            this.NextCommand = this.CreateCommand((parameter) =>
            {
                if (this.IntroPageVisible)
                {
                    this.IntroPageVisible = false;
                    this.StreamerAccountsPageVisible = true;
                    this.CanBack = true;
                }
                else if (this.StreamerAccountsPageVisible)
                {
                    this.StreamerAccountsPageVisible = false;
                }

                return Task.FromResult(0);
            });

            this.BackCommand = this.CreateCommand((parameter) =>
            {
                if (this.StreamerAccountsPageVisible)
                {
                    this.StreamerAccountsPageVisible = false;
                    this.IntroPageVisible = true;
                    this.CanBack = false;
                }

                return Task.FromResult(0);
            });
        }
    }
}