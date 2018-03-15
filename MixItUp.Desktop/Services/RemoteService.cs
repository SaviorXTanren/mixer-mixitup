using Mixer.Base.Clients;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class RemoteService : WebSocketClientBase
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

        public RemoteService()
        {
            this.ClientID = Guid.NewGuid();
        }

        public async Task<bool> Connect()
        {
            this.OnPacketReceivedOccurred -= RemoteService_OnPacketReceivedOccurred;

            this.OnPacketReceivedOccurred += RemoteService_OnPacketReceivedOccurred;
            this.OnHeartbeat += ConnectHeartbeat;

            await this.ConnectInternal(RemoteService.GetConnectConnectionURL(this.ClientID));

            await this.WaitForResponse(() => { return this.Connected; });

            this.OnHeartbeat -= ConnectHeartbeat;

            return this.Connected;
        }

        private void ConnectHeartbeat(object sender, HeartbeatRemoteMessage e)
        {
            this.Connected = this.Authenticated = true;
        }

        public async Task SendAuthClientGrant(ObservableCollection<RemoteBoardModel> boards) { await Send(new AuthClientGrantRemoteMessage() { Boards = boards }); }

        public async Task SendAuthClientDeny() { await Send(new AuthClientDenyRemoteMessage()); }

        public async Task SendBoardDetail(RemoteBoardModel board) { await Send(new BoardDetailRemoteMessage() { Board = board }); }

        public async Task SendActionAck(Guid componentID) { await Send(new ActionAckRemoteMessage() { ItemID = componentID }); }

        private async Task ReceiveHeartbeat(string packet)
        {
            HeartbeatRemoteMessage heartbeat = JsonConvert.DeserializeObject<HeartbeatRemoteMessage>(packet);
            this.ServerLastSeen = DateTimeOffset.Now;

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
        }

        private async void RemoteService_OnPacketReceivedOccurred(object sender, string packet)
        {
            try
            {
                dynamic jsonObject = JsonConvert.DeserializeObject(packet);
                if (jsonObject["Type"] != null)
                {
                    MessageType type = (MessageType)((int)jsonObject["Type"]);
                    switch (type)
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
                            BoardRequestRemoteMessage boardRequestPacket = JsonConvert.DeserializeObject<BoardRequestRemoteMessage>(packet);
                            RemoteBoardModel board = ChannelSession.Settings.RemoteBoards.FirstOrDefault(b => b.ID.Equals(boardRequestPacket.BoardID));
                            if (board != null)
                            {
                                await this.SendBoardDetail(board);
                            }
                            break;

                        case MessageType.ACTION_REQ:
                            ActionRequestRemoteMessage actionRequestPacket = JsonConvert.DeserializeObject<ActionRequestRemoteMessage>(packet);
                            RemoteCommand command = ChannelSession.Settings.RemoteCommands.FirstOrDefault(c => c.ID.Equals(actionRequestPacket.ItemID));
                            if (command != null)
                            {
                                await command.Perform();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
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