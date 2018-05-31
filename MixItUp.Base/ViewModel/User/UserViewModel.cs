using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public enum MixerRoleEnum
    {
        Banned,

        User = 10,

        Pro = 20,

        Partner = 25,

        Follower = 30,

        Subscriber = 40,

        Mod = 50,

        [Name("Channel Editor")]
        ChannelEditor = 55,

        Staff = 60,

        Streamer = 70,

        Custom = 99,
    }

    [DataContract]
    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public const string DefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.png";

        public static IEnumerable<MixerRoleEnum> SelectableBasicUserRoles()
        {
            List<MixerRoleEnum> roles = new List<MixerRoleEnum>(EnumHelper.GetEnumList<MixerRoleEnum>());
            roles.Remove(MixerRoleEnum.Banned);
            roles.Remove(MixerRoleEnum.Custom);
            return roles;
        }

        public static IEnumerable<string> SelectableAdvancedUserRoles()
        {
            List<string> roles = new List<string>(UserViewModel.SelectableBasicUserRoles().Select(r => EnumHelper.GetEnumName(r)));
            if (ChannelSession.Services != null && ChannelSession.Services.GameWisp != null && ChannelSession.Services.GameWisp.ChannelInfo != null)
            {
                try
                {
                    foreach (GameWispTier tier in ChannelSession.Services.GameWisp.ChannelInfo.GetActiveTiers())
                    {
                        roles.Add(tier.MIURoleName);
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }
            return roles;
        }

        public uint ID { get; set; }

        public string UserName { get; set; }

        public string AvatarLink { get; set; }

        public uint GameTypeID { get; set; }

        public DateTimeOffset? MixerAccountDate { get; set; }

        public DateTimeOffset? FollowDate { get; set; }

        public DateTimeOffset? SubscribeDate { get; set; }

        public HashSet<string> CustomRoles { get; set; }

        public int ChatOffenses { get; set; }

        public int Sparks { get; set; }

        public string InteractiveID { get; set; }

        public string InteractiveGroupID { get; set; }

        public GameWispSubscriber GameWispUser { get; set; }

        public UserViewModel()
        {
            this.CustomRoles = new HashSet<string>();
            this.AvatarLink = UserViewModel.DefaultAvatarLink;
        }

        public UserViewModel(UserModel user) : this(user.id, user.username)
        {
            this.AvatarLink = (!string.IsNullOrEmpty(user.avatarUrl)) ? user.avatarUrl : UserViewModel.DefaultAvatarLink;
            this.MixerAccountDate = user.createdAt;
        }

        public UserViewModel(ChannelModel channel) : this(channel.id, channel.token) { }

        public UserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public UserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public UserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public UserViewModel(InteractiveParticipantModel participant) : this(participant.userID, participant.username) { this.SetInteractiveDetails(participant); }

        public UserViewModel(uint id, string username) : this(id, username, new string[] { }) { }

        public UserViewModel(UserDataViewModel user) : this(user.ID, user.UserName) { }

        public UserViewModel(uint id, string username, string[] userRoles)
            : this()
        {
            this.ID = id;
            this.UserName = username;

            this.SetMixerRoles(userRoles);
        }

        private readonly HashSet<MixerRoleEnum> mixerRoles = new HashSet<MixerRoleEnum>();
        public HashSet<MixerRoleEnum> MixerRoles
        {
            get
            {
                lock (this.mixerRoles)
                {
                    if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() > DateTimeOffset.MinValue && !this.mixerRoles.Contains(MixerRoleEnum.Follower))
                    {
                        this.mixerRoles.Add(MixerRoleEnum.Follower);
                    }

                    return new HashSet<MixerRoleEnum>(mixerRoles);
                }
            }
        }

        public UserDataViewModel Data { get { return ChannelSession.Settings.UserData.GetValueIfExists(this.ID, new UserDataViewModel(this)); } }

        [JsonIgnore]
        public string RolesDisplayString { get; private set; }

        [JsonIgnore]
        public MixerRoleEnum PrimaryRole { get { return this.MixerRoles.Max(); } }

        [JsonIgnore]
        public MixerRoleEnum PrimarySortableRole { get { return this.MixerRoles.Where(r => r != MixerRoleEnum.Follower).Max(); } }

        [JsonIgnore]
        public string MixerAgeString { get { return (this.MixerAccountDate != null) ? this.MixerAccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }

        [JsonIgnore]
        public bool IsFollower { get { return this.MixerRoles.Contains(MixerRoleEnum.Follower) || this.IsSubscriber; } }

        [JsonIgnore]
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }

        [JsonIgnore]
        public bool IsSubscriber { get { return this.MixerRoles.Overlaps(new MixerRoleEnum[] { MixerRoleEnum.Subscriber, MixerRoleEnum.Streamer }); } }

        [JsonIgnore]
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }

        [JsonIgnore]
        public int SubscribeMonths
        {
            get
            {
                if (this.SubscribeDate != null)
                {
                    return this.SubscribeDate.GetValueOrDefault().TotalMonthsFromNow();
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string PrimaryRoleColorName
        {
            get
            {
                switch (this.PrimaryRole)
                {
                    case MixerRoleEnum.Streamer: return "UserStreamerRoleColor";
                    case MixerRoleEnum.Staff: return "UserStaffRoleColor";
                    case MixerRoleEnum.Mod: return "UserModRoleColor";
                    case MixerRoleEnum.Pro: return "UserProRoleColor";
                    default: return "UserDefaultRoleColor";
                }
            }
        }

        [JsonIgnore]
        public bool IsInteractiveParticipant { get { return !string.IsNullOrEmpty(this.InteractiveID) && !string.IsNullOrEmpty(this.InteractiveGroupID); } }

        [JsonIgnore]
        public GameWispTier GameWispTier
        {
            get
            {
                if (ChannelSession.Services.GameWisp != null && this.GameWispUser != null)
                {
                    return ChannelSession.Services.GameWisp.ChannelInfo.GetActiveTiers().FirstOrDefault(t => t.ID.ToString().Equals(this.GameWispUser.TierID));
                }
                return null;
            }
        }

        public async Task RefreshDetails(bool getChatDetails = true)
        {
            if (this.ID > 0)
            {
                UserWithChannelModel user = await ChannelSession.Connection.GetUser(this.ID);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.avatarUrl))
                    {
                        this.AvatarLink = user.avatarUrl;
                    }
                    this.MixerAccountDate = user.createdAt;
                    this.Sparks = (int)user.sparks;

                    this.GameTypeID = user.channel.typeId.GetValueOrDefault();

                    this.Data.UpdateData(user);
                }

                if (getChatDetails)
                {
                    ChatUserModel chatUser = await ChannelSession.Connection.GetChatUser(ChannelSession.Channel, this.ID);
                    if (chatUser != null)
                    {
                        this.SetChatDetails(chatUser);
                    }
                }

                this.FollowDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.GetModel());

                if (this.IsSubscriber)
                {
                    UserWithGroupsModel userGroups = await ChannelSession.Connection.GetUserInChannel(ChannelSession.Channel, this.ID);
                    if (userGroups != null && userGroups.groups != null)
                    {
                        UserGroupModel subscriberGroup = userGroups.groups.FirstOrDefault(g => g.name.Equals("Subscriber") && g.deletedAt == null);
                        if (subscriberGroup != null)
                        {
                            this.SubscribeDate = subscriberGroup.createdAt;
                        }
                    }
                }

                await this.SetCustomRoles();
            }
        }

        public async Task SetCustomRoles()
        {
            if (this.ID > 0)
            {
                this.CustomRoles.Clear();
                if (ChannelSession.Services.GameWisp != null)
                {
                    if (this.GameWispUser == null)
                    {
                        await this.SetGameWispSubscriber();
                    }

                    if (this.GameWispTier != null)
                    {
                        this.CustomRoles.Add(this.GameWispTier.MIURoleName);
                    }
                }
            }
        }

        public void SetChatDetails(ChatUserModel chatUser)
        {
            if (this.ID > 0 && chatUser != null)
            {
                this.SetMixerRoles(chatUser.userRoles);
            }
        }

        public void SetInteractiveDetails(InteractiveParticipantModel participant)
        {
            this.InteractiveID = participant.sessionID;
            this.InteractiveGroupID = participant.groupID;
        }

        public void RemoveInteractiveDetails()
        {
            this.InteractiveID = null;
            this.InteractiveGroupID = null;
        }

        public async Task SetGameWispSubscriber()
        {
            if (ChannelSession.Services.GameWisp != null)
            {
                IEnumerable<GameWispSubscriber> subscribers = await ChannelSession.Services.GameWisp.GetCachedSubscribers();

                GameWispSubscriber subscriber = null;
                if (this.Data.GameWispUserID > 0)
                {
                    subscriber = await ChannelSession.Services.GameWisp.GetSubscriber(this.Data.GameWispUserID);
                }
                else
                {
                    subscriber = subscribers.FirstOrDefault(s => s.UserName.Equals(this.UserName, StringComparison.CurrentCultureIgnoreCase));
                }

                this.GameWispUser = subscriber;
                if (subscriber != null)
                {
                    this.Data.GameWispUserID = subscriber.UserID;
                }
            }
        }

        public void UpdateMinuteUserData(IEnumerable<UserCurrencyViewModel> currenciesToUpdate)
        {
            this.Data.ViewingMinutes++;
            foreach (UserCurrencyViewModel currency in currenciesToUpdate)
            {
                if ((this.Data.ViewingMinutes % currency.AcquireInterval) == 0)
                {
                    this.Data.AddCurrencyAmount(currency, currency.AcquireAmount);
                    if (this.IsSubscriber)
                    {
                        this.Data.AddCurrencyAmount(currency, currency.SubscriberBonus);
                    }
                }
            }
        }

        public UserModel GetModel()
        {
            return new UserModel()
            {
                id = this.ID,
                username = this.UserName,
            };
        }

        public ChatUserModel GetChatModel()
        {
            return new ChatUserModel()
            {
                userId = this.ID,
                userName = this.UserName,
                userRoles = this.MixerRoles.Select(r => r.ToString()).ToArray(),
            };
        }

        public InteractiveParticipantModel GetParticipantModel()
        {
            return new InteractiveParticipantModel()
            {
                userID = this.ID,
                username = this.UserName,
                sessionID = this.InteractiveID,
                groupID = this.InteractiveGroupID,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public int CompareTo(UserViewModel other)
        {
            int order = this.PrimaryRole.CompareTo(other.PrimaryRole);
            if (order == 0)
            {
                return this.UserName.CompareTo(other.UserName);
            }
            return order;
        }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }

        private void SetMixerRoles(string[] userRoles)
        {
            lock (this.mixerRoles)
            {
                this.mixerRoles.Clear();
                this.mixerRoles.Add(MixerRoleEnum.User);

                if (userRoles != null && userRoles.Length > 0)
                {
                    if (userRoles.Any(r => r.Equals("Owner"))) { this.mixerRoles.Add(MixerRoleEnum.Streamer); }
                    if (userRoles.Any(r => r.Equals("Staff"))) { this.mixerRoles.Add(MixerRoleEnum.Staff); }
                    if (userRoles.Any(r => r.Equals("ChannelEditor"))) { this.mixerRoles.Add(MixerRoleEnum.ChannelEditor); }
                    if (userRoles.Any(r => r.Equals("Mod"))) { this.mixerRoles.Add(MixerRoleEnum.Mod); }
                    if (userRoles.Any(r => r.Equals("Subscriber"))) { this.mixerRoles.Add(MixerRoleEnum.Subscriber); }
                    if (userRoles.Any(r => r.Equals("Partner"))) { this.mixerRoles.Add(MixerRoleEnum.Partner); }
                    if (userRoles.Any(r => r.Equals("Pro"))) { this.mixerRoles.Add(MixerRoleEnum.Pro); }
                    if (userRoles.Any(r => r.Equals("Banned"))) { this.mixerRoles.Add(MixerRoleEnum.Banned); }
                }

                if (ChannelSession.Channel.user.username.Equals(this.UserName))
                {
                    this.mixerRoles.Add(MixerRoleEnum.Streamer);
                }

                if (this.mixerRoles.Contains(MixerRoleEnum.Streamer))
                {
                    this.mixerRoles.Add(MixerRoleEnum.Subscriber);
                    this.mixerRoles.Add(MixerRoleEnum.Mod);
                    this.mixerRoles.Add(MixerRoleEnum.Follower);
                }

                List<MixerRoleEnum> mixerDisplayRoles = this.mixerRoles.ToList();
                if (this.mixerRoles.Contains(MixerRoleEnum.Banned))
                {
                    mixerDisplayRoles.Clear();
                    mixerDisplayRoles.Add(MixerRoleEnum.Banned);
                }
                else
                {
                    if (this.mixerRoles.Count() > 1)
                    {
                        mixerDisplayRoles.Remove(MixerRoleEnum.User);
                    }

                    if (this.mixerRoles.Contains(MixerRoleEnum.Subscriber) || this.mixerRoles.Contains(MixerRoleEnum.Streamer))
                    {
                        mixerDisplayRoles.Remove(MixerRoleEnum.Follower);
                    }

                    if (this.mixerRoles.Contains(MixerRoleEnum.Streamer))
                    {
                        mixerDisplayRoles.Remove(MixerRoleEnum.Subscriber);
                        mixerDisplayRoles.Remove(MixerRoleEnum.Mod);
                    }
                }

                List<string> displayRoles = new List<string>(mixerDisplayRoles.Select(r => EnumHelper.GetEnumName(r)));
                displayRoles.AddRange(this.CustomRoles);

                this.RolesDisplayString = string.Join(", ", displayRoles.OrderByDescending(r => r));
            }
        }
    }
}
