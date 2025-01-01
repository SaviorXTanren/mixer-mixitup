using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo.New;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class TrovoUserPlatformV2Model : UserPlatformV2ModelBase
    {
        [DataMember]
        public HashSet<string> CustomRoles { get; set; } = new HashSet<string>();

        public TrovoUserPlatformV2Model(UserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Trovo;

            this.SetUserProperties(user);
        }

        public TrovoUserPlatformV2Model(PrivateUserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Trovo;
            this.ID = user.userId;
            this.Username = user.userName;
            this.DisplayName = user.nickName;
            this.AvatarLink = user.profilePic;
        }

        public TrovoUserPlatformV2Model(ChatMessageModel message)
        {
            this.Platform = StreamingPlatformTypeEnum.Trovo;

            this.SetUserProperties(message);
        }

        public TrovoUserPlatformV2Model(string id, string username, string displayName)
        {
            this.Platform = StreamingPlatformTypeEnum.Trovo;
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        [Obsolete]
        public TrovoUserPlatformV2Model() : base() { }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<TrovoSession>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<TrovoSession>().StreamerService.GetUserByName(this.Username);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }

                this.Roles.Add(UserRoleEnum.User);
            }
        }

        public void SetUserProperties(ChatMessageModel message)
        {
            if (message != null)
            {
                this.ID = message.sender_id.ToString();
                this.Username = message.user_name;
                this.DisplayName = message.nick_name;
                if (!string.IsNullOrEmpty(message.avatar))
                {
                    this.AvatarLink = message.FullAvatarURL;
                }

                if (message.roles != null)
                {
                    HashSet<string> rolesSet = new HashSet<string>(message.roles);

                    if (rolesSet.Remove(ChatMessageModel.StreamerRole)){ this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
                    if (rolesSet.Remove(ChatMessageModel.AdminRole)) { this.Roles.Add(UserRoleEnum.TrovoAdmin); } else { this.Roles.Remove(UserRoleEnum.TrovoAdmin); }
                    if (rolesSet.Remove(ChatMessageModel.WardenRole)) { this.Roles.Add(UserRoleEnum.TrovoWarden); } else { this.Roles.Remove(UserRoleEnum.TrovoWarden); }
                    if (rolesSet.Remove(ChatMessageModel.SuperModRole)) { this.Roles.Add(UserRoleEnum.TrovoSuperMod); } else { this.Roles.Remove(UserRoleEnum.TrovoSuperMod); }
                    if (rolesSet.Remove(ChatMessageModel.ModeratorRole)) { this.Roles.Add(UserRoleEnum.Moderator); } else { this.Roles.Remove(UserRoleEnum.Moderator); }
                    if (rolesSet.Remove(ChatMessageModel.EditorRole)) { this.Roles.Add(UserRoleEnum.TrovoEditor); } else { this.Roles.Remove(UserRoleEnum.TrovoEditor); }
                    if (rolesSet.Remove(ChatMessageModel.FollowerRole)) { this.Roles.Add(UserRoleEnum.Follower); } else { this.Roles.Remove(UserRoleEnum.Follower); }

                    if (rolesSet.Remove(ChatMessageModel.SubscriberRole))
                    {
                        this.Roles.Add(UserRoleEnum.Subscriber);
                        if (ServiceManager.Get<TrovoSession>().Subscribers.TryGetValue(this.ID, out ChannelSubscriberModel subscriber))
                        {
                            if (int.TryParse(subscriber.sub_tier, out int subTier) && subTier > this.SubscriberTier)
                            {
                                this.SubscriberTier = subTier;
                            }
                            if (subscriber.sub_created_at != null)
                            {
                                this.SubscribeDate = TrovoService.GetTrovoDateTime(subscriber.sub_created_at.GetValueOrDefault().ToString());
                            }
                        }
                    }
                    else
                    {
                        this.Roles.Remove(UserRoleEnum.Subscriber);
                        this.SubscriberTier = 1;
                        this.SubscribeDate = null;
                    }

                    this.CustomRoles.Clear();
                    foreach (string role in rolesSet)
                    {
                        this.CustomRoles.Add(role);
                    }
                }
            }
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.user_id;
            this.Username = user.username;
            this.DisplayName = user.nickname;
        }
    }
}
