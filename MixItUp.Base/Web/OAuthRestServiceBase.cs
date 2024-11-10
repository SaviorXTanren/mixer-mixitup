using MixItUp.Base.Model.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// An OAuth-based rest service
    /// </summary>
    public abstract class OAuthRestServiceBase
    {
        /// <summary>
        /// Invoked when an update for rate limiting has occurred.
        /// </summary>
        public event EventHandler<HttpRequestRateLimits> RateLimitUpdateOccurred = delegate { };

        /// <summary>
        /// Creates an instance of the OAuthRestServiceBase.
        /// </summary>
        public OAuthRestServiceBase() { }

        /// <summary>
        /// Performs a GET REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>A response message of the request</returns>
        protected async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.GetAsync(requestUri);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="throwExceptionOnFailure">Throws an exception on a failed request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> GetAsync<T>(string requestUri, bool throwExceptionOnFailure = true)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.GetAsync<T>(requestUri, throwExceptionOnFailure);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>A JObject of the contents of the response</returns>
        protected async Task<JObject> GetJObjectAsync(string requestUri)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.GetJObjectAsync(requestUri);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>A string of the contents of the response</returns>
        protected async Task<string> GetStringAsync(string requestUri)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.GetStringAsync(requestUri);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="autoRefreshToken">Whether to auto refresh the OAuth token</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> PostAsync<T>(string requestUri, bool autoRefreshToken = true)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient(autoRefreshToken))
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PostAsync<T>(requestUri);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <param name="autoRefreshToken">Whether to auto refresh the OAuth token</param>
        /// <returns>A response message of the request</returns>
        protected async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, bool autoRefreshToken = true)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient(autoRefreshToken))
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PostAsync(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <param name="autoRefreshToken">Whether to auto refresh the OAuth token</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> PostAsync<T>(string requestUri, HttpContent content, bool autoRefreshToken = true)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient(autoRefreshToken))
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PostAsync<T>(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a PUT REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> PutAsync<T>(string requestUri)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PutAsync<T>(requestUri);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a PUT REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>A response message of the request</returns>
        protected async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PutAsync(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a PUT REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> PutAsync<T>(string requestUri, HttpContent content)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PutAsync<T>(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a PATCH REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>A response message of the request</returns>
        protected async Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PatchAsync(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a PATCH REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> PatchAsync<T>(string requestUri, HttpContent content)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.PatchAsync<T>(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a DELETE REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>Whether the deletion was successful</returns>
        protected async Task<bool> DeleteAsync(string requestUri, HttpContent content = null)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.DeleteAsync(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Performs a DELETE REST request using the provided request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The content to send</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        protected async Task<T> DeleteAsync<T>(string requestUri, HttpContent content = null)
        {
            using (AdvancedHttpClient client = await this.GetHttpClient())
            {
                try
                {
                    client.RateLimitUpdateOccurred += Client_RateLimitUpdateOccurred;
                    return await client.DeleteAsync<T>(requestUri, content);
                }
                finally
                {
                    client.RateLimitUpdateOccurred -= Client_RateLimitUpdateOccurred;
                }
            }
        }

        /// <summary>
        /// Gets the OAuth token for the connection of this service.
        /// </summary>
        /// <param name="autoRefreshToken">Whether to automatically refresh the OAuth token or not if it has to be</param>
        /// <returns>The OAuth token for the connection</returns>
        protected abstract Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true);

        /// <summary>
        /// Gets the base address for all RESTful calls for this service.
        /// </summary>
        /// <returns>The base address for all RESTful calls</returns>
        protected abstract string GetBaseAddress();

        /// <summary>
        /// Gets the HttpClient using the OAuth for the connection of this service.
        /// </summary>
        /// <param name="autoRefreshToken">Whether to automatically refresh the OAuth token or not if it has to be</param>
        /// <returns>The HttpClient for the connection</returns>
        protected virtual async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            OAuthTokenModel token = await this.GetOAuthToken(autoRefreshToken);
            if (token != null)
            {
                return new AdvancedHttpClient(this.GetBaseAddress(), token);
            }
            return new AdvancedHttpClient(this.GetBaseAddress());
        }

        private void Client_RateLimitUpdateOccurred(object sender, HttpRequestRateLimits rateLimits)
        {
            this.RateLimitUpdateOccurred(this, rateLimits);
        }
    }
}
