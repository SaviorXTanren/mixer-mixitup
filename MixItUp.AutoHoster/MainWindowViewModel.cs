using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.AutoHoster
{
    public enum HostingOrderEnum
    {
        [Name("In Order")]
        InOrder,
        Random
    }

    public class MainWindowViewModel : UIViewModelBase
    {
        private const string ClientID = "dd6e3bc4e4f5adbef25698bf705079c53dae75a2e2bc2851";

        private const string SettingsFileName = "Settings/AutoHosterSettings.json";

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

        public List<string> HostingOrderItems { get; set; } = new List<string>(EnumHelper.GetEnumNames<HostingOrderEnum>());
        public string HostingOrder
        {
            get { return this.hostingOrder; }
            set
            {
                this.hostingOrder = value;
                this.NotifyPropertyChanged();
            }
        }
        private string hostingOrder;

        public List<string> AgeRatingItems { get; set; } = new List<string>(EnumHelper.GetEnumNames<AgeRatingEnum>());
        public string AgeRating
        {
            get { return this.ageRating; }
            set
            {
                this.ageRating = value;
                this.NotifyPropertyChanged();
            }
        }
        private string ageRating;

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

        public MainWindowViewModel() { }

        public async Task<bool> Initialize()
        {
            if (!Directory.Exists("Settings"))
            {
                Directory.CreateDirectory("Settings");
            }

            this.settings = await SerializerHelper.DeserializeFromFile<AutoHosterSettingsModel>(SettingsFileName);
            if (this.settings != null)
            {
                try
                {
                    this.connection = await MixerConnection.ConnectViaOAuthToken(this.settings.OAuthToken);
                }
                catch (Exception ex) { Base.Util.Logger.Log(ex); }
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
                catch (Exception ex) { Base.Util.Logger.Log(ex); }
                if (this.connection == null)
                {
                    return false;
                }
            }

            foreach (ChannelHostModel host in this.settings.Channels)
            {
                this.Channels.Add(host);
            }
            this.HostingOrder = EnumHelper.GetEnumName(this.settings.HostingOrder);
            this.AgeRating = EnumHelper.GetEnumName(this.settings.AgeRating);

            return true;
        }

        public async Task Run()
        {
            IEnumerable<ChannelHostModel> channels = this.Channels.ToList();
            HostingOrderEnum hostOrder = EnumHelper.GetEnumValueFromString<HostingOrderEnum>(this.HostingOrder);
            AgeRatingEnum ageRating = EnumHelper.GetEnumValueFromString<AgeRatingEnum>(this.AgeRating);

            PrivatePopulatedUserModel currentUser = await this.connection.Users.GetCurrentUser();
            if (currentUser != null && !currentUser.channel.online)
            {
                bool keepCurrentHost = false;
                if (currentUser.channel.hosteeId != null)
                {
                    ExpandedChannelModel channel = await this.connection.Channels.GetChannel(currentUser.channel.hosteeId.GetValueOrDefault());
                    if (channel != null)
                    {
                        AgeRatingEnum channelAgeRating = EnumHelper.GetEnumValueFromString<AgeRatingEnum>(channel.audience);
                        if (channelAgeRating <= ageRating)
                        {
                            keepCurrentHost = true;
                            this.CurrentlyHosting = new ChannelHostModel()
                            {
                                ID = channel.userId,
                                Name = channel.user.username,
                            };
                        }
                    }
                }

                if (!keepCurrentHost)
                {
                    this.CurrentlyHosting = null;
                }

                if (this.CurrentlyHosting != null)
                {
                    await this.UpdateChannel(this.CurrentlyHosting);
                }

                if (this.IsAutoHostingEnabled && (this.CurrentlyHosting == null || !this.CurrentlyHosting.IsOnline))
                {
                    if (hostOrder == HostingOrderEnum.Random)
                    {
                        channels = channels.OrderBy(c => Guid.NewGuid());
                    }

                    foreach (ChannelHostModel channel in channels)
                    {
                        if (channel.IsEnabled)
                        {
                            ChannelModel channelModel = await this.UpdateChannel(channel);
                            AgeRatingEnum channelAgeRating = EnumHelper.GetEnumValueFromString<AgeRatingEnum>(channelModel.audience);
                            if (channelModel != null && channel.IsOnline && channelAgeRating <= ageRating)
                            {
                                ChannelModel updatedChannel = await this.connection.Channels.SetHostChannel(currentUser.channel, channelModel);
                                if (updatedChannel.hosteeId.GetValueOrDefault() == channelModel.id)
                                {
                                    this.CurrentlyHosting = channel;
                                    break;
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
                Base.Util.Logger.Log(ex);
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
