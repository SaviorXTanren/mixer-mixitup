using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace MixItUp.Base.Util
{
    public class RequestSenderWebServerBase
    {
        private static object lockObj = new object();

        private string address;
        private Socket socket;

        public RequestSenderWebServerBase(string address)
        {
            this.address = address;
            this.socket = IO.Socket(address);
        }

        public virtual void End()
        {
            this.socket.Close();
        }

        protected void SendData(string type, JObject data)
        {
            lock (lockObj)
            {
                this.socket.Emit(type, data.ToString());
            }
        }
    }
}
