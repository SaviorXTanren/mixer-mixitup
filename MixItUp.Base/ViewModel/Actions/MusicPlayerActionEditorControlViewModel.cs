using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class MusicPlayerActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.MusicPlayer; } }

        public IEnumerable<MusicPlayerActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<MusicPlayerActionTypeEnum>(); } }

        public MusicPlayerActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowVolume));
                this.NotifyPropertyChanged(nameof(this.ShowSearchText));
                this.NotifyPropertyChanged(nameof(this.ShowFolderPath));
            }
        }
        private MusicPlayerActionTypeEnum selectedActionType;

        public bool ShowVolume { get { return this.SelectedActionType == MusicPlayerActionTypeEnum.ChangeVolume; } }

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int volume = 100;

        public bool ShowSearchText { get { return this.SelectedActionType == MusicPlayerActionTypeEnum.PlaySpecificSong; } }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string searchText;

        public bool ShowFolderPath { get { return this.SelectedActionType == MusicPlayerActionTypeEnum.ChangeFolder; } }

        public string FolderPath
        {
            get { return this.folderPath; }
            set
            {
                this.folderPath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string folderPath;

        public MusicPlayerActionEditorControlViewModel(MusicPlayerActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.SelectedActionType == MusicPlayerActionTypeEnum.ChangeVolume)
            {
                this.Volume = action.Volume;
            }
            else if (this.SelectedActionType == MusicPlayerActionTypeEnum.PlaySpecificSong)
            {
                this.SearchText = action.SearchText;
            }
            else if (this.SelectedActionType == MusicPlayerActionTypeEnum.ChangeFolder)
            {
                this.FolderPath = action.FolderPath;
            }
        }

        public MusicPlayerActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowVolume)
            {
                return Task.FromResult<ActionModelBase>(new MusicPlayerActionModel(this.SelectedActionType) { Volume = this.Volume });
            }
            else if (this.ShowSearchText)
            {
                return Task.FromResult<ActionModelBase>(new MusicPlayerActionModel(this.SelectedActionType) { SearchText = this.SearchText });
            }
            else if (this.ShowFolderPath)
            {
                return Task.FromResult<ActionModelBase>(new MusicPlayerActionModel(this.SelectedActionType) { FolderPath = this.FolderPath });
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new MusicPlayerActionModel(this.SelectedActionType));
            }
        }
    }
}
