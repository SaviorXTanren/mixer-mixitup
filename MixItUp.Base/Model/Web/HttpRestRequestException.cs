using MixItUp.Base.Util;
using System;
using System.Net.Http;

namespace MixItUp.Base.Model.Web
{
    /// <summary>
    /// An exception detailing the failure of an Http REST web request.
    /// </summary>
    public class HttpRestRequestException : HttpRequestException
    {
        /// <summary>
        /// The response of the request.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        /// <summary>
        /// Creates a new instance of the HttpRestRequestException.
        /// </summary>
        public HttpRestRequestException() : base() { }

        /// <summary>
        /// Creates a new instance of the HttpRestRequestException with a specified message.
        /// </summary>
        /// <param name="message">The message of the exception</param>
        public HttpRestRequestException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of the HttpRestRequestException with a specified message &amp; inner exception.
        /// </summary>
        /// <param name="message">The message of the exception</param>
        /// <param name="inner">The inner exception</param>
        public HttpRestRequestException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Creates a new instance of the HttpRestRequestException with a web request response.
        /// </summary>
        /// <param name="response">The response of the failing web request</param>
        public HttpRestRequestException(HttpResponseMessage response)
            : this(response.ReasonPhrase)
        {
            this.Response = response;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object</returns>
        public override string ToString()
        {
            return $"{this.Response.RequestMessage.RequestUri} - {this.Response.StatusCode} - {this.Response.ReasonPhrase} - {this.Response.GetCallLength()}" + Environment.NewLine +
                this.Response.Content.ReadAsStringAsync().Result + Environment.NewLine + base.ToString();
        }
    }
}

