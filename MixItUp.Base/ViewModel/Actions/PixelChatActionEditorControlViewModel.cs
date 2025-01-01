using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class PixelChatSceneComponentViewModel : UIViewModelBase
    {
        private PixelChatSceneComponentModel component;
        private PixelChatOverlayModel overlay;

        public PixelChatSceneComponentViewModel(PixelChatSceneComponentModel component)
        {
            this.component = component;
        }

        public PixelChatSceneComponentViewModel(PixelChatSceneComponentModel component, PixelChatOverlayModel overlay)
            : this(component)
        {
            this.overlay = overlay;
        }

        public string ID { get { return this.component.ID; } }

        public string Name
        {
            get
            {
                if (this.overlay != null)
                {
                    return this.overlay.Name;
                }
                else
                {
                    return this.component.Name;
                }
            }
        }
    }

    public class PixelChatActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.PixelChat; } }

        public IEnumerable<PixelChatActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<PixelChatActionTypeEnum>(); } }

        public PixelChatActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowScenes");
                this.NotifyPropertyChanged("ShowOverlays");
                this.NotifyPropertyChanged("ShowTargetUsernameGrid");
                this.NotifyPropertyChanged("ShowTimeAmountGrid");

                this.SelectedScene = null;
                this.SelectedSceneComponent = null;
                this.SceneComponents.Clear();

                this.UpdateOverlayList();
            }
        }
        private PixelChatActionTypeEnum selectedActionType;

        public bool ShowScenes
        {
            get
            {
                return this.selectedActionType == PixelChatActionTypeEnum.ShowHideSceneComponent;
            }
        }

        public ObservableCollection<PixelChatSceneModel> Scenes { get; set; } = new ObservableCollection<PixelChatSceneModel>();

        public PixelChatSceneModel SelectedScene
        {
            get { return this.selectedScene; }
            set
            {
                this.selectedScene = value;
                this.NotifyPropertyChanged();

                this.SceneComponents.Clear();
                if (this.SelectedScene != null)
                {
                    foreach (var kvp in this.SelectedScene.components)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value.OverlayID))
                        {
                            PixelChatOverlayModel overlay = this.allOverlays.FirstOrDefault(o => string.Equals(o.id, kvp.Value.OverlayID));
                            if (overlay != null)
                            {
                                this.SceneComponents.Add(new PixelChatSceneComponentViewModel(kvp.Value, overlay));
                            }
                        }
                        else
                        {
                            this.SceneComponents.Add(new PixelChatSceneComponentViewModel(kvp.Value));
                        }
                    }
                }
            }
        }
        private PixelChatSceneModel selectedScene;

        public ObservableCollection<PixelChatSceneComponentViewModel> SceneComponents { get; set; } = new ObservableCollection<PixelChatSceneComponentViewModel>();

        public PixelChatSceneComponentViewModel SelectedSceneComponent
        {
            get { return this.selectedSceneComponent; }
            set
            {
                this.selectedSceneComponent = value;
                this.NotifyPropertyChanged();
            }
        }
        private PixelChatSceneComponentViewModel selectedSceneComponent;

        public bool SceneComponentVisible
        {
            get { return this.sceneComponentVisible; }
            set
            {
                this.sceneComponentVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool sceneComponentVisible;

        public bool ShowOverlays
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown ||
                    this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup || this.SelectedActionType == PixelChatActionTypeEnum.StartStreamathon ||
                    this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCredits ||
                    this.SelectedActionType == PixelChatActionTypeEnum.TriggerGiveaway || this.SelectedActionType == PixelChatActionTypeEnum.AddUserToGiveaway;
            }
        }

        public ObservableCollection<PixelChatOverlayModel> Overlays { get; set; } = new ObservableCollection<PixelChatOverlayModel>();

        public PixelChatOverlayModel SelectedOverlay
        {
            get { return this.selectedOverlay; }
            set
            {
                this.selectedOverlay = value;
                this.NotifyPropertyChanged();
            }
        }
        private PixelChatOverlayModel selectedOverlay;

        public bool ShowTargetUsernameGrid
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout || this.SelectedActionType == PixelChatActionTypeEnum.AddUserToGiveaway;
            }
        }

        public string TargetUsername
        {
            get { return this.targetUsername; }
            set
            {
                this.targetUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string targetUsername;

        public bool ShowTimeAmountGrid
        {
            get
            {
                return this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup ||
                    this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime;
            }
        }

        public string TimeAmount
        {
            get { return this.timeAmount; }
            set
            {
                this.timeAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string timeAmount;

        private string sceneID = null;
        private string sceneComponentID = null;
        private string overlayID = null;

        private List<PixelChatOverlayModel> allOverlays = new List<PixelChatOverlayModel>();

        public PixelChatActionEditorControlViewModel(PixelChatActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;

            if (this.ShowScenes)
            {
                this.sceneID = action.SceneID;
                this.sceneComponentID = action.ComponentID;
                this.SceneComponentVisible = action.SceneComponentVisible;
            }

            if (this.ShowOverlays)
            {
                this.overlayID = action.OverlayID;
            }

            if (this.ShowTargetUsernameGrid)
            {
                this.TargetUsername = action.TargetUsername;
            }

            if (this.ShowTimeAmountGrid)
            {
                this.TimeAmount = action.TimeAmount;
            }
        }

        public PixelChatActionEditorControlViewModel()
            : base()
        {
            this.SelectedActionType = PixelChatActionTypeEnum.ShowHideSceneComponent;
        }

        public override async Task<Result> Validate()
        {
            if (this.ShowScenes)
            {
                if (this.SelectedScene == null || this.SelectedSceneComponent == null)
                {
                    return new Result(MixItUp.Base.Resources.PixelChatActionMissingSceneAndSceneComponent);
                }
            }

            if (this.ShowOverlays && this.SelectedOverlay == null)
            {
                return new Result(MixItUp.Base.Resources.PixelChatActionMissingOverlay);
            }

            if (this.ShowTimeAmountGrid && string.IsNullOrEmpty(this.TimeAmount))
            {
                return new Result(MixItUp.Base.Resources.PixelChatActionMissingTimeAmount);
            }

            return await base.Validate();
        }

        protected override async Task OnOpenInternal()
        {
            if (ServiceManager.Get<PixelChatService>().IsConnected)
            {
                foreach (PixelChatSceneModel scene in (await ServiceManager.Get<PixelChatService>().GetScenes()).OrderBy(o => o.Name))
                {
                    this.Scenes.Add(scene);
                }

                foreach (PixelChatOverlayModel overlay in (await ServiceManager.Get<PixelChatService>().GetOverlays()).OrderBy(o => o.Name))
                {
                    this.allOverlays.Add(overlay);
                }

                this.UpdateOverlayList();

                if (this.ShowScenes)
                {
                    this.SelectedScene = this.Scenes.FirstOrDefault(s => s.id.Equals(this.sceneID));
                    this.SelectedSceneComponent = this.SceneComponents.FirstOrDefault(sc => sc.ID.Equals(this.sceneComponentID));
                }

                if (this.ShowOverlays)
                {
                    this.SelectedOverlay = this.allOverlays.FirstOrDefault(o => o.id.Equals(this.overlayID));
                }
            }
            await base.OnOpenInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowScenes)
            {
                return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateShowHideSceneComponent(this.SelectedScene.id, this.SelectedSceneComponent.ID, this.SceneComponentVisible));
            }
            else if (this.ShowOverlays)
            {
                if (this.ShowTargetUsernameGrid)
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateOverlayTargetUser(this.SelectedActionType, this.SelectedOverlay.id, this.TargetUsername));
                }
                else if (this.ShowTimeAmountGrid)
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateOverlayTimeAmount(this.SelectedActionType, this.SelectedOverlay.id, this.TimeAmount));
                }
                else
                {
                    return Task.FromResult<ActionModelBase>(PixelChatActionModel.CreateBasicOverlay(this.SelectedActionType, this.SelectedOverlay.id));
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        private void UpdateOverlayList()
        {
            this.SelectedOverlay = null;
            this.Overlays.Clear();

            if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerShoutout)
            {
                this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.ShoutoutOverlayType)));
            }
            else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountdown || this.SelectedActionType == PixelChatActionTypeEnum.TriggerCountup)
            {
                this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.TimerOverlayType)));
            }
            else if (this.SelectedActionType == PixelChatActionTypeEnum.StartStreamathon || this.SelectedActionType == PixelChatActionTypeEnum.AddStreamathonTime)
            {
                this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.StreamathonOverlayType)));
            }
            else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerCredits)
            {
                this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.CreditsOverlayType)));
            }
            else if (this.SelectedActionType == PixelChatActionTypeEnum.TriggerGiveaway || this.SelectedActionType == PixelChatActionTypeEnum.AddUserToGiveaway)
            {
                this.Overlays.AddRange(this.allOverlays.Where(o => o.type.Equals(PixelChatOverlayModel.GiveawayOverlayType)));
            }
        }
    }
}
