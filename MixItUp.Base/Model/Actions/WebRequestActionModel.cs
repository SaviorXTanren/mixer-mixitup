using MixItUp.Base.Model.Commands;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Actions
{
    public enum WebRequestResponseParseTypeEnum
    {
        PlainText,
        JSONToSpecialIdentifiers
    }

    [DataContract]
    public class WebRequestActionModel : ActionModelBase
    {
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

#pragma warning disable CS0612 // Type or member is obsolete
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
#pragma warning restore CS0612 // Type or member is obsolete

        private WebRequestActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.FileService.FileExists(this.Url))
            {
                await this.ProcessContents(parameters, await ChannelSession.Services.FileService.ReadFile(this.Url));
            }
            else
            {
                using (AdvancedHttpClient httpClient = new AdvancedHttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", $"MixItUp/{Assembly.GetEntryAssembly().GetName().Version.ToString()} (Web call from Mix It Up; https://mixitupapp.com; support@mixitupapp.com)");
                    httpClient.DefaultRequestHeaders.Add("Twitch-UserID", (ChannelSession.TwitchUserNewAPI != null) ? ChannelSession.TwitchUserNewAPI.id : string.Empty);
                    httpClient.DefaultRequestHeaders.Add("Twitch-UserLogin", (ChannelSession.TwitchUserNewAPI != null) ? ChannelSession.TwitchUserNewAPI.login : string.Empty);

                    using (HttpResponseMessage response = await httpClient.GetAsync(await ReplaceStringWithSpecialModifiers(this.Url, parameters, encode: true)))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            await this.ProcessContents(parameters, await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
        }

        private async Task ProcessContents(CommandParametersModel parameters, string webRequestResult)
        {
            if (!string.IsNullOrEmpty(webRequestResult))
            {
                string decodedWebRequestResult = HttpUtility.HtmlDecode(webRequestResult);
                if (this.ResponseType == WebRequestResponseParseTypeEnum.JSONToSpecialIdentifiers)
                {
                    try
                    {
                        if (this.JSONToSpecialIdentifiers != null)
                        {
                            await ProcessJSONToSpecialIdentifiers(webRequestResult, this.JSONToSpecialIdentifiers, parameters);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
                else
                {
                    parameters.SpecialIdentifiers[ResponseSpecialIdentifier] = decodedWebRequestResult;
                }
            }
        }

        public static async Task ProcessJSONToSpecialIdentifiers(string body, Dictionary<string, string> jsonToSpecialIdentifiers, CommandParametersModel parameters)
        {
            JToken jToken = JToken.Parse(body);

            foreach (var kvp in jsonToSpecialIdentifiers)
            {
                string key = await ReplaceStringWithSpecialModifiers(kvp.Key, parameters);
                string[] splits = key.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
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
                        parameters.SpecialIdentifiers[kvp.Value] = await ReplaceStringWithSpecialModifiers(HttpUtility.HtmlDecode(currentToken.ToString()), parameters);
                    }
                }
            }
        }
    }
}
