using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SongRequestServiceTypeEnum
    {
        None,
        Spotify,
        Youtube,
    }

    public class SongRequestItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public interface ISongRequestService
    {
        SongRequestServiceTypeEnum GetRequestService();

        Task Initialize(SongRequestServiceTypeEnum serviceType);

        void Disable();

        Task AddSongRequest(UserViewModel user, string identifier);

        Task RemoveSongRequest(SongRequestItem song);

        Task<SongRequestItem> GetCurrentlyPlaying();

        Task<SongRequestItem> GetNextTrack();

        Task<IEnumerable<SongRequestItem>> GetAllRequests();

        Task ClearAllRequests();
    }
}
