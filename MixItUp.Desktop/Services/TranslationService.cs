using Mixer.Base.Model.OAuth;
using Mixer.Base.Services;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace MixItUp.Desktop.Services
{
    public class TranslationService : RestServiceBase, ITranslationService
    {
        private const int ExpirationLength = 300000;

        private const string TranslatePacketString = "<TranslateArrayRequest><AppId></AppId><From></From><Options><Category xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">general</Category><ContentType xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">text/plain</ContentType><ProfanityAction xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">{2}</ProfanityAction><ReservedFlags xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" /><State xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">0</State><Uri xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">all</Uri><User xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">all</User></Options><Texts><string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{1}</string></Texts><To>{0}</To></TranslateArrayRequest>";

        private const string TranslatedTextStartTag = "<TranslatedText>";
        private const string TranslatedTextEndTag = "</TranslatedText>";

        private OAuthTokenModel token = new OAuthTokenModel();

        public async Task SetAccessToken()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/jwt");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ChannelSession.SecretManager.GetSecret("AzureTranslationKey"));
                    HttpResponseMessage response = await client.PostAsync("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", null);
                    string accessToken = await this.ProcessStringResponse(response);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(accessToken))
                    {
                        this.token = new OAuthTokenModel() { accessToken = accessToken, expiresIn = ExpirationLength };
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task<IEnumerable<CultureInfo>> GetAvailableLanguages()
        {
            List<CultureInfo> languages = new List<CultureInfo>();
            try
            {
                HttpResponseMessage response = await this.GetAsync("https://api.microsofttranslator.com/V2/Http.svc/GetLanguagesForTranslate");
                DataContractSerializer serializer = new DataContractSerializer(typeof(string[]));
                foreach (string language in (string[])serializer.ReadObject(await response.Content.ReadAsStreamAsync()))
                {
                    languages.Add(new CultureInfo(language));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return languages;
        }

        public async Task<string> Translate(CultureInfo language, string text, bool allowProfanity = true)
        {
            try
            {
                string content = string.Format(TranslatePacketString, language.TwoLetterISOLanguageName, HttpUtility.UrlEncode(text), (allowProfanity) ? "NoAction" : "Deleted");
                HttpResponseMessage response = await this.PostAsync("https://api.microsofttranslator.com/V2/Http.svc/TranslateArray", new StringContent(content, Encoding.UTF8, "text/xml"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string translation = await this.ProcessStringResponse(response);
                    translation = translation.Substring(translation.IndexOf(TranslatedTextStartTag) + TranslatedTextStartTag.Length);
                    translation = translation.Substring(0, translation.IndexOf(TranslatedTextEndTag));
                    return translation;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected override string GetBaseAddress() { return "https://api.microsofttranslator.com/V2/Http.svc/"; }

        protected override async Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            if (autoRefreshToken && this.token.ExpirationDateTime < DateTimeOffset.Now)
            {
                await this.SetAccessToken();
            }
            return this.token;
        }

        protected override async Task<HttpClientWrapper> GetHttpClient(bool autoRefreshToken = true)
        {
            HttpClientWrapper client = await base.GetHttpClient(autoRefreshToken);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            return client;
        }
    }
}
