using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ISocketIOConnection
    {
        event EventHandler OnConnected;
        event EventHandler OnDisconnected;

        Task Connect(string connectionURL);

        Task Disconnect();

        void Listen(string eventString, Action processEvent);
        void Listen(string eventString, Action<object> processEvent);

        void Send(string eventString, object data);
    }
}
