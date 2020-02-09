using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.SignalR.Client;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Services;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IRemoteService
    {
        Task Authenticate(Guid clientID, string secret, string accessToken);

        Task RequestProfiles(Guid clientID);

        Task SendProfiles(IEnumerable<RemoteProfileModel> profiles);

        Task RequestBoard(Guid clientID, Guid profileID, Guid boardID);

        Task SendBoard(RemoteBoardModel board);

        Task SendCommand(Guid clientID, Guid commandID);
    }

    public abstract class LocalRemoteServiceBase : OAuthRestServiceBase, IRemoteService
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

    public class LocalStreamerRemoteService : LocalRemoteServiceBase
    {
        public LocalStreamerRemoteService(string apiAddress, string signalRAddress) : base(apiAddress, new SignalRConnection(signalRAddress)) { }

        public override async Task<bool> InitializeConnection(RemoteConnectionAuthenticationTokenModel connection)
        {
            if (!this.IsConnected)
            {
                if (!await this.ValidateConnection(connection))
                {
                    return false;
                }

                this.AuthenticationToken = connection;

                this.ListenForRequestProfiles(async (clientID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            await this.SendProfiles(ChannelSession.Settings.RemoteProfiles.Where(p => !p.IsStreamer || clientConnection.IsStreamer));
                            ChannelSession.Services.Telemetry.TrackRemoteSendProfiles(connection.ID);
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForRequestBoard(async (clientID, profileID, boardID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            if (ChannelSession.Settings.RemoteProfileBoards.ContainsKey(profileID) && ChannelSession.Settings.RemoteProfileBoards[profileID].Boards.ContainsKey(boardID))
                            {
                                await this.SendBoard(ChannelSession.Settings.RemoteProfileBoards[profileID].Boards[boardID]);
                                ChannelSession.Services.Telemetry.TrackRemoteSendBoard(connection.ID, profileID, boardID);
                            }
                            else
                            {
                                await this.SendBoard(null);
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForSendCommand(async (clientID, commandID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            CommandBase command = ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(commandID));
                            if (command != null)
                            {
                                await command.Perform();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                await this.Connect();
                await this.Authenticate(connection.ID, ChannelSession.Services.Secrets.GetSecret("RemoteHostSecret"), connection.AccessToken);
                await Task.Delay(3000);

                if (this.IsConnected)
                {
                    ChannelSession.Services.Telemetry.TrackRemoteAuthentication(connection.ID);
                }

                return this.IsConnected;
            }
            return true;
        }

        public override async Task<RemoteConnectionAuthenticationTokenModel> NewHost(string name)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                return await this.GetAsync<RemoteConnectionAuthenticationTokenModel>("authentication/newhost?name=" + name);
            });
        }

        public override Task<RemoteConnectionShortCodeModel> NewClient(string name) { throw new System.NotImplementedException(); }

        public override Task<RemoteConnectionAuthenticationTokenModel> ValidateClient(RemoteConnectionShortCodeModel shortCode) { throw new System.NotImplementedException(); }

        public override async Task<RemoteConnectionModel> ApproveClient(RemoteConnectionModel connection, string clientShortCode, bool rememberClient = false)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                return await this.GetAsync<RemoteConnectionAuthenticationTokenModel>(string.Format("authentication/approveclient?hostID={0}&shortCode={1}&rememberClient={2}", connection.ID, clientShortCode, rememberClient));
            });
        }

        public override async Task<bool> ValidateConnection(RemoteConnectionAuthenticationTokenModel authToken)
        {
            return await this.AsyncWrapper<bool>(async () =>
            {
                HttpResponseMessage response = await this.PostAsync("authentication/validateconnection", AdvancedHttpClient.CreateContentFromObject(authToken));
                return response.IsSuccessStatusCode;
            });
        }

        public override async Task<bool> RemoveClient(RemoteConnectionModel hostConnection, RemoteConnectionModel clientConnection)
        {
            return await this.AsyncWrapper<bool>(async () =>
            {
                return await this.GetAsync<bool>(string.Format("authentication/removeclient?hostID={0}&clientID={1}", hostConnection.ID, clientConnection.ID));
            });
        }
    }
}
