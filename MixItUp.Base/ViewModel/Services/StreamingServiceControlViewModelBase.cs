using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Services
{
    public abstract class StreamingServiceControlViewModelBase : ServiceControlViewModelBase
    {
        public StreamingServiceControlViewModelBase(string name) : base(name) { }

        public void ChangeDefaultStreamingSoftware()
        {
            List<StreamingSoftwareTypeEnum> connected = new List<StreamingSoftwareTypeEnum>();
            if (ServiceManager.Get<IOBSStudioService>().IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.OBSStudio);
            }
            else if (ServiceManager.Get<XSplitService>().IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.XSplit);
            }
            else if (ServiceManager.Get<StreamlabsDesktopService>().IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.StreamlabsDesktop);
            }

            if (connected.Count == 1)
            {
                ChannelSession.Settings.DefaultStreamingSoftware = connected.First();
            }
        }
    }
}
