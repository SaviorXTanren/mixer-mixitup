using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
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

        public string LoginWithButtonColor
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "#9146FF"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#19D66B"; }
                return "#3f51b5";
            }
        }
        public string LoginWithButtonImage
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/TwitchMonochrome.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/YouTube.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/TrovoMonochrome.png"; }
                return StreamingPlatforms.GetPlatformImage(this.Platform);
            }
        }
        public string LoginWithButtonTextForeground
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#000000"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#FFFFFF"; }
                return "#000000";
            }
        }

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

        public ICommand StreamerAccountCommand { get; set; }
        public string StreamerAccountButtonContent
        {
            get
            {
                if (this.IsStreamerAccountConnected)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogOutOfTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogOutOfYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogOutOfTrovo; }
                }
                else
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogInWithTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogInWithYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogInWithTrovo; }
                }
                return string.Empty;
            }
        }
        public bool IsStreamerAccountConnected { get { return this.session.IsConnected; } }

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

        public ICommand BotAccountCommand { get; set; }
        public string BotAccountButtonContent
        {
            get
            {
                if (this.IsBotAccountConnected)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogOutOfTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogOutOfYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogOutOfTrovo; }
                }
                else
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogInWithTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogInWithYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogInWithTrovo; }
                }
                return string.Empty;
            }
        }
        public bool IsBotAccountConnected { get { return this.session.IsBotConnected; } }

        private StreamingPlatformSessionBase session;

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            this.session = StreamingPlatforms.GetPlatformSession(this.Platform);
            if (this.session.IsConnected)
            {
                this.StreamerAccountUsername = this.session.StreamerUsername;
                this.StreamerAccountAvatar = this.session.StreamerAvatarURL;
            }

            if (this.session.IsBotConnected)
            {
                this.BotAccountUsername = this.session.BotUsername;
                this.BotAccountAvatar = this.session.BotAvatarURL;
            }

            this.StreamerAccountCommand = this.CreateCommand(async () =>
            {
                try
                {
                    if (this.IsStreamerAccountConnected)
                    {
                        await this.session.DisableStreamer();

                        this.StreamerAccountUsername = null;
                        this.StreamerAccountAvatar = null;

                        this.BotAccountUsername = null;
                        this.BotAccountAvatar = null;
                    }
                    else
                    {

                        Result result = await this.session.ConnectStreamer();
                        if (result.Success)
                        {
                            if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.None)
                            {
                                ChannelSession.Settings.DefaultStreamingPlatform = this.Platform;
                            }

                            this.StreamerAccountUsername = this.session.StreamerUsername;
                            this.StreamerAccountAvatar = this.session.StreamerAvatarURL;
                        }
                        else
                        {
                            await this.session.DisableStreamer();
                            await this.session.DisableBot();

                            this.StreamerAccountUsername = null;
                            this.StreamerAccountAvatar = null;

                            this.BotAccountUsername = null;
                            this.BotAccountAvatar = null;

                            await DialogHelper.ShowMessage(result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                this.NotifyAllProperties();
            });

            this.BotAccountCommand = this.CreateCommand(async () =>
            {
                try
                {
                    if (this.IsBotAccountConnected)
                    {
                        await this.session.DisableBot();

                        this.BotAccountUsername = null;
                        this.BotAccountAvatar = null;
                    }
                    else
                    {
                        Result result = await this.session.ConnectBot();
                        if (result.Success)
                        {
                            if (string.Equals(this.session.StreamerID, this.session.BotID, StringComparison.CurrentCultureIgnoreCase))
                            {
                                await this.session.DisableBot();
                                result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                            }
                            else
                            {
                                this.BotAccountUsername = this.session.BotUsername;
                                this.BotAccountAvatar = this.session.BotAvatarURL;
                            }
                        }

                        if (!result.Success)
                        {
                            await this.session.DisableBot();

                            this.BotAccountUsername = null;
                            this.BotAccountAvatar = null;

                            await DialogHelper.ShowMessage(result.Message);
                        }
                    }
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
            this.NotifyPropertyChanged(nameof(StreamerAccountButtonContent));
            this.NotifyPropertyChanged(nameof(IsBotAccountConnected));
            this.NotifyPropertyChanged(nameof(BotAccountButtonContent));
        }
    }
}
