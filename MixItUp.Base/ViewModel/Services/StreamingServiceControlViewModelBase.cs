using MixItUp.Base.Model.Actions;
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
            if (ChannelSession.Services.OBSStudio.IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.OBSStudio);
            }
            else if (ChannelSession.Services.XSplit.IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.XSplit);
            }
            else if (ChannelSession.Services.StreamlabsOBS.IsConnected)
            {
                connected.Add(StreamingSoftwareTypeEnum.StreamlabsOBS);
            }

            if (connected.Count == 1)
            {
                ChannelSession.Settings.DefaultStreamingSoftware = connected.First();
            }
        }
    }
}
