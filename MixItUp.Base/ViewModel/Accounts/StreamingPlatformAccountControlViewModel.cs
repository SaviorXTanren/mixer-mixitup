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

        public string UserAccountUsername { get; private set; }
        public string UserAccountAvatar { get; private set; }

        public ICommand UserAccountCommand { get; set; }
        public string UserAccountButtonContent
        {
            get
            {
                if (this.IsUserAccountConnected)
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
        public bool IsUserAccountConnected { get { return StreamingPlatforms.GetPlatformSession(this.Platform).IsConnected; } }

        public string BotAccountUsername { get; private set; }
        public string BotAccountAvatar { get; private set; }

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
        public bool IsBotAccountConnected { get { return StreamingPlatforms.GetPlatformSession(this.Platform).IsBotConnected; } }

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(this.Platform);
            if (session.IsConnected)
            {
                this.UserAccountUsername = session.StreamerUsername;
                this.UserAccountAvatar = session.StreamerAvatarURL;
            }

            if (session.IsBotConnected)
            {
                this.BotAccountUsername = session.BotUsername;
                this.BotAccountAvatar = session.BotAvatarURL;
            }

            this.UserAccountCommand = this.CreateCommand(async () =>
            {
                if (this.IsUserAccountConnected)
                {
                    await StreamingPlatforms.GetPlatformSession(this.Platform).DisableStreamer();

                    this.UserAccountUsername = null;
                    this.UserAccountAvatar = null;

                    this.BotAccountUsername = null;
                    this.BotAccountAvatar = null;
                }
                else
                {
                    Result result = await StreamingPlatforms.GetPlatformSession(this.Platform).ConnectStreamer();
                    if (result.Success)
                    {
                        if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.None)
                        {
                            ChannelSession.Settings.DefaultStreamingPlatform = this.Platform;
                        }

                        this.UserAccountUsername = session.StreamerUsername;
                        this.UserAccountAvatar = session.StreamerAvatarURL;
                    }
                    else
                    {
                        this.UserAccountUsername = null;
                        this.UserAccountAvatar = null;

                        this.BotAccountUsername = null;
                        this.BotAccountAvatar = null;

                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                this.NotifyAllProperties();
            });

            this.BotAccountCommand = this.CreateCommand(async () =>
            {
                if (this.IsBotAccountConnected)
                {
                    await StreamingPlatforms.GetPlatformSession(this.Platform).DisableBot();

                    this.BotAccountUsername = null;
                    this.BotAccountAvatar = null;
                }
                else
                {
                    Result result = await StreamingPlatforms.GetPlatformSession(this.Platform).ConnectBot();
                    if (result.Success)
                    {
                        if (string.Equals(StreamingPlatforms.GetPlatformSession(this.Platform).StreamerID, StreamingPlatforms.GetPlatformSession(this.Platform).BotID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            await StreamingPlatforms.GetPlatformSession(this.Platform).DisableBot();
                            result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                        }
                        else
                        {
                            this.BotAccountUsername = session.BotUsername;
                            this.BotAccountAvatar = session.BotAvatarURL;
                        }
                    }

                    if (!result.Success)
                    {
                        this.BotAccountUsername = null;
                        this.BotAccountAvatar = null;

                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                this.NotifyAllProperties();
            });
        }

        private void NotifyAllProperties()
        {
            this.NotifyPropertyChanged("UserAccountUsername");
            this.NotifyPropertyChanged("UserAccountAvatar");
            this.NotifyPropertyChanged("BotAccountUsername");
            this.NotifyPropertyChanged("BotAccountAvatar");
            this.NotifyPropertyChanged("IsUserAccountConnected");
            this.NotifyPropertyChanged("IsUserAccountNotConnected");
            this.NotifyPropertyChanged("UserAccountButtonContent");
            this.NotifyPropertyChanged("CanConnectBotAccount");
            this.NotifyPropertyChanged("IsBotAccountConnected");
            this.NotifyPropertyChanged("IsBotAccountNotConnected");
            this.NotifyPropertyChanged("BotAccountButtonContent");
        }
    }
}
