using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum MusicPlayerActionTypeEnum
    {
        PlayPause,
        Play,
        Pause,
        Stop,
        Next,
        Previous,
        ChangeVolume,
    }

    [DataContract]
    public class MusicPlayerActionModel : ActionModelBase
    {
        [DataMember]
        public MusicPlayerActionTypeEnum ActionType { get; set; }

        [DataMember]
        public int Volume { get; set; }

        public MusicPlayerActionModel(MusicPlayerActionTypeEnum actionType)
            : base(ActionTypeEnum.MusicPlayer)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        private MusicPlayerActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == MusicPlayerActionTypeEnum.PlayPause)
            {
                if (ServiceManager.Get<IMusicPlayerService>().State == MusicPlayerState.Playing)
                {
                    await ServiceManager.Get<IMusicPlayerService>().Pause();
                }
                else
                {
                    await ServiceManager.Get<IMusicPlayerService>().Play();
                }
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.Play)
            {
                await ServiceManager.Get<IMusicPlayerService>().Play();
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.Pause)
            {
                await ServiceManager.Get<IMusicPlayerService>().Pause();
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.Stop)
            {
                await ServiceManager.Get<IMusicPlayerService>().Stop();
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.Next)
            {
                await ServiceManager.Get<IMusicPlayerService>().Next();
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.Previous)
            {
                await ServiceManager.Get<IMusicPlayerService>().Previous();
            }
            else if (this.ActionType == MusicPlayerActionTypeEnum.ChangeVolume)
            {
                await ServiceManager.Get<IMusicPlayerService>().ChangeVolume(this.Volume);
            }
        }
    }
}
