using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SongRequestServiceTypeEnum
    {
        Spotify,
        YouTube,
        SoundCloud,
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

        Task AddSongRequest(UserViewModel user, string identifier);
        Task RemoveSongRequest(SongRequestItem song);

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
