using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class VTubeStudioActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.VTubeStudio; } }

        public bool VTubeStudioConnected { get { return ChannelSession.Services.VTubeStudio.IsConnected; } }
        public bool VTubeStudioNotConnected { get { return !this.VTubeStudioConnected; } }

        public IEnumerable<VTubeStudioActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<VTubeStudioActionTypeEnum>(); } }

        public VTubeStudioActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowLoadModelGrid");
                this.NotifyPropertyChanged("ShowRunHotKeyGrid");
            }
        }
        private VTubeStudioActionTypeEnum selectedActionType;

        public bool ShowLoadModelGrid { get { return this.SelectedActionType == VTubeStudioActionTypeEnum.LoadModel; } }

        public ThreadSafeObservableCollection<VTubeStudioModel> Models { get; set; } = new ThreadSafeObservableCollection<VTubeStudioModel>();

        public VTubeStudioModel SelectedModel
        {
            get { return this.selectedModel; }
            set
            {
                bool updateOccurred = value != this.selectedModel;

                this.selectedModel = value;
                this.NotifyPropertyChanged();

                if (updateOccurred)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.LoadHotKeysForCurrentModel();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }
        private VTubeStudioModel selectedModel;

        public bool ShowRunHotKeyGrid { get { return this.SelectedActionType == VTubeStudioActionTypeEnum.RunHotKey; } }

        public VTubeStudioModel CurrentModel
        {
            get { return this.currentModel; }
            set
            {
                this.currentModel = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CurrentModelDisplayText");
            }
        }
        private VTubeStudioModel currentModel;

        public string CurrentModelDisplayText { get { return MixItUp.Base.Resources.VTubeStudioCurrentModel + ((this.CurrentModel != null) ? this.CurrentModel.modelName : MixItUp.Base.Resources.Unknown); } }

        public ThreadSafeObservableCollection<VTubeStudioHotKey> HotKeys { get; set; } = new ThreadSafeObservableCollection<VTubeStudioHotKey>();

        public VTubeStudioHotKey SelectedHotKey
        {
            get { return this.selectedHotKey; }
            set
            {
                this.selectedHotKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private VTubeStudioHotKey selectedHotKey;

        private string modelID;
        private string hotKeyID;

        public VTubeStudioActionEditorControlViewModel(VTubeStudioActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowLoadModelGrid)
            {
                this.modelID = action.ModelID;
            }
            else if (this.ShowRunHotKeyGrid)
            {
                this.modelID = action.ModelID;
                this.hotKeyID = action.HotKeyID;
            }
        }

        public VTubeStudioActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowLoadModelGrid)
            {
                if (this.SelectedModel == null)
                {
                    if (this.VTubeStudioConnected || string.IsNullOrEmpty(this.modelID))
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.VTubeStudioActionMissingModel));
                    }
                }
            }
            else if (this.ShowRunHotKeyGrid)
            {
                if (this.SelectedHotKey == null)
                {
                    if (this.VTubeStudioConnected)
                    {
                        if (this.CurrentModel != null && string.Equals(this.modelID, this.CurrentModel.modelID))
                        {
                            return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.VTubeStudioActionMissingHotKey));
                        }
                    }
                    else if (string.IsNullOrEmpty(this.modelID))
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.VTubeStudioActionMissingHotKey));
                    }
                }
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowLoadModelGrid)
            {
                return Task.FromResult<ActionModelBase>(VTubeStudioActionModel.CreateForModelLoad(this.SelectedModel?.modelID ?? this.modelID));
            }
            else if (this.ShowRunHotKeyGrid)
            {
                if (this.SelectedHotKey != null)
                {
                    return Task.FromResult<ActionModelBase>(VTubeStudioActionModel.CreateForRunHotKey(this.CurrentModel.modelID, this.SelectedHotKey.hotkeyID));
                }
                else
                {
                    return Task.FromResult<ActionModelBase>(VTubeStudioActionModel.CreateForRunHotKey(this.modelID, this.hotKeyID));
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        protected override async Task OnLoadedInternal()
        {
            if (this.VTubeStudioConnected)
            {
                this.CurrentModel = await ChannelSession.Services.VTubeStudio.GetCurrentModel();

                foreach (VTubeStudioModel model in await ChannelSession.Services.VTubeStudio.GetAllModels())
                {
                    this.Models.Add(model);
                }
                this.SelectedModel = this.Models.FirstOrDefault(m => string.Equals(m.modelID, this.modelID));

                await this.LoadHotKeysForCurrentModel();
                this.SelectedHotKey = this.HotKeys.FirstOrDefault(hk => string.Equals(hk.hotkeyID, this.hotKeyID));
            }
            await base.OnLoadedInternal();
        }

        private async Task LoadHotKeysForCurrentModel()
        {
            this.HotKeys.Clear();
            this.SelectedHotKey = null;

            if (this.SelectedModel != null)
            {
                foreach (VTubeStudioHotKey hotKey in await ChannelSession.Services.VTubeStudio.GetHotKeys(this.SelectedModel.modelID))
                {
                    this.HotKeys.Add(hotKey);
                }
            }
        }
    }
}