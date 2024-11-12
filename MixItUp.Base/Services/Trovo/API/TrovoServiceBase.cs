using MixItUp.Base.Model.Trovo;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// Base class for all Trovo services.
    /// </summary>
    public abstract class TrovoServiceBase : OAuthRestServiceBase
    {
        private const string TrovoRestAPIBaseAddressFormat = "https://open-api.trovo.live/openplatform/";

        /// <summary>
        /// The client ID for the connection.
        /// </summary>
        public string ClientID { get; private set; }

        /// <summary>
        /// The Trovo connection.
        /// </summary>
        protected TrovoConnection connection;

        private string baseAddress;

        /// <summary>
        /// Creates an instance of the TrovoServiceBase.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public TrovoServiceBase(TrovoConnection connection) : this(connection, TrovoRestAPIBaseAddressFormat) { }

        /// <summary>
        /// Creates an instance of the TrovoServiceBase.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        /// <param name="baseAddress">The base address to use</param>
        public TrovoServiceBase(TrovoConnection connection, string baseAddress)
        {
            Validator.ValidateVariable(connection, "connection");
            this.connection = connection;
            this.baseAddress = baseAddress;
            this.ClientID = connection.ClientID;
        }

        internal TrovoServiceBase(string clientID)
        {
            this.baseAddress = TrovoRestAPIBaseAddressFormat;
            this.ClientID = clientID;
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI for paged offset data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<IEnumerable<T>> GetPagedOffsetAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1) where T : PageDataResponseModel
        {
            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            if (maxLimit > 0)
            {
                queryParameters["limit"] = maxLimit.ToString();
            }

            List<T> results = new List<T>();
            int lastCount = -1;
            int totalCount = 0;
            do
            {
                if (totalCount > 0)
                {
                    queryParameters["offset"] = totalCount.ToString();
                }
                T data = await this.GetAsync<T>(requestUri + string.Join("&", queryParameters.Select(kvp => kvp.Key + "=" + kvp.Value)));

                lastCount = -1;
                if (data != null)
                {
                    results.Add(data);
                    lastCount = data.GetItemCount();
                    totalCount += lastCount;
                }
            }
            while (totalCount < maxResults && lastCount > 0 && lastCount < totalCount);

            return results;
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for paged cursor data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <param name="parameters">Optional parameters to include in the request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<IEnumerable<T>> PostPagedTokenAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1, Dictionary<string, object> parameters = null) where T : PageDataResponseModel
        {
            JObject requestParameters = new JObject();
            if (maxLimit > 0)
            {
                requestParameters["limit"] = maxLimit;
            }
            requestParameters["after"] = true;

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    requestParameters[kvp.Key] = kvp.Value.ToString();
                }
            }

            List<T> results = new List<T>();
            string token = null;
            int cursor = -1;
            int count = 0;
            do
            {
                if (!string.IsNullOrEmpty(token) && cursor > 0)
                {
                    requestParameters["token"] = token;
                    requestParameters["cursor"] = cursor;
                }
                T data = await this.PostAsync<T>(requestUri, AdvancedHttpClient.CreateContentFromObject(requestParameters));

                if (data != null)
                {
                    results.Add(data);
                    count += data.GetItemCount();

                    if (data.cursor < data.total_page)
                    {
                        token = data.token;
                        cursor = data.cursor;
                    }
                    else
                    {
                        token = null;
                        cursor = -1;
                    }
                }
            }
            while (count < maxResults && !string.IsNullOrEmpty(token));

            return results;
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for paged cursor data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <param name="parameters">Optional parameters to include in the request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<IEnumerable<T>> PostPagedCursorAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1, Dictionary<string, object> parameters = null) where T : PageDataResponseModel
        {
            JObject requestParameters = new JObject();
            if (maxLimit > 0)
            {
                requestParameters["limit"] = maxLimit;
            }

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    requestParameters[kvp.Key] = new JObject(kvp.Value);
                }
            }

            List<T> results = new List<T>();
            int cursor = -1;
            int count = 0;
            do
            {
                if (cursor > 0)
                {
                    requestParameters["cursor"] = cursor;
                }
                T data = await this.PostAsync<T>(requestUri, AdvancedHttpClient.CreateContentFromObject(requestParameters));

                if (data != null)
                {
                    results.Add(data);
                    count += data.GetItemCount();
                    if (data.cursor < data.total_page)
                    {
                        cursor = data.cursor;
                    }
                    else
                    {
                        cursor = -1;
                    }
                }
            }
            while (count < maxResults && cursor >= 0);

            return results;
        }

        /// <summary>
        /// Gets the OAuth token for the connection of this service.
        /// </summary>
        /// <param name="autoRefreshToken">Whether to automatically refresh the OAuth token or not if it has to be</param>
        /// <returns>The OAuth token for the connection</returns>
        protected override async Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            if (this.connection != null)
            {
                return await this.connection.GetOAuthToken(autoRefreshToken);
            }
            return null;
        }

        /// <summary>
        /// Gets the base address for all RESTful calls for this service.
        /// </summary>
        /// <returns>The base address for all RESTful calls</returns>
        protected override string GetBaseAddress() { return this.baseAddress; }

        /// <summary>
        /// Gets the HttpClient using the OAuth for the connection of this service.
        /// </summary>
        /// <param name="autoRefreshToken">Whether to automatically refresh the OAuth token or not if it has to be</param>
        /// <returns>The HttpClient for the connection</returns>
        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = new AdvancedHttpClient(this.GetBaseAddress());
            OAuthTokenModel token = await this.GetOAuthToken(autoRefreshToken);
            if (token != null)
            {
                client = new AdvancedHttpClient(this.GetBaseAddress());
                client.SetAuthorization("OAuth", token.accessToken);
            }
            client.DefaultRequestHeaders.Add("Client-ID", this.ClientID);
            return client;
        }
    }
}
