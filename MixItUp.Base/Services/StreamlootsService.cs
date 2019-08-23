using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
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
    public class StreamlootsPurchaseModel
    {
        public string type { get; set; }
        public StreamlootsPurchaseDataModel data { get; set; }
    }

    public class StreamlootsPurchaseDataModel
    {
        public List<StreamlotsDataFieldModel> fields { get; set; }

        // This is the person receiving the action (if gifted)
        public string Giftee
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("giftee"));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public int Quantity
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("quantity"));
                return (field != null) ? int.Parse(field.value) : 0;
            }
        }

        // This is the person doing the action (purchase or gifter)
        public string Username
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("username"));
                return (field != null) ? field.value : string.Empty;
            }
        }
    }

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
        public List<StreamlotsDataFieldModel> fields { get; set; }

        public string Message
        {
            get
            {
                StreamlotsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("message"));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public string LongMessage
        {
            get
            {
                StreamlotsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("longMessage"));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public string Username
        {
            get
            {
                StreamlotsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("username"));
                return (field != null) ? field.value : string.Empty;
            }
        }
    }

    public class StreamlotsDataFieldModel
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
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
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
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    this.webRequest = WebRequest.Create(string.Format("https://widgets.streamloots.com/alerts/{0}/media-stream", this.token.accessToken));
                    ((HttpWebRequest)this.webRequest).AllowReadStreamBuffering = false;
                    var response = this.webRequest.GetResponse();
                    this.responseStream = response.GetResponseStream();

                    UTF8Encoding encoder = new UTF8Encoding();
                    string textBuffer = string.Empty;
                    var buffer = new byte[100000];
                    while (!this.cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (this.responseStream.CanRead)
                        {
                            int len = this.responseStream.Read(buffer, 0, 100000);
                            if (len > 10)
                            {
                                string text = encoder.GetString(buffer, 0, len);
                                if (!string.IsNullOrEmpty(text))
                                {
                                    Util.Logger.LogDiagnostic("Streamloots Packet Received: " + text);

                                    textBuffer += text;
                                    try
                                    {
                                        JObject jobj = JObject.Parse("{ " + textBuffer + " }");
                                        if (jobj != null && jobj.ContainsKey("data"))
                                        {
                                            textBuffer = string.Empty;
                                            if (jobj.Value<JObject>("data").ContainsKey("data") && jobj.Value<JObject>("data").Value<JObject>("data").ContainsKey("type"))
                                            {
                                                var type = jobj.Value<JObject>("data").Value<JObject>("data").Value<string>("type");
                                                switch(type.ToLower())
                                                {
                                                    case "purchase":
                                                        await ProcessPurchase(jobj);
                                                        break;
                                                    case "redemption":
                                                        await ProcessCardRedemption(jobj);
                                                        break;
                                                    default:
                                                        Util.Logger.LogDiagnostic($"Unknown Streamloots packet type: {type}");
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logger.LogDiagnostic(ex);
                                    }
                                }
                            }
                        }
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Util.Logger.Log(ex);
                    await Task.Delay(5000);
                }
                finally
                {
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
            }
        }

        private async Task ProcessPurchase(JObject jobj)
        {
            var purchase = jobj["data"].ToObject<StreamlootsPurchaseModel>();
            if (purchase != null)
            {
                UserViewModel user = new UserViewModel(0, purchase.data.Username);
                UserViewModel giftee = (string.IsNullOrEmpty(purchase.data.Giftee)) ? null : new UserViewModel(0, purchase.data.Giftee);

                UserModel userModel = await ChannelSession.Connection.GetUser(user.UserName);
                if (userModel != null)
                {
                    user = new UserViewModel(userModel);
                }

                if (giftee != null)
                {
                    UserModel gifteeModel = await ChannelSession.Connection.GetUser(giftee.UserName);
                    if (gifteeModel != null)
                    {
                        giftee = new UserViewModel(gifteeModel);
                    }
                }

                EventCommand command = null;
                IEnumerable<string> arguments = null;
                if (giftee == null)
                {
                    command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.StreamlootsPackPurchased));
                }
                else
                {
                    command = ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.StreamlootsPackGifted));
                    arguments = new List<string>() { giftee.UserName };
                }

                if (command != null)
                {
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers.Add("streamlootspurchasequantity", purchase.data.Quantity.ToString());

                    await command.Perform(user, arguments, extraSpecialIdentifiers: specialIdentifiers);
                }
            }
        }

        private async Task ProcessCardRedemption(JObject jobj)
        {
            string cardData = string.Empty;
            StreamlootsCardModel card = jobj["data"].ToObject<StreamlootsCardModel>();
            if (card != null)
            {
                UserViewModel user = new UserViewModel(0, card.data.Username);

                UserModel userModel = await ChannelSession.Connection.GetUser(user.UserName);
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

                    string message = card.data.Message;
                    if (string.IsNullOrEmpty(message))
                    {
                        message = card.data.LongMessage;
                    }
                    specialIdentifiers.Add("streamlootsmessage", message);

                    await command.Perform(user, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
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
