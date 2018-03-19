using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    public enum SongRequestServiceTypeEnum
    {
        Spotify,
        Youtube,
    }

    /// <summary>
    /// Interaction logic for SongRequestControl.xaml
    /// </summary>
    public partial class SongRequestControl : MainControlBase
    {
        private const string SpotifyLinkPrefix = "https://open.spotify.com/track/";

        private SongRequestServiceTypeEnum serviceType;

        private List<string> youtubeRequests;

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public SongRequestControl()
        {
            InitializeComponent();

            this.youtubeRequests = new List<string>();
        }

        protected override Task InitializeInternal()
        {
            GlobalEvents.OnChatCommandMessageReceived += GlobalEvents_OnChatCommandMessageReceived;

            return base.InitializeInternal();
        }

        private async void GlobalEvents_OnChatCommandMessageReceived(object sender, ChatMessageCommandViewModel command)
        {
            if (command.CommandName.Equals(ChannelSession.Settings.SongRequestCommand))
            {

            }
        }

        public void AddYoutubeSongRequest(string identifier)
        {
            identifier = identifier.Replace("https://www.youtube.com/watch?v=", "");
            identifier = identifier.Replace("https://youtu.be/", "");
            if (identifier.Contains("&"))
            {
                identifier = identifier.Substring(0, identifier.IndexOf("&"));
            }
            this.youtubeRequests.Add(identifier);
        }

        public async Task AddSpotifySongRequest(UserViewModel user, string identifier, int artistNumber = 0)
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
                        await ChannelSession.Chat.Whisper(user.UserName, "We could not find any songs with the name specified");
                        return;
                    }
                    else if (artistToSongID.Count > 1)
                    {
                        if (artistNumber > 0)
                        {
                            songID = artistToSongID.ElementAt(artistNumber - 1).Value;
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, string.Format("There are multiple artists with this song, please re-run the command with \"!{0} /# <SONG NAME>\" where the # is the number of the following artists:", ChannelSession.Settings.SongRequestCommand));
                            List<string> artistsStrings = new List<string>();
                            for (int i = 0; i < artistToSongID.Count; i++)
                            {
                                artistsStrings.Add((i + 1) + ") " + artistToSongID.ElementAt(i));
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
                            await ChannelSession.Services.Spotify.AddSongToPlaylist(identifier);
                            await ChannelSession.Chat.SendMessage(string.Format(string.Format("The song \"{0}\" by \"{1}\" was added to the queue", song.Name, song.Artist.Name)));
                            return;
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "We could not find a valid song for your request");
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
    }
}
