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
        public const string ReceiveProfilesMethodName = "ReceiveProfiles";
        public const string ReceiveProfileBoardMethodName = "ReceiveProfileBoard";
        public const string ReceiveCommandMethodName = "ReceiveCommand";

        public const string SendProfilesMethodName = "SendProfiles";
        public const string SendProfileBoardMethodName = "SendProfileBoard";
        public const string SendCommandMethodName = "SendCommand";

        private SignalRConnection connection;

        public RemoteServiceBase(SignalRConnection connection)
        {
            this.connection = connection;
        }

        public void ListenForReceiveProfiles(Action<IEnumerable<RemoteProfileModel>> action) { this.connection.Listen(ReceiveProfilesMethodName, action); }

        public void ListenForReceiveProfileBoard(Action<RemoteProfileBoardModel> action) { this.connection.Listen(ReceiveProfileBoardMethodName, action); }

        public void ListenForReceiveCommand(Action<Guid> action) { this.connection.Listen(ReceiveCommandMethodName, action); }

        public async Task SendProfiles(IEnumerable<RemoteProfileModel> profiles) { await this.AsyncWrapper(this.connection.Send(SendProfilesMethodName, profiles)); }

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
        Task SendProfiles(IEnumerable<RemoteProfileModel> profiles);

        Task SendProfileBoard(RemoteProfileBoardModel profileBoard);

        Task SendCommand(Guid commandID);
    }
}
