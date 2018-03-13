using MixItUp.Base.Model.Remote;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class RemoteService : WebSocketServerBase, IRemoteService
    {
        private const string BaseAddress = "ws://sockets.mixitupapp.com/Remote?SessionMode={0}&DeviceRole={1}&DeviceID={2}&SessionID={3}&AutoAccept={4}&AccessCode={5}&AuthToken={6}&DeviceInfo={7}";

        public Guid SessionID { get; private set; }
        public Guid ClientID { get; private set; }

        public DateTimeOffset ServerLastSeen { get; private set; }

        public string AccessCode { get; private set; }
        public DateTimeOffset AccessCodeExpiration { get; private set; }

        public bool AutoAcceptClients { get; protected set; }

        public string RequestedClientName { get; private set; }
        public Guid RequestedClientID { get; private set; }
        public DateTimeOffset RequestedClientAuthExpiration { get; private set; }

        public event EventHandler<HeartbeatRemoteMessage> OnHeartbeat;
        public event EventHandler<HeartbeatAckRemoteMessage> OnHeartbeatAck;

        public event EventHandler<BoardRequestRemoteMessage> OnReceiveBoardRequest;
        public event EventHandler<ActionRequestRemoteMessage> OnReceiveActionRequest;

        public static string GetConnectConnectionURL(Guid id, bool autoAccept = false, string authToken = null)
        {
            return RemoteService.GetConnectionURL("CONNECT", "HOST", id, null, autoAccept, null, authToken);
        }

        public static string GetReconnectConnectionURL(Guid id, string sessionID = null, bool autoAccept = false, string authToken = null)
        {
            return RemoteService.GetConnectionURL("RECONNECT", "HOST", id, null, autoAccept, sessionID, authToken);
        }

        private static string GetConnectionURL(string sessionMode, string deviceRole, Guid id, string sessionID = null, bool autoAccept = false, string accessCode = null, string authToken = null, string deviceInfo = null)
        {
            return string.Format(RemoteService.BaseAddress, sessionMode, deviceRole, id.ToString(), sessionID, autoAccept, accessCode, authToken, deviceInfo);
        }

        public RemoteService(string address) : base(address) { }

        public async Task SendAuthClientDeny()
        {
            await Send(new AuthClientDenyRemoteMessage());
        }

        public async Task SendAuthClientGrant(ObservableCollection<RemoteBoardModel> boards)
        {
            await Send(new AuthClientGrantRemoteMessage() { Boards = boards });
        }

        public async Task SendBoardDetail(RemoteBoardModel board)
        {
            await Send(new BoardDetailRemoteMessage() { Board = board });
        }

        public async Task SendActionAck(Guid componentID)
        {
            await Send(new ActionAckRemoteMessage() { ComponentID = componentID });
        }

        protected override async Task PacketReceived(string packet)
        {
            RemoteMessageBase message = JsonConvert.DeserializeObject<RemoteMessageBase>(packet);
            switch (message.Type)
            {
                case MessageType.HEARTBEAT:
                    await ReceiveHeartbeat(packet);
                    break;

                case MessageType.DISCONNECT_REQ:
                    break;

                case MessageType.MESSAGE_FAILED:
                    break;

                case MessageType.SESSION_NEW://Host Receive -> Server Send
                    NewSessionRemoteMessage newSessionPacket = JsonConvert.DeserializeObject<NewSessionRemoteMessage>(packet);
                    this.SessionID = newSessionPacket.SessionID;
                    this.AccessCode = newSessionPacket.AccessCode;
                    this.AccessCodeExpiration = newSessionPacket.Expiration;
                    break;

                case MessageType.ACCESS_CODE_NEW://Host Receive -> Server Send
                    AccessCodeNewRemoteMessage newRemotePacket = JsonConvert.DeserializeObject<AccessCodeNewRemoteMessage>(packet);
                    this.AccessCode = newRemotePacket.AccessCode;
                    this.AccessCodeExpiration = newRemotePacket.Expiration;
                    break;

                case MessageType.AUTH_REQ:
                    AuthRequestRemoteMessage authRequestPacket = JsonConvert.DeserializeObject<AuthRequestRemoteMessage>(packet);
                    this.RequestedClientName = authRequestPacket.DeviceInfo;
                    this.RequestedClientID = authRequestPacket.ClientID;
                    this.RequestedClientAuthExpiration = authRequestPacket.Expiration;
                    break;

                case MessageType.BOARD_REQ:
                    this.SendEvent(packet, this.OnReceiveBoardRequest);
                    break;

                case MessageType.ACTION_REQ:
                    this.SendEvent(packet, this.OnReceiveActionRequest);
                    break;
            }

            await base.PacketReceived(packet);
        }

        private async Task ReceiveHeartbeat(string packet)
        {
            HeartbeatRemoteMessage heartbeat = JsonConvert.DeserializeObject<HeartbeatRemoteMessage>(packet);
            this.ServerLastSeen = DateTimeOffset.UtcNow;

            if (this.OnHeartbeat != null)
            {
                this.OnHeartbeat(this, heartbeat);
            }

            HeartbeatAckRemoteMessage heartbeatAck = new HeartbeatAckRemoteMessage()
            {
                SessionID = SessionID,
                ClientID = ClientID,
            };
            await this.Send(heartbeatAck);

            if (this.OnHeartbeatAck != null)
            {
                this.OnHeartbeatAck(this, heartbeatAck);
            }
        }

        private void SendEvent<T>(string packet, EventHandler<T> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, JsonConvert.DeserializeObject<T>(packet));
            }
        }
    }
}