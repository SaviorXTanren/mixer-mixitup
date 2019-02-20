using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using MixItUp.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class LocalRemoteServiceBase : IRemoteService
    {
        public const string AuthenticateMethodName = "Authenticate";
        public const string RequestProfilesMethodName = "RequestProfiles";
        public const string SendProfilesMethodName = "SendProfiles";
        public const string RequestProfileBoardMethodName = "RequestProfileBoard";
        public const string SendProfileBoardMethodName = "SendProfileBoard";
        public const string SendCommandMethodName = "SendCommand";

        public bool IsConnected { get { return this.signalRConnection.IsConnected(); } }

        protected string apiAddress { get; private set; }

        private SignalRConnection signalRConnection;

        public LocalRemoteServiceBase(string apiAddress, SignalRConnection signalRConnection)
        {
            this.apiAddress = apiAddress;
            this.signalRConnection = signalRConnection;
        }

        public abstract Task InitializeConnection(RemoteConnectionAuthenticationTokenModel connection);

        public abstract Task<RemoteConnectionAuthenticationTokenModel> NewHost(string name);

        public abstract Task<RemoteConnectionShortCodeModel> NewClient(string name);

        public abstract Task<RemoteConnectionAuthenticationTokenModel> ValidateClient(RemoteConnectionShortCodeModel shortCode);

        public abstract Task<RemoteConnectionModel> ApproveClient(RemoteConnectionModel connection, string clientShortCode, bool rememberClient = false);

        public abstract Task<bool> RemoveClient(RemoteConnectionModel hostConnection, RemoteConnectionModel clientConnection);

        public async Task Connect() { await this.signalRConnection.Connect(); }

        public void ListenForRequestProfiles(Action action) { this.signalRConnection.Listen(RequestProfilesMethodName, action); }

        public void ListenForSendProfiles(Action<IEnumerable<RemoteProfileModel>> action) { this.signalRConnection.Listen(SendProfilesMethodName, action); }

        public void ListenForRequestProfileBoard(Action<Guid> action) { this.signalRConnection.Listen(RequestProfileBoardMethodName, action); }

        public void ListenForSendProfileBoard(Action<RemoteProfileBoardModel> action) { this.signalRConnection.Listen(SendProfileBoardMethodName, action); }

        public void ListenForSendCommand(Action<Guid> action) { this.signalRConnection.Listen(SendCommandMethodName, action); }

        public async Task Authenticate(Guid clientID, string secret, string accessToken) { await this.AsyncWrapper(this.signalRConnection.Send(AuthenticateMethodName, clientID, secret, accessToken)); }

        public async Task RequestProfiles() { await this.AsyncWrapper(this.signalRConnection.Send(RequestProfilesMethodName)); }

        public async Task SendProfiles(IEnumerable<RemoteProfileModel> profiles) { await this.AsyncWrapper(this.signalRConnection.Send(SendProfilesMethodName, profiles.ToList())); }

        public async Task RequestProfileBoard(Guid id) { await this.AsyncWrapper(this.signalRConnection.Send(RequestProfileBoardMethodName, id)); }

        public async Task SendProfileBoard(RemoteProfileBoardModel profileBoard) { await this.AsyncWrapper(this.signalRConnection.Send(SendProfileBoardMethodName, profileBoard)); }

        public async Task SendCommand(RemoteCommandItemModel command) { await this.SendCommand(command.CommandID); }

        public async Task SendCommand(Guid commandID) { await this.AsyncWrapper(this.signalRConnection.Send(SendCommandMethodName, commandID)); }

        protected HttpClient GetHttpClient() { return new HttpClient() { BaseAddress = new Uri(this.apiAddress) }; }

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

        Task RequestProfiles();

        Task SendProfiles(IEnumerable<RemoteProfileModel> profiles);

        Task RequestProfileBoard(Guid profileID);

        Task SendProfileBoard(RemoteProfileBoardModel profileBoard);

        Task SendCommand(Guid commandID);
    }
}
