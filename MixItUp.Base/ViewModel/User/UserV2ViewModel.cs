using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public class UserV2ViewModel : UIViewModelBase, IEquatable<UserV2ViewModel>, IComparable<UserV2ViewModel>
    {
        private StreamingPlatformTypeEnum platform;
        private UserV2Model model;
        private UserPlatformV2ModelBase platformModel;

        private PatreonUser patreonUser;

        public UserV2ViewModel(StreamingPlatformTypeEnum platform, UserV2Model model)
        {
            this.platform = platform;
            this.model = model;
            
            if (this.platform != StreamingPlatformTypeEnum.None && this.platform != StreamingPlatformTypeEnum.All)
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2ModelBase>(this.platform);
            }
            else if (this.model.HasPlatformData(ChannelSession.Settings.DefaultStreamingPlatform))
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2ModelBase>(ChannelSession.Settings.DefaultStreamingPlatform);
            }
            else
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2ModelBase>(this.model.GetPlatforms().First());
            }
        }

        public Guid ID { get { return this.model.ID; } }

        public StreamingPlatformTypeEnum Platform { get { return this.platform; } }

        public string PlatformID { get { return this.platformModel.ID; } }

        public string Username { get { return this.platformModel.Username; } }

        public string DisplayName { get { return !string.IsNullOrEmpty(this.platformModel.DisplayName) ? this.platformModel.DisplayName : this.Username; } }

        public string FullDisplayName
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    if (!string.IsNullOrEmpty(this.platformModel.DisplayName) && !string.Equals(this.DisplayName, this.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{this.DisplayName} ({this.Username})";
                    }
                    else
                    {
                        return this.DisplayName;
                    }
                }
                else
                {
                    return this.DisplayName;
                }
            }
        }

        public string AvatarLink { get { return this.platformModel.AvatarLink; } }

        public HashSet<UserRoleEnum> Roles { get { return this.platformModel.Roles; } }

        public UserRoleEnum PrimaryRole { get { return (this.Roles.Count() > 0) ? this.Roles.ToList().Max() : UserRoleEnum.User; } }

        public bool IsFollower { get { return this.Roles.Contains(UserRoleEnum.Follower) || this.IsSubscriber; } }

        public bool IsPlatformSubscriber { get { return this.Roles.Contains(UserRoleEnum.Subscriber); } }

        public bool IsExternalSubscriber
        {
            get
            {
                if (this.PatreonUser != null && ServiceManager.Get<PatreonService>().IsConnected && !string.IsNullOrEmpty(ChannelSession.Settings.PatreonTierSubscriberEquivalent))
                {
                    PatreonTier userTier = this.PatreonTier;
                    PatreonTier equivalentTier = ServiceManager.Get<PatreonService>().Campaign.GetTier(ChannelSession.Settings.PatreonTierSubscriberEquivalent);
                    if (userTier != null && equivalentTier != null && userTier.Amount >= equivalentTier.Amount)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsSubscriber { get { return this.IsPlatformSubscriber || this.IsExternalSubscriber; } }

        public string Color
        {
            get
            {
                if (this.color == null)
                {
                    if (ChannelSession.Settings.UseCustomUsernameColors)
                    {
                        foreach (UserRoleEnum role in this.Roles.OrderByDescending(r => r))
                        {
                            if (ChannelSession.Settings.CustomUsernameColors.ContainsKey(role))
                            {
                                string name = ChannelSession.Settings.CustomUsernameColors[role];
                                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(name))
                                {
                                    this.color = ColorSchemes.HTMLColorSchemeDictionary[name];
                                    break;
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(this.color))
                    {
                        if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                        {
                            this.color = ((TwitchUserPlatformV2Model)this.platformModel).Color;
                        }
                    }

                    if (string.IsNullOrEmpty(this.color))
                    {
                        this.color = UserViewModel.UserDefaultColor;
                    }
                }
                return this.color;
            }
        }
        private string color;

        public string ChannelLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return $"https://www.twitch.tv/{this.Username}"; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return ((YouTubeUserPlatformV2Model)this.platformModel).YouTubeURL; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return $"https://www.glimesh.tv/{this.Username}"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return $"https://trovo.live/{this.Username}"; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate { get { return this.platformModel.AccountDate; } }

        public DateTimeOffset? FollowDate { get { return this.platformModel.FollowDate; } }

        public DateTimeOffset? SubscribeDate { get { return this.platformModel.SubscribeDate; } }

        public int SubscribeTier { get { return this.platformModel.SubscriberTier; } }

        public bool IsUnassociated { get { return this.Platform == StreamingPlatformTypeEnum.None; } }

        public HashSet<StreamingPlatformTypeEnum> AllPlatforms { get { return this.model.GetPlatforms(); } }

        public int OnlineViewingMinutes
        {
            get { return this.model.OnlineViewingMinutes; }
            set
            {
                this.model.OnlineViewingMinutes = value;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public int OnlineViewingMinutesOnly
        {
            get { return this.OnlineViewingMinutes % 60; }
            set
            {
                this.OnlineViewingMinutes = (this.OnlineViewingHoursOnly * 60) + value;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public int OnlineViewingHoursOnly
        {
            get { return this.OnlineViewingMinutes / 60; }
            set
            {
                this.OnlineViewingMinutes = value * 60 + this.OnlineViewingMinutesOnly;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public uint ModerationStrikes
        {
            get { return this.model.ModerationStrikes; }
            set { this.model.ModerationStrikes = value; }
        }

        public DateTimeOffset LastActivity { get { return this.model.LastActivity; } }

        public PatreonCampaignMember PatreonUser { get { return this.PatreonUser; } set { this.PatreonUser = value; } }

        public PatreonTier PatreonTier
        {
            get
            {
                if (ServiceManager.Get<PatreonService>().IsConnected && this.PatreonUser != null)
                {
                    return ServiceManager.Get<PatreonService>().Campaign.GetTier(this.PatreonUser.TierID);
                }
                return null;
            }
        }

        public void UpdateLastActivity() { this.model.LastActivity = DateTimeOffset.Now; }

        public void UpdateViewingMinutes()
        {
            this.OnlineViewingMinutes++;
            ChannelSession.Settings.UserData.ManualValueChanged(this.ID);

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.OnlineViewingHoursOnly >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.Roles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.Regular);
            }
        }

        public async Task AddModerationStrike(string moderationReason = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
            extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

            this.ModerationStrikes++;
            if (this.ModerationStrikes == 1)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike1CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.ModerationStrikes == 2)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike2CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.ModerationStrikes >= 3)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike3CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
        }

        public Task RemoveModerationStrike()
        {
            if (this.ModerationStrikes > 0)
            {
                this.ModerationStrikes--;
            }
            return Task.CompletedTask;
        }

        public async Task Refresh()
        {
            await this.platformModel.Refresh();

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.OnlineViewingHoursOnly >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.Roles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.Regular);
            }

            this.RefreshPatreonProperties();
        }

        public void RefreshPatreonProperties()
        {
            if (ServiceManager.Get<PatreonService>().IsConnected && this.PatreonUser == null)
            {
                IEnumerable<PatreonCampaignMember> campaignMembers = ServiceManager.Get<PatreonService>().CampaignMembers;

                PatreonCampaignMember patreonUser = null;
                if (!string.IsNullOrEmpty(this.model.PatreonUserID))
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => u.UserID.Equals(this.model.PatreonUserID));
                }
                else
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => this.Platform.HasFlag(u.User.Platform) && string.Equals(u.User.PlatformUserID, this.PlatformID, StringComparison.InvariantCultureIgnoreCase));
                }

                this.PatreonUser = patreonUser;
                if (patreonUser != null)
                {
                    this.model.PatreonUserID = patreonUser.UserID;
                }
                else
                {
                    this.model.PatreonUserID = null;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is UserV2ViewModel)
            {
                return this.Equals((UserV2ViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserV2ViewModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public int CompareTo(UserV2ViewModel other) { return this.Username.CompareTo(other.Username); }
    }
}
