using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum MusicPlayerState
    {
        Stopped,
        Playing,
        Paused,
    }

    public class MusicPlayerSong
    {
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Length { get; set; }

        public string LengthString
        {
            get
            {
                int minutes = this.Length % 60;
                int seconds = this.Length / 60;
                string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();
                return $"{minutes}:{secondsText}";
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Artist))
            {
                return $"{this.Artist} - {this.Title}";
            }
            else
            {
                return this.Title;
            }
        }
    }

    public interface IMusicPlayerService
    {
        event EventHandler SongChanged;

        MusicPlayerState State { get; }

        int Volume { get; }

        MusicPlayerSong CurrentSong { get; }

        ThreadSafeObservableCollection<MusicPlayerSong> Songs { get; }

        Task Play();

        Task Pause();

        Task Stop();

        Task Next();

        Task Previous();

        Task ChangeVolume(int amount);

        Task ChangeFolder(string folderPath);

        Task LoadSongs();

        Task<MusicPlayerSong> SearchAndPlaySong(string searchText);
    }
}
