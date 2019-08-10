using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.SignalR.Client;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class LocalRemoteServiceBase : RestServiceBase, IRemoteService
    {
        public const string AuthenticateMethodName = "Authenticate";
        public const string RequestProfilesMethodName = "RequestProfiles";
        public const string SendProfilesMethodName = "SendProfiles";
        public const string RequestBoardMethodName = "RequestBoard";
        public const string SendBoardMethodName = "SendBoard";
        public const string SendCommandMethodName = "SendCommand";

        public bool IsConnected { get { return this.signalRConnection.IsConnected(); } }

        public RemoteConnectionAuthenticationTokenModel AuthenticationToken { get; protected set; }

        protected string apiAddress { get; private set; }

        private SignalRConnection signalRConnection;

        public LocalRemoteServiceBase(string apiAddress, SignalRConnection signalRConnection)
        {
            this.apiAddress = apiAddress;
            this.signalRConnection = signalRConnection;
        }

        public abstract Task<bool> InitializeConnection(RemoteConnectionAuthenticationTokenModel connection);

        public abstract Task<RemoteConnectionAuthenticationTokenModel> NewHost(string name);

        public abstract Task<RemoteConnectionShortCodeModel> NewClient(string name);

        public abstract Task<RemoteConnectionAuthenticationTokenModel> ValidateClient(RemoteConnectionShortCodeModel shortCode);

        public abstract Task<RemoteConnectionModel> ApproveClient(RemoteConnectionModel connection, string clientShortCode, bool rememberClient = false);

        public abstract Task<bool> ValidateConnection(RemoteConnectionAuthenticationTokenModel authToken);

        public abstract Task<bool> RemoveClient(RemoteConnectionModel hostConnection, RemoteConnectionModel clientConnection);

        public async Task Connect() { await this.signalRConnection.Connect(); }

        public async Task Disconnect() { await this.signalRConnection.Disconnect(); }

        public void ListenForRequestProfiles(Action<Guid> action) { this.signalRConnection.Listen(RequestProfilesMethodName, action); }

        public void ListenForSendProfiles(Action<IEnumerable<RemoteProfileModel>> action) { this.signalRConnection.Listen(SendProfilesMethodName, action); }

        public void ListenForRequestBoard(Action<Guid, Guid, Guid> action) { this.signalRConnection.Listen(RequestBoardMethodName, action); }

        public void ListenForSendBoard(Action<RemoteBoardModel> action) { this.signalRConnection.Listen(SendBoardMethodName, action); }

        public void ListenForSendCommand(Action<Guid, Guid> action) { this.signalRConnection.Listen(SendCommandMethodName, action); }

        public async Task Authenticate(Guid clientID, string secret, string accessToken) { await this.AsyncWrapper(this.signalRConnection.Send(AuthenticateMethodName, clientID, secret, accessToken)); }

        public async Task RequestProfiles(Guid clientID) { await this.AsyncWrapper(this.signalRConnection.Send(RequestProfilesMethodName, clientID)); }

        public async Task SendProfiles(IEnumerable<RemoteProfileModel> profiles) { await this.AsyncWrapper(this.signalRConnection.Send(SendProfilesMethodName, profiles.ToList())); }

        public async Task RequestBoard(Guid clientID, Guid profileID, Guid boardID) { await this.AsyncWrapper(this.signalRConnection.Send(RequestBoardMethodName, clientID, profileID, boardID)); }

        public async Task SendBoard(RemoteBoardModel profileBoard) { await this.AsyncWrapper(this.signalRConnection.Send(SendBoardMethodName, profileBoard)); }

        public async Task SendCommand(Guid clientID, Guid commandID) { await this.AsyncWrapper(this.signalRConnection.Send(SendCommandMethodName, clientID, commandID)); }

        protected override string GetBaseAddress() { return this.apiAddress; }

        protected override Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true) { return Task.FromResult<OAuthTokenModel>(new OAuthTokenModel()); }

        protected async Task AsyncWrapper(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        protected async Task<T> AsyncWrapper<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex) { Logger.Log(ex); }
            return default(T);
        }
    }

    public interface IRemoteService
    {
        Task Authenticate(Guid clientID, string secret, string accessToken);

        Task RequestProfiles(Guid clientID);

        Task SendProfiles(IEnumerable<RemoteProfileModel> profiles);

        Task RequestBoard(Guid clientID, Guid profileID, Guid boardID);

        Task SendBoard(RemoteBoardModel board);

        Task SendCommand(Guid clientID, Guid commandID);
    }
}
