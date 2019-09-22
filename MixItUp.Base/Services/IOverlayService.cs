using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class OverlayTextToSpeech
    {
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Voice { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Rate { get; set; }
    }

    public interface IOverlayService
    {
        string Name { get; }
        int Port { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred;

        Task<bool> Initialize();

        Task Disconnect();

        Task<int> TestConnection();

        void StartBatching();

        Task EndBatching();

        Task ShowItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);
        Task UpdateItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);
        Task HideItem(OverlayItemModelBase item);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        void SetLocalFile(string fileID, string filePath);
    }
}
