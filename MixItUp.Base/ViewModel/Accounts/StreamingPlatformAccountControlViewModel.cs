using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Accounts
{
    public class StreamingPlatformAccountControlViewModel : UIViewModelBase
    {
        public StreamingPlatformTypeEnum Platform
        {
            get { return this.platform; }
            private set
            {
                this.platform = value;
                this.NotifyPropertyChanged();
                this.NotifyAllProperties();
            }
        }
        private StreamingPlatformTypeEnum platform;

        public string PlatformName { get { return EnumLocalizationHelper.GetLocalizedName(this.Platform); } }

        public string PlatformImage { get { return StreamingPlatforms.GetPlatformImage(this.Platform); } }

        public string ButtonColor
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "#9146FF"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#19D66B"; }
                return "#3f51b5";
            }
        }
        public string ButtonImage
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/TwitchMonochrome.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/YouTube.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/TrovoMonochrome.png"; }
                return StreamingPlatforms.GetPlatformImage(this.Platform);
            }
        }
        public string ButtonTextForeground
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#000000"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#FFFFFF"; }
                return "#000000";
            }
        }

        public string ButtonLoginText
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogInWithTwitch; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogInWithYouTube; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogInWithTrovo; }
                return string.Empty;
            }
        }

        public string ButtonLogoutText
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogOutOfTwitch; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogOutOfYouTube; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogOutOfTrovo; }
                return string.Empty;
            }
        }

        public bool IsStreamerAccountEnabled { get { return this.session.IsEnabled; } }
        public bool IsStreamerAccountConnected { get { return this.session.IsConnected; } }

        public string StreamerAccountUsername
        {
            get { return this.streamerAccountUsername; }
            set
            {
                this.streamerAccountUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamerAccountUsername;
        public string StreamerAccountAvatar
        {
            get { return this.streamerAccountAvatar; }
            set
            {
                this.streamerAccountAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamerAccountAvatar;

        public bool IsBotAccountEnabled { get { return this.session.IsBotEnabled; } }
        public bool IsBotAccountConnected { get { return this.session.IsBotConnected; } }

        public string BotAccountUsername
        {
            get { return this.botAccountUsername; }
            set
            {
                this.botAccountUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string botAccountUsername;
        public string BotAccountAvatar
        {
            get { return this.botAccountAvatar; }
            set
            {
                this.botAccountAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private string botAccountAvatar;

        public bool IsStreamerAccountLogInVisible { get { return !this.IsStreamerAccountEnabled && !this.IsStreamerAccountConnected && this.streamerConnectTask == null; } }
        public ICommand StreamerAccountLogInCommand { get; set; }
        public bool IsStreamerAccountCancelVisible { get { return !this.IsStreamerAccountEnabled && !this.IsStreamerAccountConnected && this.streamerConnectTask != null; } }
        public ICommand StreamerAccountCancelCommand { get; set; }
        public bool IsStreamerAccountLogoutVisible { get { return this.IsStreamerAccountEnabled || this.IsStreamerAccountConnected; } }
        public ICommand StreamerAccountLogOutCommand { get; set; }

        public bool IsBotAccountLogInVisible { get { return !this.IsBotAccountEnabled && !this.IsBotAccountConnected && this.botConnectTask == null; } }
        public ICommand BotAccountLogInCommand { get; set; }
        public bool IsBotAccountCancelVisible { get { return !this.IsBotAccountEnabled && !this.IsBotAccountConnected && this.botConnectTask != null; } }
        public ICommand BotAccountCancelCommand { get; set; }
        public bool IsBotAccountLogoutVisible { get { return this.IsBotAccountEnabled || this.IsBotAccountConnected; } }
        public ICommand BotAccountLogOutCommand { get; set; }

        private StreamingPlatformSessionBase session;

        private Task streamerConnectTask = null;
        private CancellationTokenSource streamerConnectCancellationTokenSource = new CancellationTokenSource();

        private Task botConnectTask = null;
        private CancellationTokenSource botConnectCancellationTokenSource = new CancellationTokenSource();

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            this.session = StreamingPlatforms.GetPlatformSession(this.Platform);
            if (this.session.IsEnabled)
            {
                if (this.session.IsConnected)
                {
                    this.StreamerAccountUsername = this.session.StreamerUsername;
                    this.StreamerAccountAvatar = this.session.StreamerAvatarURL;
                }
                else
                {
                    this.StreamerAccountUsername = Resources.Unknown;
                }
            }

            if (this.session.IsBotEnabled)
            {
                if (this.session.IsBotConnected)
                {
                    this.BotAccountUsername = this.session.BotUsername;
                    this.BotAccountAvatar = this.session.BotAvatarURL;
                }
                else
                {
                    this.BotAccountUsername = Resources.Unknown;
                }
            }

            this.StreamerAccountLogInCommand = this.CreateCommand(() =>
            {
                try
                {
                    this.streamerConnectCancellationTokenSource.Cancel();
                    this.streamerConnectCancellationTokenSource = new CancellationTokenSource();

                    this.streamerConnectTask = AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        try
                        {
                            Result result = await this.session.ManualConnectStreamer(this.streamerConnectCancellationTokenSource.Token);
                            if (result.Success && !this.streamerConnectCancellationTokenSource.IsCancellationRequested)
                            {
                                if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.None)
                                {
                                    ChannelSession.Settings.DefaultStreamingPlatform = this.Platform;
                                }

                                if (string.Equals(this.session.StreamerID, this.session.BotID, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    await this.session.DisableBot();
                                }

                                this.StreamerAccountUsername = this.session.StreamerUsername;
                                this.StreamerAccountAvatar = this.session.StreamerAvatarURL;
                            }
                            else
                            {
                                await this.session.DisableStreamer();

                                if (!this.streamerConnectCancellationTokenSource.IsCancellationRequested)
                                {
                                    await DispatcherHelper.Dispatcher.InvokeAsync(async () => await DialogHelper.ShowMessage(result.Message));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }

                        this.streamerConnectTask = null;

                        this.NotifyAllProperties();

                    }, this.streamerConnectCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });

            this.StreamerAccountCancelCommand = this.CreateCommand(async () =>
            {
                try
                {
                    this.streamerConnectCancellationTokenSource.Cancel();

                    this.streamerConnectTask = null;

                    await this.session.DisableStreamer();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });

            this.StreamerAccountLogOutCommand = this.CreateCommand(async () =>
            {
                try
                {
                    await this.session.DisableStreamer();

                    this.StreamerAccountUsername = null;
                    this.StreamerAccountAvatar = null;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });


            this.BotAccountLogInCommand = this.CreateCommand(() =>
            {
                try
                {
                    this.botConnectCancellationTokenSource.Cancel();
                    this.botConnectCancellationTokenSource = new CancellationTokenSource();

                    this.botConnectTask = AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        try
                        {
                            Result result = await this.session.ManualConnectBot(this.botConnectCancellationTokenSource.Token);
                            if (result.Success && !this.botConnectCancellationTokenSource.IsCancellationRequested)
                            {
                                if (string.Equals(this.session.StreamerID, this.session.BotID, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    await this.session.DisableBot();

                                    await DispatcherHelper.Dispatcher.InvokeAsync(async () => await DialogHelper.ShowMessage(Resources.BotAccountMustBeDifferent));
                                }

                                this.BotAccountUsername = this.session.BotUsername;
                                this.BotAccountAvatar = this.session.BotAvatarURL;
                            }
                            else
                            {
                                await this.session.DisableBot();

                                if (!this.botConnectCancellationTokenSource.IsCancellationRequested)
                                {
                                    await DispatcherHelper.Dispatcher.InvokeAsync(async () => await DialogHelper.ShowMessage(result.Message));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }

                        this.botConnectTask = null;

                        this.NotifyAllProperties();

                    }, this.botConnectCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });

            this.BotAccountCancelCommand = this.CreateCommand(async () =>
            {
                try
                {
                    this.botConnectCancellationTokenSource.Cancel();

                    this.botConnectTask = null;

                    await this.session.DisableBot();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });

            this.BotAccountLogOutCommand = this.CreateCommand(async () =>
            {
                try
                {
                    await this.session.DisableBot();

                    this.BotAccountUsername = null;
                    this.BotAccountAvatar = null;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.NotifyAllProperties();
            });
        }

        private void NotifyAllProperties()
        {
            this.NotifyPropertyChanged(nameof(IsStreamerAccountConnected));
            this.NotifyPropertyChanged(nameof(IsStreamerAccountLogInVisible));
            this.NotifyPropertyChanged(nameof(IsStreamerAccountCancelVisible));
            this.NotifyPropertyChanged(nameof(IsStreamerAccountLogoutVisible));

            this.NotifyPropertyChanged(nameof(IsBotAccountConnected));
            this.NotifyPropertyChanged(nameof(IsBotAccountLogInVisible));
            this.NotifyPropertyChanged(nameof(IsBotAccountCancelVisible));
            this.NotifyPropertyChanged(nameof(IsBotAccountLogoutVisible));
        }
    }
}
