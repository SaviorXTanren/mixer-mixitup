using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Model.SongRequests
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

    public class SongRequestModel : IEquatable<SongRequestModel>
    {
        public string ID { get; set; }
        public string URI { get; set; }
        public string Name { get; set; }
        public string AlbumImage { get; set; }

        public SongRequestServiceTypeEnum Type { get; set; }

        public bool IsFromBackupPlaylist { get; set; }

        public UserViewModel User { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is SongRequestModel)
            {
                return this.Equals((SongRequestModel)obj);
            }
            return false;
        }

        public bool Equals(SongRequestModel other) { return other != null && this.Type == other.Type && this.ID.Equals(other.ID); }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", this.ID, this.Name, this.Type);
        }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class SongRequestCurrentlyPlayingModel : SongRequestModel, IEquatable<SongRequestCurrentlyPlayingModel>
    {
        public SongRequestStateEnum State { get; set; }
        public long Progress { get; set; }
        public long Length { get; set; }

        public int Volume { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is SongRequestCurrentlyPlayingModel)
            {
                return this.Equals((SongRequestCurrentlyPlayingModel)obj);
            }
            return false;
        }

        public bool Equals(SongRequestCurrentlyPlayingModel other) { return other != null && this.Type == other.Type && this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3} - {4} / {5}", this.ID, this.Name, this.Type, this.State, this.Progress, this.Length);
        }
    }
}
