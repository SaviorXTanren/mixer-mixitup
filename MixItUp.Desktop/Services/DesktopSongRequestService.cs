using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Desktop.Services
{
    public class DesktopSongRequestService : ISongRequestService
    {
        private const string MixItUpPlaylistName = "Mix It Up Request Playlist";
        private const string MixItUpPlaylistDescription = "This playlist contains songs that are requested by users through Mix It Up";
        private const string SpotifyLinkPrefix = "https://open.spotify.com/track/";

        private SongRequestServiceTypeEnum serviceType;

        private List<SongRequestItem> youtubeRequests;

        private SpotifyPlaylist playlist;

        public DesktopSongRequestService()
        {
            this.youtubeRequests = new List<SongRequestItem>();
        }

        public SongRequestServiceTypeEnum GetRequestService() { return this.serviceType; }

        public async Task Initialize(SongRequestServiceTypeEnum serviceType)
        {
            this.serviceType = serviceType;

            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                this.youtubeRequests.Clear();
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                foreach (SpotifyPlaylist playlist in await ChannelSession.Services.Spotify.GetCurrentPlaylists())
                {
                    if (playlist.Name.Equals(DesktopSongRequestService.MixItUpPlaylistName))
                    {
                        this.playlist = playlist;
                        await this.ClearAllRequests();
                        break;
                    }
                }

                if (this.playlist == null)
                {
                    this.playlist = await ChannelSession.Services.Spotify.CreatePlaylist(DesktopSongRequestService.MixItUpPlaylistName, DesktopSongRequestService.MixItUpPlaylistDescription);
                }
            }
        }

        public void Disable()
        {
            this.serviceType = SongRequestServiceTypeEnum.None;
            this.youtubeRequests.Clear();
        }

        public async Task AddSongRequest(UserViewModel user, string identifier)
        {
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                if (string.IsNullOrEmpty(identifier))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "You must specify a Youtube link or ID to add a Song Request");
                    return;
                }

                await this.AddYoutubeSongRequest(user, identifier);
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                if (string.IsNullOrEmpty(identifier))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "You must specify a Spotify song ID, link, or song name to add a Song Request");
                    return;
                }

                await this.AddSpotifySongRequest(user, identifier);
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, "Song Requests are not currently enabled");
            }
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveSongRequest(SongRequestItem song)
        {
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                this.youtubeRequests.Remove(song);
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                await ChannelSession.Services.Spotify.RemoveSongFromPlaylist(this.playlist, new SpotifySong() { ID = song.ID });
            }
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task<SongRequestItem> GetCurrentlyPlaying()
        {
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                if (this.youtubeRequests.Count > 0)
                {
                    return this.youtubeRequests.FirstOrDefault();
                }
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                if (currentlyPlaying != null)
                {
                    return this.GetSpotifySongRequest(currentlyPlaying);
                }
            }
            return null;
        }

        public async Task<SongRequestItem> GetNextTrack()
        {
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                if (this.youtubeRequests.Count > 1)
                {
                    return this.youtubeRequests.Skip(1).FirstOrDefault();
                }
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                IEnumerable<SpotifySong> songs = await ChannelSession.Services.Spotify.GetPlaylistSongs(this.playlist);
                if (songs != null)
                {
                    SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                    if (currentlyPlaying != null)
                    {
                        int indexOfCurrentlyPlaying = songs.ToList().IndexOf(currentlyPlaying);
                        if (indexOfCurrentlyPlaying >= 0 && (indexOfCurrentlyPlaying + 1) < songs.Count())
                        {
                            SpotifySong nextSong = songs.ElementAt(indexOfCurrentlyPlaying + 1);
                            return this.GetSpotifySongRequest(nextSong);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<SongRequestItem>> GetAllRequests()
        {
            List<SongRequestItem> requests = new List<SongRequestItem>();
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                foreach (SongRequestItem youtubeSong in this.youtubeRequests)
                {
                    requests.Add(youtubeSong);
                }
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                IEnumerable<SpotifySong> songs = await ChannelSession.Services.Spotify.GetPlaylistSongs(this.playlist);
                if (songs != null)
                {
                    foreach (SpotifySong song in songs)
                    {
                        requests.Add(this.GetSpotifySongRequest(song));
                    }
                }
            }
            return requests;
        }

        public async Task ClearAllRequests()
        {
            if (this.serviceType == SongRequestServiceTypeEnum.Youtube)
            {
                this.youtubeRequests.Clear();
            }
            else if (this.serviceType == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
            {
                IEnumerable<SpotifySong> songs = await ChannelSession.Services.Spotify.GetPlaylistSongs(this.playlist);
                if (songs != null)
                {
                    await ChannelSession.Services.Spotify.RemoveSongsFromPlaylist(this.playlist, songs);
                }
            }
        }

        private async Task AddYoutubeSongRequest(UserViewModel user, string identifier)
        {
            identifier = identifier.Replace("https://www.youtube.com/watch?v=", "");
            identifier = identifier.Replace("https://youtu.be/", "");
            if (identifier.Contains("&"))
            {
                identifier = identifier.Substring(0, identifier.IndexOf("&"));
            }

            bool pageExists = false;

            try
            {
                HttpWebRequest request = WebRequest.Create("https://youtu.be/" + identifier) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                response.Close();
                pageExists = (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex) { Logger.Log(ex); }

            if (pageExists)
            {
                this.youtubeRequests.Add(new SongRequestItem() { ID = identifier });
                await ChannelSession.Chat.SendMessage(" was added to the queue");
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, "The YouTube link/video ID you entered was not valid");
            }
        }

        private async Task AddSpotifySongRequest(UserViewModel user, string identifier)
        {
            if (ChannelSession.Services.Spotify != null)
            {
                string songID = null;
                if (identifier.StartsWith("spotify:track:"))
                {
                    songID = identifier.Replace("spotify:track:", "");
                }
                else if (identifier.StartsWith("https://open.spotify.com/track/"))
                {
                    identifier = identifier.Replace(SpotifyLinkPrefix, "");
                    songID = identifier.Substring(0, identifier.IndexOf('?'));
                }
                else
                {
                    int artistIndex = 0;
                    if (identifier.StartsWith("/"))
                    {
                        string artistIndexString = identifier.Substring(0, identifier.IndexOf(' '));
                        identifier = identifier.Substring(identifier.IndexOf(' ') + 1);

                        artistIndexString = artistIndexString.Replace("/", "");
                        int.TryParse(artistIndexString, out artistIndex);
                    }

                    Dictionary<string, string> artistToSongID = new Dictionary<string, string>();
                    foreach (SpotifySong song in await ChannelSession.Services.Spotify.SearchSongs(identifier))
                    {
                        if (!artistToSongID.ContainsKey(song.Artist.Name))
                        {
                            artistToSongID[song.Artist.Name] = song.ID;
                        }
                    }

                    if (artistToSongID.Count == 0)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "We could not find any songs with the name specified. You can visit https://open.spotify.com/search and search for the song you want. Once you have found it, right click on the song and select \"Copy Song Link\" and run this command with that link.");
                        return;
                    }
                    else if (artistToSongID.Count == 1)
                    {
                        songID = artistToSongID.First().Value;
                    }
                    else if (artistToSongID.Count > 1)
                    {
                        if (artistIndex > 0 && artistIndex < artistToSongID.Count())
                        {
                            songID = artistToSongID.ElementAt(artistIndex - 1).Value;
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, string.Format("There are multiple artists with this song, please re-run the command with \"/# {0}\" where the # is the number of the following artists:", identifier));
                            List<string> artistsStrings = new List<string>();
                            for (int i = 0; i < artistToSongID.Count; i++)
                            {
                                artistsStrings.Add((i + 1) + ". " + artistToSongID.ElementAt(i).Key);
                            }
                            await ChannelSession.Chat.Whisper(user.UserName, string.Join(", ", artistsStrings));
                            return;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(songID))
                {
                    SpotifySong song = await ChannelSession.Services.Spotify.GetSong(songID);
                    if (song != null)
                    {
                        if (song.Explicit && !ChannelSession.Settings.SpotifyAllowExplicit)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "Explicit content is currently blocked for song requests");
                            return;
                        }
                        else
                        {
                            await ChannelSession.Services.Spotify.AddSongToPlaylist(this.playlist, song);

                            SongRequestItem request = this.GetSpotifySongRequest(song);
                            await ChannelSession.Chat.SendMessage(string.Format("{0} was added to the queue", request.Name));
                            return;
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "We could not find a valid song for your request.");
                        return;
                    }
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "This was not a valid request");
                    return;
                }
            }
        }

        private SongRequestItem GetSpotifySongRequest(SpotifySong song)
        {
            return new SongRequestItem() { ID = song.ID, Name = song.ToString() };
        }

        private async Task<SongRequestItem> GetYoutubeSongRequest(string youtubeID)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create("https://youtu.be/" + youtubeID) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                response.Close();
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }
    }
}
