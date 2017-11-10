using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Interactive;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class InteractiveClientWrapper : MixerRequestWrapperBase
    {
        public event EventHandler<InteractiveGiveInputModel> OnGiveInput;
        public event EventHandler<InteractiveConnectedSceneModel> OnControlDelete;
        public event EventHandler<InteractiveConnectedSceneModel> OnControlCreate;
        public event EventHandler<InteractiveConnectedSceneCollectionModel> OnSceneUpdate;
        public event EventHandler<Tuple<InteractiveConnectedSceneModel, InteractiveConnectedSceneModel>> OnSceneDelete;
        public event EventHandler<InteractiveConnectedSceneCollectionModel> OnSceneCreate;
        public event EventHandler<InteractiveGroupCollectionModel> OnGroupUpdate;
        public event EventHandler<Tuple<InteractiveGroupModel, InteractiveGroupModel>> OnGroupDelete;
        public event EventHandler<InteractiveGroupCollectionModel> OnGroupCreate;
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantUpdate;
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantJoin;
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantLeave;
        public event EventHandler<InteractiveIssueMemoryWarningModel> OnIssueMemoryWarning;
        public event EventHandler<InteractiveConnectedSceneModel> OnControlUpdate;

        public static LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public InteractiveClient Client { get; private set; }

        public InteractiveClientWrapper(InteractiveClient client)
        {
            this.Client = client;
            if (InteractiveClientWrapper.InteractiveUsers != null)
            {
                InteractiveClientWrapper.InteractiveUsers = new LockedDictionary<string, InteractiveParticipantModel>();
            }

            this.Client.OnParticipantJoin += InteractiveClient_OnParticipantJoin;
            this.Client.OnParticipantUpdate += InteractiveClient_OnParticipantUpdate;
            this.Client.OnParticipantLeave += InteractiveClient_OnParticipantLeave;
            this.Client.OnGiveInput += InteractiveClient_OnGiveInput;
        }

        public async Task<bool> ConnectAndReady() { return await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Ready()); }

        public async Task<bool> CreateGroups(IEnumerable<InteractiveGroupModel> groups) { return await this.RunAsync(this.Client.CreateGroups(groups)); }
        public async Task<InteractiveGroupCollectionModel> GetGroups() { return await this.RunAsync(this.Client.GetGroups()); }
        public async Task<bool> DeleteGroup(InteractiveGroupModel groupToDelete, InteractiveGroupModel groupToReplace) { return await this.RunAsync(this.Client.DeleteGroup(groupToDelete, groupToReplace)); }

        public async Task<InteractiveParticipantCollectionModel> GetAllParticipants() { return await this.RunAsync(this.Client.GetAllParticipants()); }
        public async Task<InteractiveParticipantCollectionModel> UpdateParticipants(IEnumerable<InteractiveParticipantModel> participants) { return await this.RunAsync(this.Client.UpdateParticipants(participants)); }

        public async Task<InteractiveConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task<InteractiveConnectedControlCollectionModel> UpdateControls(InteractiveConnectedSceneModel scene, IEnumerable<InteractiveConnectedButtonControlModel> controls) { return await this.RunAsync(this.Client.UpdateControls(scene, controls)); }

        public async Task<bool> CaptureSparkTransaction(string transactionID) { return await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task Disconnect() { await this.RunAsync(this.Client.Disconnect()); }

        #region Interactive Event Handlers

        private void InteractiveClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e) { this.OnClearMessagesOccurred(sender, e); }

        #endregion Interactive Event Handlers
    }
}
