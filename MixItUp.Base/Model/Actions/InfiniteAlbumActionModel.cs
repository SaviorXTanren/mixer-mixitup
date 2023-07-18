using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum InfiniteAlbumActionTypeEnum
    {
        Styles,
        Emotions,
        Instruments,
        SoundEffects,
    }

    [DataContract]
    public class InfiniteAlbumActionModel : ActionModelBase
    {
        public static InfiniteAlbumActionModel Create(InfiniteAlbumActionTypeEnum actionType, InfiniteAlbumCommand command)
        {
            return new InfiniteAlbumActionModel
            {
                ActionType = actionType,
                Command = command,
            };
        }

        [DataMember]
        public InfiniteAlbumActionTypeEnum ActionType { get; set; }

        [DataMember]
        public InfiniteAlbumCommand Command { get; set; }

        public InfiniteAlbumActionModel()
            : base(ActionTypeEnum.InfiniteAlbum)
        {
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.InfiniteAlbumOAuthToken != null && !ServiceManager.Get<InfiniteAlbumService>().IsConnected)
            {
                Result result = await ServiceManager.Get<InfiniteAlbumService>().Connect(ChannelSession.Settings.InfiniteAlbumOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (ServiceManager.Get<InfiniteAlbumService>().IsConnected)
            {
                await ServiceManager.Get<InfiniteAlbumService>().SendCommand(this.Command);
            }
        }
    }
}