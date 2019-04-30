using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MixItUp.Desktop.Services
{
    [ComVisible(true)]
    public class YouTubeSongRequestService : IYouTubeSongRequestService
    {
        private Dispatcher dispatcher;
        private WebBrowser browser;
        private SongRequestItem status = null;
        private YouTubeSongRequestHttpListenerServer httpListenerServer;

        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/youtubesongrequests/";
        public const int Port = 8199;

        public string HttpListenerServerAddress { get { return string.Format(RegularOverlayHttpListenerServerAddressFormat, Port); } }

        public YouTubeSongRequestService(Dispatcher dispatcher, WebBrowser browser)
        {
            this.dispatcher = dispatcher;
            this.browser = browser;

            this.browser.ObjectForScripting = this;
        }

        public async Task Initialize()
        {
            if (this.httpListenerServer != null)
            {
                return;
            }

            this.httpListenerServer = new YouTubeSongRequestHttpListenerServer(this.HttpListenerServerAddress, Port);
            await this.httpListenerServer.Initialize();
            this.httpListenerServer.Start();

            await this.dispatcher.InvokeAsync(() =>
            {
                this.browser.Navigate(HttpListenerServerAddress);
            });
        }

        public void SongRequestComplete()
        {
            // Currently unused, but we COULD wire this up if desired
        }

        public void SetStatus(string result)
        {
            this.status = SerializerHelper.DeserializeFromString<SongRequestItem>(result);
        }

        public async Task<SongRequestItem> GetStatus()
        {
            this.status = null;

            if (this.httpListenerServer != null)
            {
                await this.dispatcher.InvokeAsync(() =>
                {
                    this.browser.InvokeScript("getStatus");
                });

                for (int i = 0; i < 10 && this.status == null; i++)
                {
                    await Task.Delay(500);
                }
            }

            return status;
        }

        public async Task PlayPause()
        {
            if (this.httpListenerServer != null)
            {
                await this.dispatcher.InvokeAsync(() =>
                {
                    this.browser.InvokeScript("playPause");
                });
            }
        }

        public async Task PlaySong(string itemId, int volume)
        {
            if (this.httpListenerServer != null)
            {
                await this.dispatcher.InvokeAsync(() =>
                {
                    this.browser.InvokeScript("play", new object[] { itemId, volume });
                });
            }
        }

        public async Task SetVolume(int volume)
        {
            if (this.httpListenerServer != null)
            {
                await this.dispatcher.InvokeAsync(() =>
                {
                    this.browser.InvokeScript("setVolume", new object[] { volume });
                });
            }
        }

        public async Task Stop()
        {
            if (this.httpListenerServer != null)
            {
                await this.dispatcher.InvokeAsync(() =>
                {
                    this.browser.InvokeScript("stop");
                });
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
