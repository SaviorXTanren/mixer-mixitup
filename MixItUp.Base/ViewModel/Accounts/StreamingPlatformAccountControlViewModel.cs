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
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#FF0000"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#19D66B"; }
                if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return "#060818"; }
                return "#3f51b5";
            }
        }
        public string LoginWithButtonImage
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/TwitchMonochrome.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/YouTubeMonochrome.png"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/TrovoMonochrome.png"; }
                return StreamingPlatforms.GetPlatformImage(this.Platform);
            }
        }
        public string LoginWithButtonTextForeground
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "#FFFFFF"; }
                if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return "#FFFFFF"; }
                return "#000000";
            }
        }

        public StreamingPlatformAccountModel UserAccount { get; set; }
        public string UserAccountUsername { get { return this.UserAccount?.Username; } }
        public string UserAccountAvatar { get { return this.UserAccount?.AvatarURL; } }

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
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return MixItUp.Base.Resources.LogOutOfGlimesh; }
                }
                else
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogInWithTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogInWithYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogInWithTrovo; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return MixItUp.Base.Resources.LogInWithGlimesh; }
                }
                return string.Empty;
            }
        }
        public bool IsUserAccountConnected { get { return StreamingPlatforms.GetPlatformSessionService(this.Platform).IsConnected; } }

        public StreamingPlatformAccountModel BotAccount { get; set; }
        public string BotAccountUsername { get { return this.BotAccount?.Username; } }
        public string BotAccountAvatar { get { return this.BotAccount?.AvatarURL; } }

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
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return MixItUp.Base.Resources.LogOutOfGlimesh; }
                }
                else
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return MixItUp.Base.Resources.LogInWithTwitch; }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return MixItUp.Base.Resources.LogInWithYouTube; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return MixItUp.Base.Resources.LogInWithTrovo; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return MixItUp.Base.Resources.LogInWithGlimesh; }
                }
                return string.Empty;
            }
        }
        public bool IsBotAccountConnected { get { return StreamingPlatforms.GetPlatformSessionService(this.Platform).IsBotConnected; } }

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            if (this.IsUserAccountConnected)
            {
                this.UserAccount = StreamingPlatforms.GetPlatformSessionService(this.Platform).UserAccount;
            }

            if (this.IsBotAccountConnected)
            {
                this.BotAccount = StreamingPlatforms.GetPlatformSessionService(this.Platform).BotAccount;
            }

            this.UserAccountCommand = this.CreateCommand(async () =>
            {
                if (this.IsUserAccountConnected)
                {
                    await StreamingPlatforms.GetPlatformSessionService(this.Platform).DisconnectUser(ChannelSession.Settings);
                    this.UserAccount = null;
                    this.BotAccount = null;
                }
                else
                {
                    Result result = await StreamingPlatforms.GetPlatformSessionService(this.Platform).ConnectUser();
                    if (result.Success)
                    {
                        this.UserAccount = StreamingPlatforms.GetPlatformSessionService(this.Platform).UserAccount;
                        if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.None)
                        {
                            ChannelSession.Settings.DefaultStreamingPlatform = this.Platform;
                        }
                    }
                    else
                    {
                        this.UserAccount = null;
                        this.BotAccount = null;
                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                this.NotifyAllProperties();
            });

            this.BotAccountCommand = this.CreateCommand(async () =>
            {
                if (this.IsBotAccountConnected)
                {
                    await StreamingPlatforms.GetPlatformSessionService(this.Platform).DisconnectBot(ChannelSession.Settings);
                    this.BotAccount = null;
                }
                else
                {
                    Result result = await StreamingPlatforms.GetPlatformSessionService(this.Platform).ConnectBot();
                    if (result.Success)
                    {
                        if (string.Equals(StreamingPlatforms.GetPlatformSessionService(this.Platform).UserID, StreamingPlatforms.GetPlatformSessionService(this.Platform).BotID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            await StreamingPlatforms.GetPlatformSessionService(this.Platform).DisconnectBot(ChannelSession.Settings);
                            result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                        }
                        else
                        {
                            this.BotAccount = StreamingPlatforms.GetPlatformSessionService(this.Platform).BotAccount;
                        }
                    }

                    if (!result.Success)
                    {
                        this.BotAccount = null;
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
