using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MixItUp.Desktop.Services
{
    [ComVisible(true)]
    public class YouTubeSongRequestProviderService : ISongRequestProviderService
    {
        public const string YouTubeDefaultAlbumArt = "https://www.youtube.com/yt/about/media/images/brand-resources/icons/YouTube_icon_full-color.svg";

        private const string YouTubeFullLinkPrefix = "https://www.youtube.com/watch?v=";
        private const string YouTubeFullLinkWithTimePattern = @"https://www.youtube.com/watch\?t=\w+&v=";
        private const string YouTubeLongLinkPrefix = "www.youtube.com/watch?v=";
        private const string YouTubeShortLinkPrefix = "youtu.be/";
        private const string YouTubeHost = "youtube.com";

        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/youtubesongrequests/";
        public const int Port = 8199;

        private Dispatcher dispatcher;
        private WebBrowser browser;
        private SongRequestCurrentlyPlayingModel status = null;
        private YouTubeSongRequestHttpListenerServer httpListenerServer;

        public string HttpListenerServerAddress { get { return string.Format(RegularOverlayHttpListenerServerAddressFormat, Port); } }

        public SongRequestServiceTypeEnum Type { get { return SongRequestServiceTypeEnum.YouTube; } }

        public YouTubeSongRequestProviderService(Dispatcher dispatcher, WebBrowser browser)
        {
            this.dispatcher = dispatcher;
            this.browser = browser;

            this.browser.ObjectForScripting = this;
        }

        public async Task<bool> Initialize()
        {
            if (this.httpListenerServer != null)
            {
                return true;
            }

            this.httpListenerServer = new YouTubeSongRequestHttpListenerServer(this.HttpListenerServerAddress, Port);
            await this.httpListenerServer.Initialize();
            this.httpListenerServer.Start();

            await this.dispatcher.InvokeAsync(() =>
            {
                this.browser.Navigate(HttpListenerServerAddress);
            });

            return true;
        }

        public void SongRequestComplete(string result)
        {
            this.status = SerializerHelper.DeserializeFromString<SongRequestCurrentlyPlayingModel>(result);
        }

        public void SetStatus(string result)
        {
            this.status = SerializerHelper.DeserializeFromString<SongRequestCurrentlyPlayingModel>(result);
        }

        public void Error(string error)
        {
            Logger.Log("YouTube Song Requests Error: " + error);
        }


        public async Task<IEnumerable<SongRequestModel>> GetPlaylist(string identifier)
        {
            List<SongRequestModel> results = new List<SongRequestModel>();
            try
            {
                Uri uri = new Uri(identifier);
                if (uri.Host.EndsWith(YouTubeHost, StringComparison.InvariantCultureIgnoreCase))
                {
                    NameValueCollection queryParameteters = HttpUtility.ParseQueryString(uri.Query);
                    if (!string.IsNullOrEmpty(queryParameteters["list"]))
                    {
                        using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                        {
                            string pageToken = null;
                            do
                            {
                                string restURL = string.Format("youtube/v3/playlistItems?playlistId={0}&maxResults=50&part=snippet,contentDetails&key={1}", HttpUtility.UrlEncode(queryParameteters["list"]), ChannelSession.SecretManager.GetSecret("YouTubeKey"));
                                if (!string.IsNullOrEmpty(pageToken))
                                {
                                    restURL += "&pageToken=" + pageToken;
                                }
                                HttpResponseMessage response = await client.GetAsync(restURL);
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string content = await response.Content.ReadAsStringAsync();
                                    JObject jobj = JObject.Parse(content);
                                    if (jobj["nextPageToken"] != null)
                                    {
                                        pageToken = jobj["nextPageToken"].ToString();
                                    }
                                    else
                                    {
                                        pageToken = null;
                                    }

                                    if (jobj["items"] != null && jobj["items"] is JArray)
                                    {
                                        JArray items = (JArray)jobj["items"];
                                        foreach (JToken item in items)
                                        {
                                            if (item["kind"] != null && item["kind"].ToString().Equals("youtube#playlistItem"))
                                            {
                                                string albumArt = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString();
                                                results.Add(new SongRequestModel()
                                                {
                                                    ID = item["contentDetails"]["videoId"].ToString(),
                                                    URI = item["contentDetails"]["videoId"].ToString(),
                                                    Name = item["snippet"]["title"].ToString(),
                                                    Type = SongRequestServiceTypeEnum.YouTube,
                                                    AlbumImage = (!string.IsNullOrEmpty(albumArt)) ? albumArt : YouTubeDefaultAlbumArt
                                                });
                                            }
                                        }
                                    }
                                }
                            } while (!string.IsNullOrEmpty(pageToken));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return results;
        }

        public async Task<IEnumerable<SongRequestModel>> Search(string identifier)
        {
            List<SongRequestModel> results = new List<SongRequestModel>();
            try
            {
                identifier = identifier.Replace("https://", "");
                if (identifier.Contains(YouTubeLongLinkPrefix) || identifier.Contains(YouTubeShortLinkPrefix))
                {
                    identifier = identifier.Replace(YouTubeLongLinkPrefix, "");
                    identifier = identifier.Replace(YouTubeShortLinkPrefix, "");
                    if (identifier.Contains("&"))
                    {
                        identifier = identifier.Substring(0, identifier.IndexOf("&"));
                    }

                    using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                    {
                        HttpResponseMessage response = await client.GetAsync(string.Format("youtube/v3/videos?id={0}&part=snippet,contentDetails&key={1}", HttpUtility.UrlEncode(identifier), ChannelSession.SecretManager.GetSecret("YouTubeKey")));
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            JObject jobj = JObject.Parse(content);
                            if (jobj["items"] != null && jobj["items"] is JArray)
                            {
                                JArray items = (JArray)jobj["items"];
                                if (items.Count > 0)
                                {
                                    JToken item = items.First;
                                    if (item["id"] != null && item["snippet"] != null && item["contentDetails"] != null && item["kind"].ToString().Equals("youtube#video"))
                                    {
                                        string albumArt = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString();
                                        results.Add(new SongRequestModel()
                                        {
                                            ID = item["id"].ToString(),
                                            URI = item["id"].ToString(),
                                            Name = item["snippet"]["title"].ToString(),
                                            Type = SongRequestServiceTypeEnum.YouTube,
                                            AlbumImage = (!string.IsNullOrEmpty(albumArt)) ? albumArt : YouTubeDefaultAlbumArt
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                    {
                        HttpResponseMessage response = await client.GetAsync(string.Format("youtube/v3/search?q={0}&maxResults=5&type=video&part=snippet&key={1}", HttpUtility.UrlEncode(identifier), ChannelSession.SecretManager.GetSecret("YouTubeKey")));
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            JObject jobj = JObject.Parse(content);
                            if (jobj["items"] != null && jobj["items"] is JArray)
                            {
                                JArray items = (JArray)jobj["items"];
                                foreach (JToken item in items)
                                {
                                    if (item["id"] != null && item["id"]["kind"] != null && item["id"]["kind"].ToString().Equals("youtube#video"))
                                    {
                                        string albumArt = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString();
                                        results.Add(new SongRequestModel()
                                        {
                                            ID = item["id"]["videoId"].ToString(),
                                            URI = item["id"]["videoId"].ToString(),
                                            Name = item["snippet"]["title"].ToString(),
                                            Type = SongRequestServiceTypeEnum.YouTube,
                                            AlbumImage = (!string.IsNullOrEmpty(albumArt)) ? albumArt : YouTubeDefaultAlbumArt
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return results;
        }

        public async Task<SongRequestCurrentlyPlayingModel> GetStatus()
        {
            this.status = null;
            if (this.httpListenerServer != null)
            {
                await this.DispatchWrapper(() => this.browser.InvokeScript("getStatus"));
                for (int i = 0; i < 10 && this.status == null; i++)
                {
                    await Task.Delay(500);
                }
            }
            return status;
        }

        public async Task Play(SongRequestModel song)
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("play", new object[] { song.ID }));
        }

        public async Task Pause()
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("pause"));
        }

        public async Task Resume()
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("resume"));
        }

        public async Task PauseResume()
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("pauseResume"));
        }

        public async Task Stop()
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("stop"));
        }

        public async Task SetVolume(int volume)
        {
            await this.DispatchWrapper(() => this.browser.InvokeScript("setVolume", new object[] { volume }));
        }

        private async Task DispatchWrapper(Action action)
        {
            try
            {
                if (this.httpListenerServer != null)
                {
                    await this.dispatcher.InvokeAsync(() =>
                    {
                        action();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

    public class YouTubeSongRequestHttpListenerServer : HttpListenerServerBase
    {
        private const string OverlayFolderPath = "Overlay\\";
        private const string OverlayWebpageFilePath = OverlayFolderPath + "YouTubePage.html";

        private int port;
        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public YouTubeSongRequestHttpListenerServer(string address, int port)
            : base(address)
        {
            this.port = port;
        }

        public async Task Initialize()
        {
            this.webPageInstance = await ChannelSession.Services.FileService.ReadFile(OverlayWebpageFilePath);
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            string url = listenerContext.Request.Url.LocalPath;
            url = url.Trim(new char[] { '/' });

            if (url.Equals("youtubesongrequests"))
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.OK, this.webPageInstance);
            }
            else
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "");
            }
        }
    }
}
