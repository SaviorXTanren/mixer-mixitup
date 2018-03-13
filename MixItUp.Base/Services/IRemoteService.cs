using MixItUp.Base.Model.Remote;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IRemoteService
    {
        Task<bool> Initialize();

        Task Disconnect();

        Task SendAuthClientDeny();

        Task SendAuthClientGrant(ObservableCollection<RemoteBoardModel> boards);

        Task SendBoardDetail(RemoteBoardModel board);

        Task SendActionAck(Guid componentID);
    }
}
