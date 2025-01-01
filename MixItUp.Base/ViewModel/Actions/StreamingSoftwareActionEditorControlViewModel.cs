using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public class StreamingSoftwareActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.StreamingSoftware; } }

        public IEnumerable<StreamingSoftwareTypeEnum> StreamingSoftwareTypes { get { return EnumHelper.GetEnumList<StreamingSoftwareTypeEnum>(); } }

        public StreamingSoftwareTypeEnum SelectedStreamingSoftwareType
        {
            get { return this.selectedStreamingSoftwareType; }
            set
            {
                this.selectedStreamingSoftwareType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("OBSStudioNotEnabled");
                this.NotifyPropertyChanged("XSplitNotEnabled");
                this.NotifyPropertyChanged("StreamlabsOBSNotEnabled");
                this.NotifyPropertyChanged("CanSpecifySceneName");
            }
        }
        private StreamingSoftwareTypeEnum selectedStreamingSoftwareType;

        public IEnumerable<StreamingSoftwareActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<StreamingSoftwareActionTypeEnum>().OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s)); } }

        public StreamingSoftwareActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowNotSupported");
                this.NotifyPropertyChanged("ShowSceneCollectionGrid");
                this.NotifyPropertyChanged("ShowSceneGrid");
                this.NotifyPropertyChanged("ShowSourceGrid");
                this.NotifyPropertyChanged("CanSpecifySceneName");
                this.NotifyPropertyChanged("ShowTextSourceGrid");
                this.NotifyPropertyChanged("ShowImageSourceGrid");
                this.NotifyPropertyChanged("ShowMediaSourceGrid");
                this.NotifyPropertyChanged("ShowWebBrowserSourceGrid");
                this.NotifyPropertyChanged("ShowSourceDimensionsGrid");
                this.NotifyPropertyChanged("ShowSourceFilterGrid");
            }
        }
        private StreamingSoftwareActionTypeEnum selectedActionType;

        public bool OBSStudioNotEnabled { get { return this.GetCurrentlySelectedStreamingSoftwareType() == StreamingSoftwareTypeEnum.OBSStudio && !ServiceManager.Get<IOBSStudioService>().IsConnected; } }

        public bool XSplitNotEnabled { get { return this.GetCurrentlySelectedStreamingSoftwareType() == StreamingSoftwareTypeEnum.XSplit && !ServiceManager.Get<XSplitService>().IsConnected; } }

        public bool StreamlabsOBSNotEnabled { get { return this.GetCurrentlySelectedStreamingSoftwareType() == StreamingSoftwareTypeEnum.StreamlabsDesktop && !ServiceManager.Get<StreamlabsDesktopService>().IsConnected; } }

        public bool ShowNotSupported
        {
            get
            {
                StreamingSoftwareTypeEnum streamingSoftware = this.GetCurrentlySelectedStreamingSoftwareType();
                if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.StartStopStream)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.SaveReplayBuffer)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.StartStopRecording)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.SceneCollection)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit || streamingSoftware == StreamingSoftwareTypeEnum.StreamlabsDesktop)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.SourceDimensions)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.SourceFilterVisibility)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit || streamingSoftware == StreamingSoftwareTypeEnum.StreamlabsDesktop)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.ImageSource)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                else if (this.SelectedActionType == StreamingSoftwareActionTypeEnum.MediaSource)
                {
                    if (streamingSoftware == StreamingSoftwareTypeEnum.XSplit)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ShowSceneCollectionGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.SceneCollection; } }

        public string SceneCollectionName
        {
            get { return this.sceneCollectionName; }
            set
            {
                this.sceneCollectionName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sceneCollectionName;

        public bool ShowSceneGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.Scene; } }

        public string SceneName
        {
            get { return this.sceneName; }
            set
            {
                this.sceneName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sceneName;

        public bool ShowSourceGrid
        {
            get
            {
                return this.SelectedActionType == StreamingSoftwareActionTypeEnum.SourceVisibility || this.ShowTextSourceGrid || this.ShowImageSourceGrid ||
                    this.ShowMediaSourceGrid || this.ShowWebBrowserSourceGrid || this.ShowSourceDimensionsGrid;
            }
        }

        public bool CanSpecifySceneName
        {
            get
            {
                StreamingSoftwareTypeEnum streamingSoftware = this.GetCurrentlySelectedStreamingSoftwareType();
                if (streamingSoftware == StreamingSoftwareTypeEnum.OBSStudio && this.ShowWebBrowserSourceGrid)
                {
                    this.SceneName = null;
                    return false;
                }
                return true;
            }
        }

        public string SourceName
        {
            get { return this.sourceName; }
            set
            {
                this.sourceName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceName;

        public bool SourceVisible
        {
            get { return this.sourceVisible; }
            set
            {
                this.sourceVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool sourceVisible;

        public bool ShowTextSourceGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.TextSource; } }

        public string SourceText
        {
            get { return this.sourceText; }
            set
            {
                this.sourceText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceText;

        public string SourceTextFilePath
        {
            get { return this.sourceTextFilePath; }
            set
            {
                this.sourceTextFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceTextFilePath;

        public bool ShowImageSourceGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.ImageSource; } }

        public string SourceImageFilePath
        {
            get { return this.sourceImageFilePath; }
            set
            {
                this.sourceImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceImageFilePath;

        public bool ShowMediaSourceGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.MediaSource; } }

        public string SourceMediaFilePath
        {
            get { return this.sourceMediaFilePath; }
            set
            {
                this.sourceMediaFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceMediaFilePath;

        public bool ShowWebBrowserSourceGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.WebBrowserSource; } }

        public string SourceWebPageFilePath
        {
            get { return this.sourceWebPageFilePath; }
            set
            {
                this.sourceWebPageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sourceWebPageFilePath;

        public bool ShowSourceDimensionsGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.SourceDimensions; } }

        public int SourceXPosition
        {
            get { return this.sourceXPosition; }
            set
            {
                this.sourceXPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private int sourceXPosition;

        public int SourceYPosition
        {
            get { return this.sourceYPosition; }
            set
            {
                this.sourceYPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private int sourceYPosition;

        public int SourceRotation
        {
            get { return this.sourceRotation; }
            set
            {
                this.sourceRotation = value;
                this.NotifyPropertyChanged();
            }
        }
        private int sourceRotation;

        public float SourceXScale
        {
            get { return this.sourceXScale; }
            set
            {
                this.sourceXScale = value;
                this.NotifyPropertyChanged();
            }
        }
        private float sourceXScale;

        public float SourceYScale
        {
            get { return this.sourceYScale; }
            set
            {
                this.sourceYScale = value;
                this.NotifyPropertyChanged();
            }
        }
        private float sourceYScale;

        public ICommand SourceGetCurrentDimensionsCommand { get; private set; }

        public bool ShowSourceFilterGrid { get { return this.SelectedActionType == StreamingSoftwareActionTypeEnum.SourceFilterVisibility; } }

        public string FilterName
        {
            get { return this.filterName; }
            set
            {
                this.filterName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filterName;

        public bool FilterVisible
        {
            get { return this.filterVisible; }
            set
            {
                this.filterVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool filterVisible;

        public StreamingSoftwareActionEditorControlViewModel(StreamingSoftwareActionModel action)
            : base(action)
        {
            this.SelectedStreamingSoftwareType = action.StreamingSoftwareType;
            this.SelectedActionType = action.ActionType;
            if (this.ShowSceneCollectionGrid)
            {
                this.SceneCollectionName = action.ItemName;
            }
            else if (this.ShowSceneGrid)
            {
                this.SceneName = action.ItemName;
            }
            else if (this.ShowSourceGrid)
            {
                this.SceneName = action.ParentName;
                this.SourceName = action.ItemName;
                this.SourceVisible = action.Visible;
                if (this.ShowTextSourceGrid)
                {
                    this.SourceText = action.SourceText;
                    this.SourceTextFilePath = action.SourceTextFilePath;
                }
                else if (this.ShowImageSourceGrid)
                {
                    this.SourceImageFilePath = action.SourceURL;
                }
                else if (this.ShowMediaSourceGrid)
                {
                    this.SourceMediaFilePath = action.SourceURL;
                }
                else if (this.ShowWebBrowserSourceGrid)
                {
                    this.SourceWebPageFilePath = action.SourceURL;
                }
                else if (this.ShowSourceDimensionsGrid)
                {
                    this.SourceXPosition = action.SourceDimensions.X;
                    this.SourceYPosition = action.SourceDimensions.Y;
                    this.SourceRotation = action.SourceDimensions.Rotation;
                    this.SourceXScale = action.SourceDimensions.XScale;
                    this.SourceYScale = action.SourceDimensions.YScale;
                }
            }
            else if (this.ShowSourceFilterGrid)
            {
                this.SourceName = action.ParentName;
                this.FilterName = action.ItemName;
                this.FilterVisible = action.Visible;
            }
        }

        public StreamingSoftwareActionEditorControlViewModel() : base() { }

        protected override async Task OnOpenInternal()
        {
            this.SourceGetCurrentDimensionsCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.SourceName))
                {
                    StreamingSoftwareSourceDimensionsModel dimensions = await StreamingSoftwareActionModel.GetSourceDimensions(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName);
                    if (dimensions != null)
                    {
                        this.SourceXPosition = dimensions.X;
                        this.SourceYPosition = dimensions.Y;
                        this.SourceRotation = dimensions.Rotation;
                        this.SourceXScale = dimensions.XScale;
                        this.SourceYScale = dimensions.YScale;
                    }
                }
            });
            await base.OnOpenInternal();
        }

        public override Task<Result> Validate()
        {
            if (this.ShowSceneCollectionGrid)
            {
                if (string.IsNullOrEmpty(this.SceneCollectionName))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingSceneCollection));
                }
            }
            else if (this.ShowSceneGrid)
            {
                if (string.IsNullOrEmpty(this.SceneName))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingScene));
                }
            }
            else if (this.ShowSourceGrid)
            {
                if (string.IsNullOrEmpty(this.SourceName))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingSource));
                }

                if (this.ShowTextSourceGrid)
                {
                    if (string.IsNullOrEmpty(this.SourceTextFilePath))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingTextSourceFilePath));
                    }
                }
                else if (this.ShowImageSourceGrid)
                {
                    if (string.IsNullOrEmpty(this.SourceImageFilePath))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingImageSourceFilePath));
                    }
                }
                else if (this.ShowMediaSourceGrid)
                {
                    if (string.IsNullOrEmpty(this.SourceMediaFilePath))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingMediaSourceFilePath));
                    }
                }
                else if (this.ShowWebBrowserSourceGrid)
                {
                    if (string.IsNullOrEmpty(this.SourceWebPageFilePath))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingWebBrowserSourceFilePath));
                    }
                }
                else if (this.ShowSourceDimensionsGrid)
                {

                }
            }
            else if (this.ShowSourceFilterGrid)
            {
                if (string.IsNullOrEmpty(this.SourceName))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingSource));
                }

                if (string.IsNullOrEmpty(this.FilterName))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.StreamingSoftwareActionMissingFilter));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowSceneCollectionGrid)
            {
                return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateSceneCollectionAction(this.SelectedStreamingSoftwareType, this.SceneCollectionName));
            }
            else if (this.ShowSceneGrid)
            {
                return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateSceneAction(this.SelectedStreamingSoftwareType, this.SceneName));
            }
            else if (this.ShowSourceGrid)
            {
                if (this.ShowTextSourceGrid)
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateTextSourceAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible, this.SourceText, this.SourceTextFilePath));
                }
                else if (this.ShowImageSourceGrid)
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateImageSourceAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible, this.SourceImageFilePath));
                }
                else if (this.ShowMediaSourceGrid)
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateMediaSourceAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible, this.SourceMediaFilePath));
                }
                else if (this.ShowWebBrowserSourceGrid)
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateWebBrowserSourceAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible, this.SourceWebPageFilePath));
                }
                else if (this.ShowSourceDimensionsGrid)
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateSourceDimensionsAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible,
                        new StreamingSoftwareSourceDimensionsModel(this.SourceXPosition, this.SourceYPosition, this.SourceRotation, this.SourceXScale, this.SourceYScale)));
                }
                else
                {
                    return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateSourceVisibilityAction(this.SelectedStreamingSoftwareType, this.SceneName, this.SourceName, this.SourceVisible));
                }
            }
            else if (this.ShowSourceFilterGrid)
            {
                return Task.FromResult<ActionModelBase>(StreamingSoftwareActionModel.CreateSourceFilterVisibilityAction(this.SelectedStreamingSoftwareType, this.SourceName, this.FilterName, this.FilterVisible));
            }
            return Task.FromResult<ActionModelBase>(new StreamingSoftwareActionModel(this.SelectedStreamingSoftwareType, this.SelectedActionType));
        }

        private StreamingSoftwareTypeEnum GetCurrentlySelectedStreamingSoftwareType()
        {
            if (this.SelectedStreamingSoftwareType == StreamingSoftwareTypeEnum.DefaultSetting)
            {
                return ChannelSession.Settings.DefaultStreamingSoftware;
            }
            return this.SelectedStreamingSoftwareType;
        }
    }
}
