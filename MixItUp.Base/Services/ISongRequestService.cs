using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SongRequestServiceTypeEnum
    {
        Spotify,
        YouTube,
        SoundCloud,

        All = 10
    }

    public class SongRequestItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public SongRequestServiceTypeEnum Type { get; set; }
        public UserViewModel User { get; set; }
    }

    public interface ISongRequestService
    {
        bool IsEnabled { get; }

        Task<bool> Initialize();

        void Disable();

        Task AddSongRequest(UserViewModel user, SongRequestServiceTypeEnum service, string identifier, bool pickFirst = false);
        Task RemoveSongRequest(SongRequestItem song);
        Task RemoveLastSongRequestedByUser(UserViewModel user);

        Task PlayPauseCurrentSong();
        Task SkipToNextSong();
        Task RefreshVolume();

        Task<SongRequestItem> GetCurrentlyPlaying();
        Task<SongRequestItem> GetNextTrack();

        Task<IEnumerable<SongRequestItem>> GetAllRequests();
        Task ClearAllRequests();

        void OverlaySongFinished();
    }
}
