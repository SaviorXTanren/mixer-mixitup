using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System.Net.Http;

namespace MixItUp.Base.Model.Web
{
    /// <summary>
    /// An exception detailing the rate limiting of an Http REST web request.
    /// </summary>
    public class HttpRateLimitedRestRequestException : HttpRestRequestException
    {
        /// <summary>
        /// The rate limit bucket that is being throttled.
        /// </summary>
        public string RateLimitBucket { get; set; }

        /// <summary>
        /// Partial data from the request, if any.
        /// </summary>
        public object PartialData { get; set; }

        /// <summary>
        /// Creates a new instance of the HttpRateLimitedRestRequestException with a web request response.
        /// </summary>
        /// <param name="response">The response of the rate limited web request</param>
        public HttpRateLimitedRestRequestException(HttpResponseMessage response)
            : base(response)
        {
            this.RateLimitBucket = response.GetHeaderValue(HttpRequestRateLimits.MixerRateLimitResetHeader);
        }
    }
}
