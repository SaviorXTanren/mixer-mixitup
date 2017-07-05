using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace MixItUp.Base.Web
{
    public abstract class RESTWebServer
    {
        private HttpListener listener;
        private Thread listenerThread;

        public RESTWebServer()
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add("http://localhost:8000/");
            this.listener.Prefixes.Add("http://127.0.0.1:8000/");
            this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        }

        public void Start()
        {
            this.listener.Start();
            this.listenerThread = new Thread(new ParameterizedThreadStart(this.Listen));
            this.listenerThread.Start();
        }

        protected virtual HttpStatusCode RequestReceived(string data, out string result)
        {
            result = string.Empty;
            return HttpStatusCode.OK;
        }

        private void Listen(object s)
        {
            while (true)
            {
                var result = this.listener.BeginGetContext(this.ListenerCallback, this.listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            Thread.Sleep(1000);
            var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            var cleanedData = HttpUtility.UrlDecode(data_text);

            string streamResult = string.Empty;
            HttpStatusCode code = this.RequestReceived(cleanedData, out streamResult);

            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.StatusCode = (int)code;
            context.Response.StatusDescription = code.ToString();

            byte[] buffer = Encoding.UTF8.GetBytes(streamResult);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            context.Response.Close();
        }
    }
}
