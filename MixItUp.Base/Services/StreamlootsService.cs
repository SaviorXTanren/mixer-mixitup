using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamlootsCardModel
    {
        public string type { get; set; }
        public string imageUrl { get; set; }
        public string videoUrl { get; set; }
        public string soundUrl { get; set; }
        public StreamlootsCardDataModel data { get; set; }
    }

    public class StreamlootsCardDataModel
    {
        public string cardName { get; set; }
        public List<StreamlotsCardDataFieldModel> fields { get; set; }

        public string Message
        {
            get
            {
                StreamlotsCardDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("message"));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public string Username
        {
            get
            {
                StreamlotsCardDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("username"));
                return (field != null) ? field.value : string.Empty;
            }
        }
    }

    public class StreamlotsCardDataFieldModel
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public interface IStreamlootsService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();
    }

    public class StreamlootsService : IStreamlootsService, IDisposable
    {
        private OAuthTokenModel token;

        private WebRequest webRequest;
        private Stream responseStream;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public StreamlootsService(string streamlootsID) : this(new OAuthTokenModel() { accessToken = streamlootsID }) { }

        public StreamlootsService(OAuthTokenModel token) { this.token = token; }

        public Task<bool> Connect()
        {
            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(this.BackgroundCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return Task.FromResult(true);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(false);
        }

        public Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            if (this.webRequest != null)
            {
                this.webRequest.Abort();
                this.webRequest = null;
            }
            if (this.responseStream != null)
            {
                this.responseStream.Close();
                this.responseStream = null;
            }
            return Task.FromResult(0);
        }

        public OAuthTokenModel GetOAuthTokenCopy()
        {
            return this.token;
        }

        private async Task BackgroundCheck()
        {
            this.webRequest = WebRequest.Create(string.Format("https://widgets.streamloots.com/alerts/{0}/media-stream", this.token.accessToken));
            ((HttpWebRequest)this.webRequest).AllowReadStreamBuffering = false;
            var response = this.webRequest.GetResponse();
            this.responseStream = response.GetResponseStream();

            UTF8Encoding encoder = new UTF8Encoding();
            string cardData = string.Empty;
            var buffer = new byte[100000];
            while (true)
            {
                try
                {
                    while (this.responseStream.CanRead)
                    {
                        int len = this.responseStream.Read(buffer, 0, 100000);
                        if (len > 10)
                        {
                            string text = encoder.GetString(buffer, 0, len);
                            if (!string.IsNullOrEmpty(text))
                            {
                                Logger.Log(LogLevel.Debug, "Streamloots Packet Received: " + text);

                                cardData += text;
                                try
                                {
                                    JObject jobj = JObject.Parse("{ " + cardData + " }");
                                    if (jobj != null && jobj.ContainsKey("data"))
                                    {
                                        cardData = string.Empty;
                                        StreamlootsCardModel card = jobj["data"].ToObject<StreamlootsCardModel>();
                                        if (card != null)
                                        {
                                            UserViewModel user = new UserViewModel(0, card.data.Username);

                                        UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(user.UserName);
                                        if (userModel != null)
                                        {
                                            user = new UserViewModel(userModel);
                                        }

                                            EventCommand command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.StreamlootsCardRedeemed));
                                            if (command != null)
                                            {
                                                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                                specialIdentifiers.Add("streamlootscardname", card.data.cardName);
                                                specialIdentifiers.Add("streamlootscardimage", card.imageUrl);
                                                specialIdentifiers.Add("streamlootscardhasvideo", (!string.IsNullOrEmpty(card.videoUrl)).ToString());
                                                specialIdentifiers.Add("streamlootscardvideo", card.videoUrl);
                                                specialIdentifiers.Add("streamlootscardsound", card.soundUrl);
                                                specialIdentifiers.Add("streamlootsmessage", card.data.Message);
                                                await command.Perform(user, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(LogLevel.Debug, ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.cancellationTokenSource.Dispose();
                    if (this.webRequest != null)
                    {
                        this.webRequest.Abort();
                        this.webRequest = null;
                    }
                    if (this.responseStream != null)
                    {
                        this.responseStream.Close();
                        this.responseStream = null;
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
