using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.User
{
    public class UserV2ViewModel : IEquatable<UserV2ViewModel>, IComparable<UserV2ViewModel>
    {
        private StreamingPlatformTypeEnum platform;
        private UserV2Model model;
        private UserPlatformV2ModelBase platformModel;

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

        public string Username { get { return this.platformModel.Username; } }

        public void UpdateLastActivity() { this.model.LastSeen = DateTimeOffset.Now; }

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
