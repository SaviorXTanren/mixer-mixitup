using Google.Apis.Requests;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubePartner.v1;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The abstract class in charge of handling RESTful requests against the YouTube APIs.
    /// </summary>
    public abstract class YouTubeServiceBase : OAuthRestServiceBase
    {
        private const string YouTubeRestAPIBaseAddressFormat = "https://www.googleapis.com/youtube/v3/";

        private static readonly IgnorePropertiesResolver requestPropertiesToIgnore = new IgnorePropertiesResolver(new List<string>() { "Service" });

        /// <summary>
        /// The YouTube Live connection.
        /// </summary>
        protected YouTubeConnection connection;

        private string baseAddress;

        /// <summary>
        /// Creates an instance of the YouTubeServiceBase.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public YouTubeServiceBase(YouTubeConnection connection) : this(connection, YouTubeRestAPIBaseAddressFormat) { }

        /// <summary>
        /// Creates an instance of the YouTubeServiceBase.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        /// <param name="baseAddress">The base address to use</param>
        public YouTubeServiceBase(YouTubeConnection connection, string baseAddress)
        {
            Validator.ValidateVariable(connection, "connection");
            this.connection = connection;
            this.baseAddress = baseAddress;
        }

        internal YouTubeServiceBase() : this(YouTubeRestAPIBaseAddressFormat) { }

        internal YouTubeServiceBase(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI for YouTube API-wrapped data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<IEnumerable<T>> GetPagedAsync<T>(string requestUri, int maxResults = 1)
        {
            List<T> results = new List<T>();
            string nextPageToken = null;

            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            queryParameters.Add("maxResults", ((maxResults > 50) ? 50 : maxResults).ToString());

            do
            {
                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    queryParameters["pageToken"] = nextPageToken;
                }
                YouTubePagedResult<T> result = await this.GetAsync<YouTubePagedResult<T>>(requestUri + string.Join("&", queryParameters.Select(kvp => kvp.Key + "=" + kvp.Value)));

                if (result != null)
                {
                    nextPageToken = result.nextPageToken;
                    results.AddRange(result.items);
                }
            } while (results.Count < maxResults && !string.IsNullOrEmpty(nextPageToken));
            return results;
        }

        /// <summary>
        /// Wrapper method for handling calls on YouTube's .NET client API.
        /// </summary>
        /// <param name="func">The function being called</param>
        /// <returns>A typed-result</returns>
        protected async Task<T> YouTubeServiceWrapper<T>(Func<Task<T>> func)
        {
            try
            {
                await this.GetOAuthToken();
                return await func();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return default(T);
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
        /// Logs a YouTube service request.
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        protected void LogRequest<T>(YouTubeBaseServiceRequest<T> request)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Sent: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(request, propertiesToIgnore: requestPropertiesToIgnore));
            }
        }

        /// <summary>
        /// Logs a YouTube service request.
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        protected void LogRequest<T>(YouTubePartnerBaseServiceRequest<T> request)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Sent: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(request, propertiesToIgnore: requestPropertiesToIgnore));
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        protected void LogResponse<T>(YouTubeBaseServiceRequest<T> request, IDirectResponseSchema response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(response));
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        protected void LogResponse<T>(YouTubeBaseServiceRequest<T> request, string response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + response);
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        protected void LogResponse<T>(YouTubePartnerBaseServiceRequest<T> request, IDirectResponseSchema response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(response));
            }
        }
    }
}
