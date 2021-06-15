using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
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
                this.NotifyPropertyChanged("ShowLineToWrite");
                this.NotifyPropertyChanged("ShowLineToRead");
            }
        }
        private FileActionTypeEnum selectedActionType;

        public bool ShowSaveToFileGrid
        {
            get
            {
                return this.SelectedActionType == FileActionTypeEnum.SaveToFile || this.SelectedActionType == FileActionTypeEnum.AppendToFile ||
                    this.SelectedActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.selectedActionType == FileActionTypeEnum.InsertInFileAtRandomLine;
            }
        }

        public bool ShowReadFromFileGrid { get { return !this.ShowSaveToFileGrid; } }

        public bool ShowLineToWrite { get { return this.SelectedActionType == FileActionTypeEnum.InsertInFileAtSpecificLine; } }

        public bool ShowLineToRead { get { return this.SelectedActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.SelectedActionType == FileActionTypeEnum.RemoveSpecificLineFromFile; } }

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

        public string LineIndex
        {
            get { return this.lineIndex; }
            set
            {
                this.lineIndex = value;
                this.NotifyPropertyChanged();
            }
        }
        private string lineIndex;

        public string TransferText
        {
            get { return this.transferText; }
            set
            {
                this.transferText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string transferText;

        public FileActionEditorControlViewModel(FileActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.FilePath = action.FilePath;
            if (this.ShowSaveToFileGrid)
            {
                this.TransferText = action.TransferText;
                if (this.ShowLineToWrite)
                {
                    this.LineIndex = action.LineIndex;
                }
            }
            else if (this.ShowReadFromFileGrid)
            {
                this.TransferText = action.TransferText;
                if (this.ShowLineToRead)
                {
                    this.LineIndex = action.LineIndex;
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
                if (this.ShowLineToWrite)
                {
                    if (string.IsNullOrEmpty(this.LineIndex))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionMissingLineToWrite));
                    }
                }
            }
            else if (this.ShowReadFromFileGrid)
            {
                if (string.IsNullOrEmpty(this.TransferText) || !SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.TransferText))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionInvalidSpecialIdentifier));
                }

                if (this.ShowLineToRead)
                {
                    if (string.IsNullOrEmpty(this.LineIndex))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionMissingLineToRead));
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new FileActionModel(this.SelectedActionType, this.FilePath, this.TransferText, this.LineIndex));
        }
    }
}
