using Mixer.Base.Model.TestStreams;
using MixItUp.Base.Model;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Accounts
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

        public string PlatformImage
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    return "/Assets/Images/Mixer.png";
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    return "/Assets/Images/Twitch.png";
                }
                return string.Empty;
            }
        }

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
        public bool UserAccountButtonIsEnabled { get { return !this.IsUserAccountConnected; } }

        public bool IsUserAccountConnected
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    return ChannelSession.MixerUserConnection != null;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    return ChannelSession.TwitchUserConnection != null;
                }
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
                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    return ChannelSession.MixerBotConnection != null;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    return ChannelSession.TwitchBotConnection != null;
                }
                return false;
            }
        }

        public StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum platform)
        {
            this.Platform = platform;

            if (this.IsUserAccountConnected)
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    this.UserAccountAvatar = ChannelSession.MixerUser.avatarUrl;
                    this.UserAccountUsername = ChannelSession.MixerUser.username;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    this.UserAccountAvatar = ChannelSession.TwitchUserNewAPI.profile_image_url;
                    this.UserAccountUsername = ChannelSession.TwitchUserNewAPI.login;
                }
            }
            if (this.IsBotAccountConnected)
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    this.BotAccountAvatar = ChannelSession.MixerBot.avatarUrl;
                    this.BotAccountUsername = ChannelSession.MixerBot.username;
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    this.BotAccountAvatar = ChannelSession.TwitchBotNewAPI.profile_image_url;
                    this.BotAccountUsername = ChannelSession.TwitchBotNewAPI.login;
                }
            }

            this.UserAccountCommand = this.CreateCommand(async (parameter) =>
            {
                if (this.IsUserAccountConnected)
                {
                    //if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                    //{
                    //    ChannelSession.DisconnectMixerUser();
                    //}
                    //this.UserAccountAvatar = null;
                    //this.UserAccountUsername = null;
                }
                else
                {
                    Result result = new Result(false);
                    if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                    {
                        result = await ChannelSession.ConnectMixerUser(isStreamer: true);
                        if (result.Success)
                        {
                            this.UserAccountAvatar = ChannelSession.MixerUser.avatarUrl;
                            this.UserAccountUsername = ChannelSession.MixerUser.username;
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        result = await ChannelSession.ConnectTwitchUser(isStreamer: true);
                        if (result.Success)
                        {
                            this.UserAccountAvatar = ChannelSession.TwitchUserNewAPI.profile_image_url;
                            this.UserAccountUsername = ChannelSession.TwitchUserNewAPI.login;
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
                    if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                    {
                        await ChannelSession.DisconnectMixerBot();
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        await ChannelSession.DisconnectTwitchBot();
                    }
                    this.BotAccountAvatar = null;
                    this.BotAccountUsername = null;
                }
                else
                {
                    Result result = new Result(false);
                    if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                    {
                        result = await ChannelSession.ConnectMixerBot();
                        if (result.Success)
                        {
                            TestStreamSettingsModel testStreamSettings = await ChannelSession.MixerUserConnection.GetTestStreamSettings(ChannelSession.MixerChannel);
                            if (testStreamSettings != null && testStreamSettings.isActive.GetValueOrDefault())
                            {
                                if (!await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.TestStreamWarning))
                                {
                                    await ChannelSession.DisconnectMixerBot();
                                    return;
                                }
                            }

                            if (ChannelSession.MixerBot.id.Equals(ChannelSession.MixerUser.id))
                            {
                                await DialogHelper.ShowMessage(MixItUp.Base.Resources.IncorrectBotAccount);
                                await ChannelSession.DisconnectMixerBot();
                                return;
                            }

                            this.BotAccountAvatar = ChannelSession.MixerBot.avatarUrl;
                            this.BotAccountUsername = ChannelSession.MixerBot.username;
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        result = await ChannelSession.ConnectTwitchBot();
                        if (result.Success)
                        {
                            this.BotAccountAvatar = ChannelSession.TwitchBotNewAPI.profile_image_url;
                            this.BotAccountUsername = ChannelSession.TwitchBotNewAPI.login;
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
            this.NotifyPropertyChanged("UserAccountButtonIsEnabled");
            this.NotifyPropertyChanged("CanConnectBotAccount");
            this.NotifyPropertyChanged("IsBotAccountConnected");
            this.NotifyPropertyChanged("IsBotAccountNotConnected");
            this.NotifyPropertyChanged("BotAccountButtonContent");
        }
    }
}
