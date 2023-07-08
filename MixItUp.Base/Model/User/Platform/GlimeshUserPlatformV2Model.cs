using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class GlimeshUserPlatformV2Model : UserPlatformV2ModelBase
    {
        [Obsolete]
        public GlimeshUserPlatformV2Model() : base() { }

        public override Task Refresh()
        {
            return Task.CompletedTask;
        }
    }
}
