using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
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

        public string PlatformName { get { return EnumHelper.GetEnumName(this.Platform); } }

        public string PlatformImage { get { return StreamingPlatforms.GetPlatformImage(this.Platform); } }

        public string UserAccountAvatar
        {
            get { return this.userAccountAvatar; }
            set
            {
                this.userAccountAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private string userAccountAvatar;
        public string UserAccountUsername
        {
            get { return this.userAccountUsername; }
            set
            {
                this.userAccountUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string userAccountUsername;

        public ICommand UserAccountCommand { get; set; }
        public string UserAccountButtonContent { get { return this.IsUserAccountConnected ? MixItUp.Base.Resources.Logout : MixItUp.Base.Resources.Login; } }
        public bool IsUserAccountConnected
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return ServiceManager.Get<TwitchSessionService>().UserConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return ServiceManager.Get<YouTubeSessionService>().UserConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return ServiceManager.Get<TrovoSessionService>().UserConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return ServiceManager.Get<GlimeshSessionService>().UserConnection != null; }
                return false;
            }
        }

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

        public ICommand BotAccountCommand { get; set; }
        public string BotAccountButtonContent { get { return this.IsBotAccountConnected ? MixItUp.Base.Resources.Logout : MixItUp.Base.Resources.Login; } }
        public bool IsBotAccountConnected
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return ServiceManager.Get<TwitchSessionService>().BotConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return ServiceManager.Get<YouTubeSessionService>().BotConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return ServiceManager.Get<TrovoSessionService>().BotConnection != null; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return ServiceManager.Get<GlimeshSessionService>().BotConnection != null; }
                return false;
            }
        }

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            if (this.IsUserAccountConnected)
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().UserNewAPI != null)
                {
                    this.UserAccountAvatar = ServiceManager.Get<TwitchSessionService>().UserNewAPI.profile_image_url;
                    this.UserAccountUsername = ServiceManager.Get<TwitchSessionService>().UserNewAPI.display_name;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    this.UserAccountAvatar = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Thumbnails.Default__.Url;
                    this.UserAccountUsername = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Title;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().User != null)
                {
                    this.UserAccountAvatar = ServiceManager.Get<TrovoSessionService>().User.profilePic;
                    this.UserAccountUsername = ServiceManager.Get<TrovoSessionService>().User.nickName;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                {
                    this.UserAccountAvatar = ServiceManager.Get<GlimeshSessionService>().User.avatarUrl;
                    this.UserAccountUsername = ServiceManager.Get<GlimeshSessionService>().User.displayname;
                }
            }

            if (this.IsBotAccountConnected)
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().BotNewAPI != null)
                {
                    this.BotAccountAvatar = ServiceManager.Get<TwitchSessionService>().BotNewAPI.profile_image_url;
                    this.BotAccountUsername = ServiceManager.Get<TwitchSessionService>().BotNewAPI.display_name;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    this.BotAccountAvatar = ServiceManager.Get<YouTubeSessionService>().Bot.Snippet.Thumbnails.Default__.Url;
                    this.BotAccountUsername = ServiceManager.Get<YouTubeSessionService>().Bot.Snippet.Title;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().Bot != null)
                {
                    this.UserAccountAvatar = ServiceManager.Get<TrovoSessionService>().Bot.profilePic;
                    this.UserAccountUsername = ServiceManager.Get<TrovoSessionService>().Bot.nickName;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                {
                    this.BotAccountAvatar = ServiceManager.Get<GlimeshSessionService>().Bot.avatarUrl;
                    this.BotAccountUsername = ServiceManager.Get<GlimeshSessionService>().Bot.displayname;
                }
            }

            this.UserAccountCommand = this.CreateCommand(async (parameter) =>
            {
                if (this.IsUserAccountConnected)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        await ServiceManager.Get<TwitchSessionService>().DisconnectUser(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        await ServiceManager.Get<YouTubeSessionService>().DisconnectUser(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        await ServiceManager.Get<TrovoSessionService>().DisconnectUser(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        await ServiceManager.Get<GlimeshSessionService>().DisconnectUser(ChannelSession.Settings);
                    }
                    this.UserAccountAvatar = null;
                    this.UserAccountUsername = null;
                }
                else
                {
                    Result result = new Result(false);
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        result = await ServiceManager.Get<TwitchSessionService>().ConnectUser();
                        if (result.Success && ServiceManager.Get<TwitchSessionService>().UserNewAPI != null)
                        {
                            this.UserAccountAvatar = ServiceManager.Get<TwitchSessionService>().UserNewAPI.profile_image_url;
                            this.UserAccountUsername = ServiceManager.Get<TwitchSessionService>().UserNewAPI.display_name;
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        result = await ServiceManager.Get<YouTubeSessionService>().ConnectUser();
                        if (result.Success && ServiceManager.Get<YouTubeSessionService>().Channel != null)
                        {
                            this.UserAccountAvatar = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Thumbnails.Default__.Url;
                            this.UserAccountUsername = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Title;
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        result = await ServiceManager.Get<TrovoSessionService>().ConnectUser();
                        if (result.Success && ServiceManager.Get<TrovoSessionService>().User != null)
                        {
                            this.UserAccountAvatar = ServiceManager.Get<TrovoSessionService>().User.profilePic;
                            this.UserAccountUsername = ServiceManager.Get<TrovoSessionService>().User.nickName;
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        result = await ServiceManager.Get<GlimeshSessionService>().ConnectUser();
                        if (result.Success && ServiceManager.Get<GlimeshSessionService>().User != null)
                        {
                            this.UserAccountAvatar = ServiceManager.Get<GlimeshSessionService>().User.avatarUrl;
                            this.UserAccountUsername = ServiceManager.Get<GlimeshSessionService>().User.displayname;
                        }
                    }

                    if (!result.Success)
                    {
                        this.UserAccountAvatar = null;
                        this.UserAccountUsername = null;

                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                this.NotifyAllProperties();
            });

            this.BotAccountCommand = this.CreateCommand(async (parameter) =>
            {
                if (this.IsBotAccountConnected)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        await ServiceManager.Get<TwitchSessionService>().DisconnectBot(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        await ServiceManager.Get<YouTubeSessionService>().DisconnectBot(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        await ServiceManager.Get<TrovoSessionService>().DisconnectBot(ChannelSession.Settings);
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        await ServiceManager.Get<GlimeshSessionService>().DisconnectBot(ChannelSession.Settings);
                    }
                    this.BotAccountAvatar = null;
                    this.BotAccountUsername = null;
                }
                else
                {
                    Result result = new Result(false);
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        result = await ServiceManager.Get<TwitchSessionService>().ConnectBot();
                        if (result.Success)
                        {
                            if (ServiceManager.Get<TwitchSessionService>().BotNewAPI.id.Equals(ServiceManager.Get<TwitchSessionService>().UserNewAPI?.id))
                            {
                                await ServiceManager.Get<TwitchSessionService>().DisconnectBot(ChannelSession.Settings);
                                result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                            }
                            else if (ServiceManager.Get<TwitchSessionService>().BotNewAPI != null)
                            {
                                this.BotAccountAvatar = ServiceManager.Get<TwitchSessionService>().BotNewAPI.profile_image_url;
                                this.BotAccountUsername = ServiceManager.Get<TwitchSessionService>().BotNewAPI.display_name;
                            }
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        result = await ServiceManager.Get<YouTubeSessionService>().ConnectUser();
                        if (result.Success && ServiceManager.Get<YouTubeSessionService>().Channel != null)
                        {
                            this.UserAccountAvatar = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Thumbnails.Default__.Url;
                            this.UserAccountUsername = ServiceManager.Get<YouTubeSessionService>().Channel.Snippet.Title;
                        }


                        result = await ServiceManager.Get<YouTubeSessionService>().ConnectBot();
                        if (result.Success)
                        {
                            if (ServiceManager.Get<YouTubeSessionService>().Bot.Id.Equals(ServiceManager.Get<YouTubeSessionService>().Channel?.Id))
                            {
                                await ServiceManager.Get<YouTubeSessionService>().DisconnectBot(ChannelSession.Settings);
                                result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                            }
                            else if (ServiceManager.Get<YouTubeSessionService>().Bot != null)
                            {
                                this.BotAccountAvatar = ServiceManager.Get<YouTubeSessionService>().Bot.Snippet.Thumbnails.Default__.Url;
                                this.BotAccountUsername = ServiceManager.Get<YouTubeSessionService>().Bot.Snippet.Title;
                            }
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        result = await ServiceManager.Get<TrovoSessionService>().ConnectBot();
                        if (result.Success)
                        {
                            if (ServiceManager.Get<TrovoSessionService>().Bot.userId.Equals(ServiceManager.Get<TrovoSessionService>().User?.userId))
                            {
                                await ServiceManager.Get<TrovoSessionService>().DisconnectBot(ChannelSession.Settings);
                                result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                            }
                            else if (ServiceManager.Get<TrovoSessionService>().Bot != null)
                            {
                                this.BotAccountAvatar = ServiceManager.Get<TrovoSessionService>().Bot.profilePic;
                                this.BotAccountUsername = ServiceManager.Get<TrovoSessionService>().Bot.nickName;
                            }
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        result = await ServiceManager.Get<GlimeshSessionService>().ConnectBot();
                        if (result.Success)
                        {
                            if (ServiceManager.Get<GlimeshSessionService>().Bot.id.Equals(ServiceManager.Get<GlimeshSessionService>().User?.id))
                            {
                                await ServiceManager.Get<GlimeshSessionService>().DisconnectBot(ChannelSession.Settings);
                                result = new Result(MixItUp.Base.Resources.BotAccountMustBeDifferent);
                            }
                            else if (ServiceManager.Get<GlimeshSessionService>().Bot != null)
                            {
                                this.BotAccountAvatar = ServiceManager.Get<GlimeshSessionService>().Bot.avatarUrl;
                                this.BotAccountUsername = ServiceManager.Get<GlimeshSessionService>().Bot.displayname;
                            }
                        }
                    }

                    if (!result.Success)
                    {
                        this.BotAccountAvatar = null;
                        this.BotAccountUsername = null;

                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                this.NotifyAllProperties();
            });
        }

        private void NotifyAllProperties()
        {
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
