using Mixer.Base.Clients;
using Mixer.Base.Model.Interactive;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class InteractiveClientWrapper : MixerRequestWrapperBase
    {
        public InteractiveClient Client { get; private set; }

        public InteractiveClientWrapper(InteractiveClient client)
        {
            this.Client = client;
        }

        public async Task<bool> ConnectAndReady() { return await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Ready()); }

        public async Task<InteractiveConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task<InteractiveParticipantCollectionModel> GetAllParticipants() { return await this.RunAsync(this.Client.GetAllParticipants()); }

        public async Task<InteractiveConnectedControlCollectionModel> UpdateControls(InteractiveConnectedSceneModel scene, IEnumerable<InteractiveConnectedButtonControlModel> controls) { return await this.RunAsync(this.Client.UpdateControls(scene, controls)); }

        public async Task<bool> CaptureSparkTransaction(string transactionID) { return await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task Disconnect() { await this.RunAsync(this.Client.Disconnect()); }
    }
}
