using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using System.Linq;

namespace MixItUp.Base.ViewModel.User
{
    public class UserV2ViewModel
    {
        private StreamingPlatformTypeEnum platform;
        private UserV2Model model;
        private UserPlatformV2Model platformModel;

        public UserV2ViewModel(StreamingPlatformTypeEnum platform, UserV2Model model)
        {
            this.platform = platform;
            this.model = model;
            
            if (this.platform != StreamingPlatformTypeEnum.None && this.platform != StreamingPlatformTypeEnum.All)
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2Model>(this.platform);
            }
            else if (this.model.HasPlatformData(ChannelSession.Settings.DefaultStreamingPlatform))
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2Model>(ChannelSession.Settings.DefaultStreamingPlatform);
            }
            else
            {
                this.platformModel = this.model.GetPlatformData<UserPlatformV2Model>(this.model.GetPlatforms().First());
            }
        }
    }
}
