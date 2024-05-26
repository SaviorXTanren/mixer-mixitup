using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum MusicPlayerActionTypeEnum
    {
        PlayPause,
        Play,
        PlaySpecificSong,
        Pause,
        Stop,
        Next,
        Previous,
        ChangeVolume,
        ChangeFolder,
    }

    [DataContract]
    public class MusicPlayerActionModel : ActionModelBase
    {
        [DataMember]
        public MusicPlayerActionTypeEnum ActionType { get; set; }

        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string SearchText { get; set; }

        [DataMember]
        public string FolderPath { get; set; }

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
            else if (this.ActionType == MusicPlayerActionTypeEnum.PlaySpecificSong)
            {
                string search = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.SearchText, parameters);
                MusicPlayerSong song = await ServiceManager.Get<IMusicPlayerService>().SearchAndPlaySong(search);
                if (song == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(Resources.MusicPlayerUnableToFindSong, search), parameters.Platform);
                }
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
            else if (this.ActionType == MusicPlayerActionTypeEnum.ChangeFolder)
            {
                string folderPath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FolderPath, parameters);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    await ServiceManager.Get<IMusicPlayerService>().ChangeFolder(folderPath);
                }
            }
        }
    }
}
