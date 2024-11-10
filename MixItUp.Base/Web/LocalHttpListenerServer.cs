using MixItUp.Base.Util;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// An Http listening service for intercepting requests &amp; processing them.
    /// </summary>
    public abstract class LocalHttpListenerServer
    {
        private HttpListener httpListener;

        /// <summary>
        /// Creates a new instance of the LocalHttpListenerServer class with the specified address.
        /// </summary>
        public LocalHttpListenerServer() { }

        /// <summary>
        /// Starts listening for requests.
        /// </summary>
        /// <param name="address">The address to start from</param>
        /// <returns>Whether the listener started successfully</returns>
        public bool Start(string address)
        {
            try
            {
                this.httpListener = new HttpListener();
                this.httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                this.httpListener.Prefixes.Add(address);

                this.httpListener.Start();

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (this.httpListener != null && this.httpListener.IsListening)
                        {
                            try
                            {
                                HttpListenerContext context = this.httpListener.GetContext();
                                Task.Factory.StartNew(async (ctx) =>
                                {
                                    await this.ProcessConnection((HttpListenerContext)ctx);
                                    ((HttpListenerContext)ctx).Response.Close();
                                }, context, TaskCreationOptions.LongRunning);
                            }
                            catch (HttpListenerException) { }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }

                    this.Stop();
                }, TaskCreationOptions.LongRunning);

                return true;
            }
            catch (Exception ex) { Logger.Log(ex); }

            return false;
        }

        /// <summary>
        /// Stops listening for requests.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (this.httpListener != null)
                {
                    this.httpListener.Stop();
                }
            }
            catch (HttpListenerException) { }
            catch (Exception ex) { Logger.Log(ex); }
            this.httpListener = null;
        }

        /// <summary>
        /// Processes an http request.
        /// </summary>
        /// <param name="listenerContext">The context of the request</param>
        /// <returns>An awaitable task to process the request</returns>
        protected abstract Task ProcessConnection(HttpListenerContext listenerContext);

        /// <summary>
        /// Gets a parameter value of the request.
        /// </summary>
        /// <param name="listenerContext">The request context</param>
        /// <param name="parameter">The name of the parameter</param>
        /// <returns>The parameter value of the request</returns>
        protected string GetRequestParameter(HttpListenerContext listenerContext, string parameter)
        {
            if (listenerContext.Request.RawUrl.Contains(parameter))
            {
                string searchString = "?" + parameter + "=";
                int startIndex = listenerContext.Request.RawUrl.IndexOf(searchString);
                if (startIndex < 0)
                {
                    searchString = "&" + parameter + "=";
                    startIndex = listenerContext.Request.RawUrl.IndexOf(searchString);
                    if (startIndex < 0)
                    {
                        searchString = "#" + parameter + "=";
                        startIndex = listenerContext.Request.RawUrl.IndexOf(searchString);
                    }
                }

                if (startIndex >= 0)
                {
                    string token = listenerContext.Request.RawUrl.Substring(startIndex + searchString.Length);

                    int endIndex = token.IndexOf("&");
                    if (endIndex > 0)
                    {
                        token = token.Substring(0, endIndex);
                    }
                    return token;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the content of the request. 
        /// </summary>
        /// <param name="listenerContext">The request context</param>
        /// <returns>The content of the request</returns>
        protected async Task<string> GetRequestContent(HttpListenerContext listenerContext)
        {
            string data = await new StreamReader(listenerContext.Request.InputStream, listenerContext.Request.ContentEncoding).ReadToEndAsync();
            return HttpUtility.UrlDecode(data);
        }

        /// <summary>
        /// Closes the connection of the request.
        /// </summary>
        /// <param name="listenerContext">The request context</param>
        /// <param name="statusCode">The status code to send</param>
        /// <param name="content">The text content to include</param>
        /// <returns></returns>
        protected async Task CloseConnection(HttpListenerContext listenerContext, HttpStatusCode statusCode, string content)
        {
            listenerContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            listenerContext.Response.StatusCode = (int)statusCode;
            listenerContext.Response.StatusDescription = statusCode.ToString();

            byte[] buffer = Encoding.UTF8.GetBytes(content);
            await listenerContext.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
