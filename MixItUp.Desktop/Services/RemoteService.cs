using Mixer.Base.Clients;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class RemoteService : WebSocketClientBase
    {
        private const string BaseAddress = "ws://sockets.mixitupapp.com/Remote?SessionMode={0}&DeviceRole={1}&DeviceID={2}&SessionID={3}&AutoAccept={4}&AccessCode={5}&AuthToken={6}&DeviceInfo={7}";

        public bool Connected { get; private set; }

        public Guid SessionID { get; private set; }

        public DateTimeOffset ServerLastSeen { get; private set; }

        public string AccessCode { get; private set; }
        public DateTimeOffset AccessCodeExpiration { get; private set; }

        public Guid ClientID { get; private set; }
        public string ClientName { get; private set; }
        public DateTimeOffset ClientAuthExpiration { get; private set; }

        public event EventHandler<HeartbeatRemoteMessage> OnHeartbeat;
        public event EventHandler<AuthRequestRemoteMessage> OnAuthRequest;
        public event EventHandler<AccessCodeNewRemoteMessage> OnNewAccessCode;

        public event EventHandler<RemoteBoardModel> OnBoardRequest;
        public event EventHandler<RemoteCommand> OnActionRequest;

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

            this.OnDisconnectOccurred += RemoteService_OnDisconnectOccurred;
        }

        public async Task<bool> Connect()
        {
            return await this.Connect(RemoteService.GetConnectConnectionURL(this.ClientID));
        }

        public override async Task<bool> Connect(string endpoint)
        {
            this.OnHeartbeat += ConnectHeartbeat;

            await base.Connect(endpoint);

            await this.WaitForResponse(() => { return this.Connected; });

            this.OnHeartbeat -= ConnectHeartbeat;

            if (this.Connected)
            {
                this.OnDisconnectOccurred += RemoteService_OnDisconnectOccurred;
            }

            return this.Connected;
        }

        public override async Task Disconnect(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            this.OnDisconnectOccurred -= RemoteService_OnDisconnectOccurred;
            await base.Disconnect(closeStatus);
        }

        private void ConnectHeartbeat(object sender, HeartbeatRemoteMessage e)
        {
            this.Connected = true;
        }

        public async Task SendAuthClientGrant(AuthRequestRemoteMessage authRequest)
        {
            this.ClientID = authRequest.ClientID;
            this.ClientName = authRequest.DeviceInfo;
            this.ClientAuthExpiration = authRequest.Expiration;

            await Send(new AuthClientGrantRemoteMessage() { SessionID = this.SessionID, Boards = ChannelSession.Settings.RemoteBoards.Select(b => b.ToSimpleModel()).ToList() });
        }

        public async Task SendAuthClientDeny() { await Send(new AuthClientDenyRemoteMessage()); }

        public async Task SendBoardDetail(RemoteBoardModel board) { await Send(new BoardDetailRemoteMessage() { Board = board }); }

        public async Task SendActionAck(Guid componentID) { await Send(new ActionAckRemoteMessage() { ItemID = componentID }); }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                if (!string.IsNullOrEmpty(packetJSON))
                {
                    string packet = packetJSON.Substring(0, packetJSON.IndexOf('\0'));

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

                            case MessageType.SESSION_NEW:   //Host Receive -> Server Send
                                NewSessionRemoteMessage newSessionPacket = JsonConvert.DeserializeObject<NewSessionRemoteMessage>(packet);
                                this.SessionID = newSessionPacket.SessionID;
                                this.AccessCode = newSessionPacket.AccessCode;
                                this.AccessCodeExpiration = newSessionPacket.Expiration;
                                break;

                            case MessageType.ACCESS_CODE_NEW:   //Host Receive -> Server Send
                                AccessCodeNewRemoteMessage newRemotePacket = JsonConvert.DeserializeObject<AccessCodeNewRemoteMessage>(packet);
                                this.AccessCode = newRemotePacket.AccessCode;
                                this.AccessCodeExpiration = newRemotePacket.Expiration;
                                this.SendEvent(packet, this.OnNewAccessCode);
                                break;

                            case MessageType.AUTH_REQ:
                                AuthRequestRemoteMessage authRequestPacket = JsonConvert.DeserializeObject<AuthRequestRemoteMessage>(packet);
                                if (ChannelSession.Settings.RemoteSavedDevices.Any(d => d.ID.Equals(authRequestPacket.ClientID)))
                                {
                                    await this.SendAuthClientGrant(authRequestPacket);
                                }
                                else
                                {
                                    this.SendEvent(packet, this.OnAuthRequest);
                                }
                                break;

                            case MessageType.BOARD_REQ:
                                BoardRequestRemoteMessage boardRequestPacket = JsonConvert.DeserializeObject<BoardRequestRemoteMessage>(packet);
                                RemoteBoardModel board = ChannelSession.Settings.RemoteBoards.FirstOrDefault(b => b.ID.Equals(boardRequestPacket.BoardID));
                                if (board != null)
                                {
                                    await this.SendBoardDetail(board);
                                    if (this.OnBoardRequest != null)
                                    {
                                        this.OnBoardRequest(this, board);
                                    }
                                }
                                break;

                            case MessageType.ACTION_REQ:
                                ActionRequestRemoteMessage actionRequestPacket = JsonConvert.DeserializeObject<ActionRequestRemoteMessage>(packet);
                                RemoteCommand command = ChannelSession.Settings.RemoteCommands.FirstOrDefault(c => c.ID.Equals(actionRequestPacket.ItemID));
                                if (command != null)
                                {
                                    await command.Perform();
                                    if (this.OnActionRequest != null)
                                    {
                                        this.OnActionRequest(this, command);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

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

        private async Task Send(RemoteMessageBase message)
        {
            await base.Send(SerializerHelper.SerializeToString(message));
        }

        private void SendEvent<T>(string packet, EventHandler<T> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, JsonConvert.DeserializeObject<T>(packet));
            }
        }

        private void RemoteService_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            this.Connected = false;
        }
    }
}