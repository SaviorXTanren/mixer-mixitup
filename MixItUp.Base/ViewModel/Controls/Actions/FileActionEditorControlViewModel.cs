using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class FileActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.File; } }

        public IEnumerable<FileActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<FileActionTypeEnum>(); } }

        public FileActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowSaveToFileGrid");
                this.NotifyPropertyChanged("ShowReadFromFileGrid");
                this.NotifyPropertyChanged("ShowLineToRead");
            }
        }
        private FileActionTypeEnum selectedActionType;

        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.filePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filePath;

        public bool ShowSaveToFileGrid { get { return this.SelectedActionType == FileActionTypeEnum.SaveToFile || this.SelectedActionType == FileActionTypeEnum.AppendToFile; } }

        public string TextToSave
        {
            get { return this.textToSave; }
            set
            {
                this.textToSave = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textToSave;

        public bool ShowReadFromFileGrid { get { return !this.ShowSaveToFileGrid; } }

        public string LineToRead
        {
            get { return this.lineToRead; }
            set
            {
                this.lineToRead = value;
                this.NotifyPropertyChanged();
            }
        }
        private string lineToRead;

        public bool ShowLineToRead { get { return this.SelectedActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.SelectedActionType == FileActionTypeEnum.RemoveSpecificLineFromFile; } }

        public string SpecialIdentifier
        {
            get { return this.specialIdentifier; }
            set
            {
                this.specialIdentifier = value;
                this.NotifyPropertyChanged();
            }
        }
        private string specialIdentifier;

        public FileActionEditorControlViewModel(FileActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.FilePath = action.FilePath;
            if (this.ShowSaveToFileGrid)
            {
                this.TextToSave = action.TransferText;
            }
            else if (this.ShowReadFromFileGrid)
            {
                this.SpecialIdentifier = action.TransferText;
                if (this.ShowLineToRead)
                {
                    this.lineToRead = action.LineIndex;
                }
            }
        }

        public FileActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionMissingFilePath));
            }

            if (this.ShowSaveToFileGrid)
            {
                if (string.IsNullOrEmpty(this.TextToSave))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionMissingTextToSave));
                }
            }
            else if (this.ShowReadFromFileGrid)
            {
                if (string.IsNullOrEmpty(this.SpecialIdentifier))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionInvalidSpecialIdentifier));
                }

                if (this.ShowLineToRead)
                {
                    if (string.IsNullOrEmpty(this.LineToRead))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionMissingLineToRead));
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        public override Task<ActionModelBase> GetAction()
        {
            if (this.ShowSaveToFileGrid)
            {
                return Task.FromResult<ActionModelBase>(new FileActionModel(this.SelectedActionType, this.FilePath, this.TextToSave));
            }
            else if (this.ShowReadFromFileGrid)
            {
                return Task.FromResult<ActionModelBase>(new FileActionModel(this.SelectedActionType, this.FilePath, this.SpecialIdentifier, this.LineToRead));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
