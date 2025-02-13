using MixItUp.Base.Model.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// Extension methods for the HttpResponseMessage class.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Http response header used for tracking the start of a call.
        /// </summary>
        public const string HttpResponseCallStartTimestampHeaderName = "X-CallSent-Timestamp";
        /// <summary>
        /// Http response header used for tracking the end of a call.
        /// </summary>
        public const string HttpResponseCallEndTimestampHeaderName = "X-CallReceived-Timestamp";
        /// <summary>
        /// Http response header used for tracking the length of a call.
        /// </summary>
        public const string HttpResponseCallLengthTimeSpanHeaderName = "X-CallLength-Milliseconds";

        /// <summary>
        /// Adds the call times to the header of the Http response.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <param name="start">The start timestamp of the call</param>
        /// <param name="end">The end timestamp of the call</param>
        public static void AddCallTimeHeaders(this HttpResponseMessage response, DateTimeOffset start, DateTimeOffset end)
        {
            response.Headers.Add(HttpResponseCallStartTimestampHeaderName, start.ToString());
            response.Headers.Add(HttpResponseCallEndTimestampHeaderName, end.ToString());
            response.Headers.Add(HttpResponseCallLengthTimeSpanHeaderName, (end - start).TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Gets the call length of the Http response.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <returns>The call length</returns>
        public static string GetCallLength(this HttpResponseMessage response)
        {
            string result = response.GetHeaderValue(HttpResponseCallLengthTimeSpanHeaderName);
            if (!string.IsNullOrEmpty(result))
            {
                return result + " ms";
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the first value of the specified header if it exists.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <param name="name">The name of the header</param>
        /// <returns>The first value of the specified header if it exists</returns>
        public static string GetHeaderValue(this HttpResponseMessage response, string name)
        {
            if (response.Headers.Contains(name) && response.Headers.TryGetValues(name, out IEnumerable<string> values))
            {
                return values.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Processes and deserializes the HttpResponse into a type-casted object.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <param name="throwExceptionOnFailure">Throws an exception on a failed request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public static async Task<T> ProcessResponse<T>(this HttpResponseMessage response, bool throwExceptionOnFailure = true)
        {
            return JSONSerializerHelper.DeserializeFromString<T>(await response.ProcessStringResponse(throwExceptionOnFailure));
        }

        /// <summary>
        /// Processes and deserializes the HttpResponse into a JObject.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <param name="throwExceptionOnFailure">Throws an exception on a failed request</param>
        /// <returns>A JObject of the contents of the response</returns>
        public static async Task<JObject> ProcessJObjectResponse(this HttpResponseMessage response, bool throwExceptionOnFailure = true)
        {
            return JObject.Parse(await response.ProcessStringResponse(throwExceptionOnFailure));
        }

        /// <summary>
        /// Processes and deserializes the HttpResponse into a string.
        /// </summary>
        /// <param name="response">The HttpResponse to process</param>
        /// <param name="throwExceptionOnFailure">Throws an exception on a failed request</param>
        /// <returns>A string of the contents of the response</returns>
        public static async Task<string> ProcessStringResponse(this HttpResponseMessage response, bool throwExceptionOnFailure = true)
        {
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                Logger.Log(LogLevel.Debug, $"Rest API Request Complete: {response.RequestMessage.Method.Method} - {response.RequestMessage.RequestUri} - {response.StatusCode} - {response.GetCallLength()}{Environment.NewLine}{result}");
                return result;
            }
            else if (throwExceptionOnFailure)
            {
                HttpRestRequestException ex = new HttpRestRequestException(response);
                Logger.Log(ex);
                throw ex;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
