using MixItUp.Base.Services;
using MixItUp.Base.Services.Demo;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class DemoUserPlatformV2Model : UserPlatformV2ModelBase
    {
        public const string DemoAvatarFilePath = "Assets/Images/DemoAvatar.png";

        public DemoUserPlatformV2Model(UserModel user)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            this.Platform = StreamingPlatformTypeEnum.Demo;
#pragma warning restore CS0612 // Type or member is obsolete

            this.SetUserProperties(user);
        }

        public DemoUserPlatformV2Model(ChatMessageViewModel message) : this(message.User.ID.ToString(), message.User.Username, message.User.Username) { }

        public DemoUserPlatformV2Model(string id, string username, string displayName)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            this.Platform = StreamingPlatformTypeEnum.Demo;
#pragma warning restore CS0612 // Type or member is obsolete
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
            this.AvatarLink = DemoAvatarFilePath;
            if (ServiceManager.Get<IFileService>().FileExists(this.GetSavedAvatarFilePath(username)))
            {
                this.AvatarLink = this.GetSavedAvatarFilePath(username);
            }
        }

        [Obsolete]
        public DemoUserPlatformV2Model() : base() { }

        public override Task Refresh()
        {
            return Task.CompletedTask;
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.login;
            this.DisplayName = user.display_name;
            this.AvatarLink = DemoAvatarFilePath;
            if (ServiceManager.Get<IFileService>().FileExists(this.GetSavedAvatarFilePath(user.login)))
            {
                this.AvatarLink = this.GetSavedAvatarFilePath(user.login);
            }
        }

        private string GetSavedAvatarFilePath(string username)
        {
            return Path.Combine(DemoPlatformService.DemoFolder, "Assets", username + ".png");
        }
    }
}
