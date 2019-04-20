using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.AutoHoster
{
    public class MainWindowViewModel : UIViewModelBase
    {
        private const string ClientID = "dd6e3bc4e4f5adbef25698bf705079c53dae75a2e2bc2851";

        private const string SettingsFileName = "AutoHosterSettings.json";

        public ObservableCollection<ChannelHostModel> Channels { get; set; } = new ObservableCollection<ChannelHostModel>();

        public ChannelHostModel CurrentlyHosting
        {
            get { return this.currentlyHosting; }
            set
            {
                this.currentlyHosting = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CurrentlyHostingName");
            }
        }
        private ChannelHostModel currentlyHosting;
        public string CurrentlyHostingName { get { return (this.currentlyHosting != null) ? this.currentlyHosting.Name : "NONE"; } }

        public bool IsAutoHostingEnabled
        {
            get { return this.isAutoHostingEnabled; }
            set
            {
                this.isAutoHostingEnabled = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("AutoHostingState");
                this.NotifyPropertyChanged("AutoHostingStateColor");
            }
        }
        private bool isAutoHostingEnabled = true;
        public string AutoHostingState { get { return (this.IsAutoHostingEnabled) ? "ON" : "OFF"; } }
        public string AutoHostingStateColor { get { return (this.IsAutoHostingEnabled) ? "Green" : "Red"; } }

        private AutoHosterSettingsModel settings;
        private MixerConnection connection;

        public async Task<bool> Initialize()
        {
            this.settings = await SerializerHelper.DeserializeFromFile<AutoHosterSettingsModel>(SettingsFileName);
            if (this.settings != null)
            {
                try
                {
                    this.connection = await MixerConnection.ConnectViaOAuthToken(this.settings.OAuthToken);
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            else
            {
                this.settings = new AutoHosterSettingsModel();
            }

            if (this.connection == null)
            {
                try
                {
                    this.connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ClientID, new List<OAuthClientScopeEnum>() { OAuthClientScopeEnum.channel__details__self, OAuthClientScopeEnum.channel__update__self }, loginSuccessHtmlPageFilePath: "LoginRedirectPage.html");
                }
                catch (Exception ex) { Logger.Log(ex); }
                if (this.connection == null)
                {
                    return false;
                }
            }

            foreach (ChannelHostModel host in this.settings.Channels)
            {
                this.Channels.Add(host);
            }

            return true;
        }

        public async Task Run()
        {
            IEnumerable<ChannelHostModel> channels = this.Channels.ToList();

            PrivatePopulatedUserModel currentUser = await this.connection.Users.GetCurrentUser();
            if (currentUser != null && !currentUser.channel.online)
            {
                if (currentUser.channel.hosteeId != null)
                {
                    ExpandedChannelModel channel = await this.connection.Channels.GetChannel(currentUser.channel.hosteeId.GetValueOrDefault());
                    if (channel != null)
                    {
                        this.CurrentlyHosting = new ChannelHostModel()
                        {
                            ID = channel.userId,
                            Name = channel.user.username,
                        };
                    }
                    else
                    {
                        this.CurrentlyHosting = null;
                    }
                }
                else
                {
                    this.CurrentlyHosting = null;
                }

                if (this.CurrentlyHosting != null)
                {
                    await this.UpdateChannel(this.CurrentlyHosting);
                }

                if (this.IsAutoHostingEnabled && (this.CurrentlyHosting == null || !this.CurrentlyHosting.IsOnline))
                {
                    foreach (ChannelHostModel channel in channels)
                    {
                        if (channel.IsEnabled)
                        {
                            ChannelModel channelModel = await this.UpdateChannel(channel);
                            if (channelModel != null && channel.IsOnline)
                            {
                                ChannelModel updatedChannel = await this.connection.Channels.SetHostChannel(currentUser.channel, channelModel);
                                if (updatedChannel.hosteeId.GetValueOrDefault() == channelModel.id)
                                {
                                    this.CurrentlyHosting = channel;
                                }
                            }
                        }
                    }
                }
            }

            await this.SaveData();
        }

        public async Task<bool> AddChannel(string channelName)
        {
            if (!string.IsNullOrEmpty(channelName))
            {
                UserModel channel = await this.connection.Users.GetUser(channelName);
                if (channel != null)
                {
                    this.Channels.Add(new ChannelHostModel(channel));
                    await this.SaveData();
                    return true;
                }
            }
            return false;
        }

        public async Task MoveChannelUp(ChannelHostModel channel)
        {
            int index = this.Channels.IndexOf(channel) - 1;
            if (index >= 0)
            {
                this.Channels.Remove(channel);
                this.Channels.Insert(index, channel);
                await this.SaveData();
            }
        }

        public async Task MoveChannelDown(ChannelHostModel channel)
        {
            int index = this.Channels.IndexOf(channel) + 1;
            if (index < this.Channels.Count)
            {
                this.Channels.Remove(channel);
                this.Channels.Insert(index, channel);
                await this.SaveData();
            }
        }

        public async Task DeleteChannel(ChannelHostModel channel)
        {
            this.Channels.Remove(channel);
            await this.SaveData();
        }

        public async Task EnableDisableChannel(ChannelHostModel channel)
        {
            channel.IsEnabled = !channel.IsEnabled;
            await this.SaveData();
        }

        public async Task SaveData()
        {
            try
            {
                this.settings.OAuthToken = this.connection.GetOAuthTokenCopy();
                this.settings.Channels = this.Channels.ToList();
                await SerializerHelper.SerializeToFile(SettingsFileName, this.settings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async Task<ChannelModel> UpdateChannel(ChannelHostModel channel)
        {
            UserWithChannelModel channelModel = await this.connection.Users.GetUser(channel.ID);
            if (channelModel != null)
            {
                channel.ID = channel.ID;
                channel.Name = channelModel.username;
                channel.IsOnline = channelModel.channel.online;
            }
            else
            {
                channel.IsOnline = false;
            }
            return channelModel.channel;
        }
    }
}
