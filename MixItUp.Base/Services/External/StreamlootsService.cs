using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class StreamlootsPurchaseModel
    {
        public string type { get; set; }
        public StreamlootsPurchaseDataModel data { get; set; }
    }

    public class StreamlootsPurchaseDataModel
    {
        public List<StreamlootsDataFieldModel> fields { get; set; }

        // This is the person receiving the action (if gifted)
        public string Giftee
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("giftee", StringComparison.OrdinalIgnoreCase));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public int Quantity
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("quantity", StringComparison.OrdinalIgnoreCase));
                return (field != null) ? int.Parse(field.value) : 0;
            }
        }

        // This is the person doing the action (purchase or gifter)
        public string Username
        {
            get
            {
                var field = this.fields.FirstOrDefault(f => f.name.Equals("username", StringComparison.OrdinalIgnoreCase));
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
        public List<StreamlootsDataFieldModel> fields { get; set; }

        public string Message
        {
            get
            {
                StreamlootsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("message", StringComparison.OrdinalIgnoreCase));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public string LongMessage
        {
            get
            {
                StreamlootsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("longmessage", StringComparison.OrdinalIgnoreCase));
                return (field != null) ? field.value : string.Empty;
            }
        }

        public string Username
        {
            get
            {
                StreamlootsDataFieldModel field = this.fields.FirstOrDefault(f => f.name.Equals("username", StringComparison.OrdinalIgnoreCase));
                return (field != null) ? field.value : string.Empty;
            }
        }
    }

    public class StreamlootsDataFieldModel
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public interface IStreamlootsService : IOAuthExternalService
    {
        event EventHandler OnStreamlootsConnectionChanged;
    }

    public class StreamlootsService : OAuthExternalServiceBase, IStreamlootsService
    {
        private WebRequest webRequest;
        private Stream responseStream;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public event EventHandler OnStreamlootsConnectionChanged = delegate { };

        public StreamlootsService() : base("") { }

        public override string Name { get { return "Streamloots"; } }

        public override Task<Result> Connect()
        {
            return Task.FromResult(new Result(false));
        }

        public override Task Disconnect()
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

            this.OnStreamlootsConnectionChanged(this, new EventArgs());

            return Task.FromResult(0);
        }

        protected override Task<Result> InitializeInternal()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            this.TrackServiceTelemetry("Streamloots");

            this.OnStreamlootsConnectionChanged(this, new EventArgs());

            return Task.FromResult(new Result());
        }

        protected override Task RefreshOAuthToken() { return Task.FromResult(0); }

        protected override void DisposeInternal()
        {
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
                                    Logger.Log(LogLevel.Debug, "Streamloots Packet Received: " + text);

                                    textBuffer += text;
                                    try
                                    {
                                        JObject jobj = JObject.Parse("{ " + textBuffer + " }");
                                        if (jobj != null && jobj.ContainsKey("data"))
                                        {
                                            Logger.Log(LogLevel.Debug, "Streamloots Full Packet Received: " + textBuffer);

                                            textBuffer = string.Empty;
                                            if (jobj.Value<JObject>("data").ContainsKey("data") && jobj.Value<JObject>("data").Value<JObject>("data").ContainsKey("type"))
                                            {
                                                var type = jobj.Value<JObject>("data").Value<JObject>("data").Value<string>("type");
                                                switch (type.ToLower())
                                                {
                                                    case "purchase":
                                                        await ProcessPurchase(jobj);
                                                        break;
                                                    case "redemption":
                                                        await ProcessCardRedemption(jobj);
                                                        break;
                                                    default:
                                                        Logger.Log(LogLevel.Debug, $"Unknown Streamloots packet type: {type}");
                                                        break;
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
                        }
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
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
                UserViewModel user = this.GetUser(purchase.data.Username);
                UserViewModel giftee = (string.IsNullOrEmpty(purchase.data.Giftee)) ? null : this.GetUser(purchase.data.Giftee);

                EventTrigger trigger = new EventTrigger(EventTypeEnum.StreamlootsPackPurchased, user);
                trigger.SpecialIdentifiers["streamlootspurchasequantity"] = purchase.data.Quantity.ToString();
                if (giftee != null)
                {
                    trigger.Type = EventTypeEnum.StreamlootsPackGifted;
                    trigger.Arguments.Add(giftee.Username);
                }
                await ChannelSession.Services.Events.PerformEvent(trigger);

                GlobalEvents.StreamlootsPurchaseOccurred(new Tuple<UserViewModel, int>(user, purchase.data.Quantity));

                if (giftee != null)
                {
                    await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(user.Platform, user, string.Format("{0} Gifted {1} Pack(s) to {2}", user.FullDisplayName, purchase.data.Quantity, giftee.Username), ChannelSession.Settings.AlertStreamlootsColor));
                }
                else
                {
                    await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(user.Platform, user, string.Format("{0} Purchases {1} Pack(s)", user.FullDisplayName, purchase.data.Quantity), ChannelSession.Settings.AlertStreamlootsColor));
                }
            }
        }

        private async Task ProcessCardRedemption(JObject jobj)
        {
            string cardData = string.Empty;
            StreamlootsCardModel card = jobj["data"].ToObject<StreamlootsCardModel>();
            if (card != null && !string.IsNullOrEmpty(card.data?.cardName))
            {
                UserViewModel user = this.GetUser(card.data.Username);

                Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                eventCommandSpecialIdentifiers["streamlootscardname"] = card.data.cardName;
                eventCommandSpecialIdentifiers["streamlootscardimage"] = card.imageUrl;
                eventCommandSpecialIdentifiers["streamlootscardhasvideo"] = (!string.IsNullOrEmpty(card.videoUrl)).ToString();
                eventCommandSpecialIdentifiers["streamlootscardvideo"] = card.videoUrl;
                eventCommandSpecialIdentifiers["streamlootscardsound"] = card.soundUrl;

                string message = card.data.Message;
                if (string.IsNullOrEmpty(message))
                {
                    message = card.data.LongMessage;
                }
                eventCommandSpecialIdentifiers["streamlootsmessage"] = message;

                List<string> arguments = new List<string>();
                if (!string.IsNullOrEmpty(message))
                {
                    arguments = new List<string>(message.Split(' '));
                }

                EventTrigger trigger = new EventTrigger(EventTypeEnum.StreamlootsCardRedeemed, user);
                trigger.Arguments = arguments;
                trigger.SpecialIdentifiers = eventCommandSpecialIdentifiers;
                await ChannelSession.Services.Events.PerformEvent(trigger);

                StreamlootsCardCommandModel command = ChannelSession.Services.Command.StreamlootsCardCommands.FirstOrDefault(c => string.Equals(c.Name, card.data.cardName, StringComparison.CurrentCultureIgnoreCase));
                if (command != null)
                {
                    Dictionary<string, string> cardsCommandSpecialIdentifiers = new Dictionary<string, string>(eventCommandSpecialIdentifiers);
                    await ChannelSession.Services.Command.Queue(command, new CommandParametersModel(user, platform: user.Platform, arguments: arguments, specialIdentifiers: cardsCommandSpecialIdentifiers));
                }

                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(user.Platform, user, string.Format("{0} Redeemed {1} Card", user.FullDisplayName, card.data.cardName), ChannelSession.Settings.AlertStreamlootsColor));
            }
        }

        private UserViewModel GetUser(string username)
        {
            UserViewModel user = ChannelSession.Services.User.GetActiveUserByUsername(username);
            if (user == null)
            {
                user = UserViewModel.Create(username);
            }
            return user;
        }
    }
}
