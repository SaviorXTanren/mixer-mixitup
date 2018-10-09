using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum SpotifyActionTypeEnum
    {
        Play,
        Pause,
        Next,
        Previous
    }

    [DataContract]
    public class SpotifyAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SpotifyAction.asyncSemaphore; } }

        [DataMember]
        public SpotifyActionTypeEnum SpotifyType { get; set; }

        public SpotifyAction() : base(ActionTypeEnum.Spotify) { }

        public SpotifyAction(SpotifyActionTypeEnum spotifyType)
            : this()
        {
            this.SpotifyType = spotifyType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Spotify != null)
            {
                if (this.SpotifyType == SpotifyActionTypeEnum.Play)
                {
                    await ChannelSession.Services.Spotify.PlayCurrentlyPlaying();
                }
                else if (this.SpotifyType == SpotifyActionTypeEnum.Pause)
                {
                    await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                }
                else if (this.SpotifyType == SpotifyActionTypeEnum.Next)
                {
                    await ChannelSession.Services.Spotify.NextCurrentlyPlaying();
                }
                else if (this.SpotifyType == SpotifyActionTypeEnum.Previous)
                {
                    await ChannelSession.Services.Spotify.PreviousCurrentlyPlaying();
                }
            }
        }
    }
}
