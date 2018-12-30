using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SongRequestStateEnum
    {
        NotStarted = 0,
        Playing = 1,
        Paused = 2,
        Ended = 3,
    }

    public enum SongRequestServiceTypeEnum
    {
        Spotify,
        YouTube,
        [Obsolete]
        SoundCloud,

        All = 10
    }

    public class SongRequestItem : IEquatable<SongRequestItem>
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string AlbumImage { get; set; }

        public SongRequestServiceTypeEnum Type { get; set; }
        public UserViewModel User { get; set; }

        public SongRequestStateEnum State { get; set; }
        public long Progress { get; set; }
        public long Length { get; set; }

        public int Volume { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is SongRequestItem)
            {
                return this.Equals((SongRequestItem)obj);
            }
            return false;
        }

        public bool Equals(SongRequestItem other) { return other != null && this.Type == other.Type && this.ID.Equals(other.ID); }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3} - {4} / {5}", this.ID, this.Name, this.Type, this.State, this.Progress, this.Length);
        }
    }

    public interface ISongRequestService
    {
        bool IsEnabled { get; }

        Task<bool> Initialize();

        Task Disable();

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

        Task StatusUpdate(SongRequestItem item);
    }
}
