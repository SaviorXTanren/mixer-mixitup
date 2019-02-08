using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class RemoteServiceBase : IRemoteService
    {
        public const string AuthenticateMethodName = "Authenticate";
        public const string RequestProfilesMethodName = "RequestProfiles";
        public const string SendProfilesMethodName = "SendProfiles";
        public const string RequestProfileBoardMethodName = "RequestProfileBoard";
        public const string SendProfileBoardMethodName = "SendProfileBoard";
        public const string SendCommandMethodName = "SendCommand";

        private SignalRConnection connection;

        public RemoteServiceBase(SignalRConnection connection)
        {
            this.connection = connection;
        }

        public async Task Connect() { await this.connection.Connect(); }

        public void ListenForRequestProfiles(Action action) { this.connection.Listen(RequestProfilesMethodName, action); }

        public void ListenForSendProfiles(Action<IEnumerable<RemoteProfileModel>> action) { this.connection.Listen(SendProfilesMethodName, action); }

        public void ListenForRequestProfileBoard(Action<Guid> action) { this.connection.Listen(RequestProfileBoardMethodName, action); }

        public void ListenForSendProfileBoard(Action<RemoteProfileBoardModel> action) { this.connection.Listen(SendProfileBoardMethodName, action); }

        public void ListenForSendCommand(Action<Guid> action) { this.connection.Listen(SendCommandMethodName, action); }

        public async Task Authenticate(Guid clientID, string secret, string accessToken) { await this.AsyncWrapper(this.connection.Send(AuthenticateMethodName, clientID, secret, accessToken)); }

        public async Task RequestProfiles() { await this.AsyncWrapper(this.connection.Send(RequestProfilesMethodName)); }

        public async Task SendProfiles(IEnumerable<RemoteProfileModel> profiles) { await this.AsyncWrapper(this.connection.Send(SendProfilesMethodName, profiles)); }

        public async Task RequestProfileBoard(Guid id) { await this.AsyncWrapper(this.connection.Send(RequestProfileBoardMethodName, id)); }

        public async Task SendProfileBoard(RemoteProfileBoardModel profileBoard) { await this.AsyncWrapper(this.connection.Send(SendProfileBoardMethodName, profileBoard)); }

        public async Task SendCommand(Guid commandID) { await this.AsyncWrapper(this.connection.Send(SendCommandMethodName, commandID)); }

        private async Task AsyncWrapper(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex) { Logger.Log(ex); }
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
