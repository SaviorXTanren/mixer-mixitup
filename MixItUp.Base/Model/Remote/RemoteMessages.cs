using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Remote
{
    public enum MessageType
    {
        MESSAGE_FAILED,
        SESSION_NEW,            //Server->Host
        HEARTBEAT,              //Server->Host/Client
        HEARTBEAT_ACK,          //Host/Client->Server
        DISCONNECT_REQ,         //Server->Host/Client
        DISCONNECT_ACK,         //Host/Client->Server
        ACCESS_CODE_NEW,        //Server->Host
        AUTH_CLIENT_HOLD,       //Server->Client
        AUTH_REQ,               //Sever->Host
        AUTH_CLIENT_EXPIRED,    //Server->Client
        AUTH_CLIENT_DENY,       //Server->Client
        AUTH_CLIENT_GRANT,      //Server->Client
        BOARD_REQ,              //[RELAY]Client->Host
        BOARD_DETAIL,           //[RELAY]Host->Client(s)
        ACTION_REQ,             //[RELAY]Client->Host
        ACTION_ACK,             //[RELAY]Host->Client
    }

    public abstract class RemoteMessageBase
    {
        public DateTimeOffset Timestamp { get; set; }
        public MessageType Type { get; set; }

        public RemoteMessageBase()
        {
            this.Timestamp = DateTimeOffset.Now;
        }
    }

    public class NewSessionRemoteMessage : RemoteMessageBase
    {
        public Guid SessionID { get; set; }
        public string AccessCode { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public NewSessionRemoteMessage()
        {
            this.Type = MessageType.SESSION_NEW;
        }
    }

    public class HeartbeatRemoteMessage : RemoteMessageBase
    {
        public HeartbeatRemoteMessage()
        {
            this.Type = MessageType.HEARTBEAT;
        }
    }

    public class HeartbeatAckRemoteMessage : RemoteMessageBase
    {
        public Guid ClientID { get; set; }
        public Guid SessionID { get; set; }

        public HeartbeatAckRemoteMessage()
        {
            this.Type = MessageType.HEARTBEAT_ACK;
        }
    }

    public class DisconnectRemoteMessage : RemoteMessageBase
    {
        public Guid ID { get; set; }
        public Guid SessionID { get; set; }

        public DisconnectRemoteMessage()
        {
            this.Type = MessageType.DISCONNECT_REQ;
        }
    }

    public class DisconnectAckRemoteMessage : RemoteMessageBase
    {
        public DisconnectAckRemoteMessage()
        {
            this.Type = MessageType.DISCONNECT_ACK;
        }
    }

    public class AccessCodeNewRemoteMessage : RemoteMessageBase
    {
        public Guid SessionID { get; set; }
        public string AccessCode { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public AccessCodeNewRemoteMessage()
        {
            this.Type = MessageType.ACCESS_CODE_NEW;
        }
    }

    public class AuthClientHoldRemoteMessage : RemoteMessageBase
    {
        public AuthClientHoldRemoteMessage()
        {
            this.Type = MessageType.AUTH_CLIENT_HOLD;
        }
    }

    public class AuthRequestRemoteMessage : RemoteMessageBase
    {
        public Guid ClientID { get; set; }
        public string DeviceInfo { get; set; }
        public string AccessCode { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public AuthRequestRemoteMessage()
        {
            this.Type = MessageType.AUTH_REQ;
        }
    }

    public class AuthClientExpiredRemoteMessage : RemoteMessageBase
    {
        public Guid ClientID { get; set; }

        public AuthClientExpiredRemoteMessage()
        {
            this.Type = MessageType.AUTH_CLIENT_EXPIRED;
        }
    }

    public class AuthClientDenyRemoteMessage : RemoteMessageBase
    {
        public AuthClientDenyRemoteMessage()
        {
            this.Type = MessageType.AUTH_CLIENT_DENY;
        }
    }

    public class AuthClientGrantRemoteMessage : RemoteMessageBase
    {
        public Guid SessionID { get; set; }
        public List<RemoteBoardModel> Boards { get; set; }

        public AuthClientGrantRemoteMessage()
        {
            this.Type = MessageType.AUTH_CLIENT_GRANT;
            this.Boards = new List<RemoteBoardModel>();
        }
    }

    public class BoardRequestRemoteMessage : RemoteMessageBase
    {
        public Guid BoardID { get; set; }

        public BoardRequestRemoteMessage()
        {
            this.Type = MessageType.BOARD_REQ;
        }
    }

    public class BoardDetailRemoteMessage : RemoteMessageBase
    {
        public RemoteBoardModel Board { get; set; }

        public BoardDetailRemoteMessage()
        {
            this.Type = MessageType.BOARD_DETAIL;
        }
    }

    public class ActionRequestRemoteMessage : RemoteMessageBase
    {
        public Guid ItemID { get; set; }

        public ActionRequestRemoteMessage()
        {
            this.Type = MessageType.ACTION_REQ;
        }
    }

    public class ActionAckRemoteMessage : RemoteMessageBase
    {
        public Guid ItemID { get; set; }

        public ActionAckRemoteMessage()
        {
            this.Type = MessageType.ACTION_ACK;
        }
    }
}
