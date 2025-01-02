using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;

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

        [Obsolete]
        public WebRequestActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string url = await ReplaceStringWithSpecialModifiers(this.Url, parameters);
            if (ServiceManager.Get<IFileService>().FileExists(url))
            {
                await this.ProcessContents(parameters, await ServiceManager.Get<IFileService>().ReadFile(url));
            }
            else
            {
                using (AdvancedHttpClient httpClient = new AdvancedHttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", $"MixItUp/{Assembly.GetEntryAssembly().GetName().Version.ToString()} (Web call from Mix It Up; https://mixitupapp.com; support@mixitupapp.com)");
                    httpClient.DefaultRequestHeaders.Add("Twitch-UserID", ServiceManager.Get<TwitchSession>()?.StreamerID ?? string.Empty);
                    httpClient.DefaultRequestHeaders.Add("Twitch-UserLogin", ServiceManager.Get<TwitchSession>().StreamerUsername ?? string.Empty);
                    httpClient.DefaultRequestHeaders.Add("YouTube-UserID", ServiceManager.Get<YouTubeSession>()?.StreamerID ?? string.Empty);
                    httpClient.DefaultRequestHeaders.Add("YouTube-UserLogin", Uri.EscapeDataString(ServiceManager.Get<YouTubeSession>().StreamerUsername ?? string.Empty));
                    httpClient.DefaultRequestHeaders.Add("Trovo-UserID", ServiceManager.Get<TrovoSession>()?.StreamerID ?? string.Empty);
                    httpClient.DefaultRequestHeaders.Add("Trovo-UserLogin", ServiceManager.Get<TrovoSession>().StreamerUsername ?? string.Empty);

                    string targetUrl = await ReplaceStringWithSpecialModifiers(this.Url, parameters, encode: true);
                    if (!Uri.IsWellFormedUriString(targetUrl, UriKind.RelativeOrAbsolute))
                    {
                        targetUrl = await ReplaceStringWithSpecialModifiers(this.Url, parameters);
                    }

                    using (HttpResponseMessage response = await httpClient.GetAsync(targetUrl))
                    {
                        if (string.Equals(response?.Content?.Headers?.ContentType?.CharSet, "utf8"))
                        {
                            response.Content.Headers.ContentType.CharSet = "utf-8";
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            await this.ProcessContents(parameters, await response.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            string body = string.Empty;
                            try
                            {
                                body = await response.Content.ReadAsStringAsync();
                            }
                            catch { }

                            Logger.Log(LogLevel.Error, $"{nameof(WebRequestActionModel)}: Failed to call '{targetUrl}'. Status code: {response.StatusCode}");
                            Logger.Log(LogLevel.Error, $"Response Body: {body}");
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
            try
            {
                JToken jToken = JToken.Parse(body);

                foreach (var kvp in jsonToSpecialIdentifiers)
                {
                    try
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
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
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
