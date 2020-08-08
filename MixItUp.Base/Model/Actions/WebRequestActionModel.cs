using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Actions
{
    public enum WebRequestResponseParseTypeEnum
    {
        [Name("Plain Text")]
        PlainText,
        [Name("JSON to Special Identifers")]
        JSONToSpecialIdentifiers
    }

    [DataContract]
    public class WebRequestActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return WebRequestActionModel.asyncSemaphore; } }

        public const string ResponseSpecialIdentifier = "webrequestresult";

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public WebRequestResponseParseTypeEnum ResponseType { get; set; }

        [DataMember]
        public Dictionary<string, string> JSONToSpecialIdentifiers { get; set; }

        public WebRequestActionModel(string url, WebRequestResponseParseTypeEnum responseType)
            : base(ActionTypeEnum.WebRequest)
        {
            this.Url = url;
            this.ResponseType = responseType;
        }

        public WebRequestActionModel(string url, Dictionary<string, string> jsonToSpecialIdentifiers)
            : this(url, WebRequestResponseParseTypeEnum.JSONToSpecialIdentifiers)
        {
            this.JSONToSpecialIdentifiers = jsonToSpecialIdentifiers;
        }

        internal WebRequestActionModel(MixItUp.Base.Actions.WebRequestAction action)
            : base(ActionTypeEnum.WebRequest)
        {
            this.Url = action.Url;
            if (action.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.JSONToSpecialIdentifiers)
            {
                this.ResponseType = WebRequestResponseParseTypeEnum.JSONToSpecialIdentifiers;
                this.JSONToSpecialIdentifiers = action.JSONToSpecialIdentifiers;
            }
            else
            {
                this.ResponseType = WebRequestResponseParseTypeEnum.PlainText;
            }
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"MixItUp/{Assembly.GetEntryAssembly().GetName().Version.ToString()} (Web call from Mix It Up; https://mixitupapp.com; support@mixitupapp.com)");
                httpClient.DefaultRequestHeaders.Add("Twitch-UserID", (ChannelSession.TwitchUserNewAPI != null) ? ChannelSession.TwitchUserNewAPI.id : string.Empty);
                httpClient.DefaultRequestHeaders.Add("Twitch-UserLogin", (ChannelSession.TwitchUserNewAPI != null) ? ChannelSession.TwitchUserNewAPI.login : string.Empty);

                using (HttpResponseMessage response = await httpClient.GetAsync(await this.ReplaceStringWithSpecialModifiers(this.Url, user, platform, arguments, specialIdentifiers, encode: true)))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string webRequestResult = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(webRequestResult))
                        {
                            string decodedWebRequestResult = HttpUtility.HtmlDecode(webRequestResult);
                            if (this.ResponseType == WebRequestResponseParseTypeEnum.JSONToSpecialIdentifiers)
                            {
                                try
                                {
                                    JToken jToken = JToken.Parse(webRequestResult);
                                    if (this.JSONToSpecialIdentifiers != null)
                                    {
                                        foreach (var kvp in this.JSONToSpecialIdentifiers)
                                        {
                                            string[] splits = kvp.Key.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (splits.Count() > 0)
                                            {
                                                JToken currentToken = jToken;
                                                for (int i = 0; i < splits.Count(); i++)
                                                {
                                                    if (currentToken is JObject)
                                                    {
                                                        JObject jobjToken = (JObject)currentToken;
                                                        if (jobjToken.ContainsKey(splits[i]))
                                                        {
                                                            currentToken = jobjToken[splits[i]];
                                                        }
                                                        else
                                                        {
                                                            currentToken = null;
                                                            break;
                                                        }
                                                    }
                                                    else if (currentToken is JArray)
                                                    {
                                                        JArray jarrayToken = (JArray)currentToken;
                                                        if (int.TryParse(splits[i], out int index) && index >= 0 && index < jarrayToken.Count)
                                                        {
                                                            currentToken = jarrayToken[index];
                                                        }
                                                        else
                                                        {
                                                            currentToken = null;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        currentToken = null;
                                                        break;
                                                    }
                                                }

                                                if (currentToken != null)
                                                {
                                                    specialIdentifiers[kvp.Value] = await this.ReplaceStringWithSpecialModifiers(HttpUtility.HtmlDecode(currentToken.ToString()), user, platform, arguments, specialIdentifiers);
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
                            else
                            {
                                specialIdentifiers[ResponseSpecialIdentifier] = decodedWebRequestResult;
                            }
                        }
                    }
                }
            }
        }
    }
}
