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
                this.NotifyPropertyChanged(nameof(this.ShowSaveToFileGrid));
                this.NotifyPropertyChanged(nameof(this.ShowReadFromFileGrid));
                this.NotifyPropertyChanged(nameof(this.ShowLineToWrite));
                this.NotifyPropertyChanged(nameof(this.ShowLineToRead));
                this.NotifyPropertyChanged(nameof(this.ShowTextToRemoveGrid));
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

        public bool ShowReadFromFileGrid
        {
            get
            {
                return this.SelectedActionType == FileActionTypeEnum.ReadFromFile || this.SelectedActionType == FileActionTypeEnum.ReadSpecificLineFromFile ||
                    this.SelectedActionType == FileActionTypeEnum.ReadRandomLineFromFile || this.SelectedActionType == FileActionTypeEnum.RemoveSpecificLineFromFile ||
                    this.SelectedActionType == FileActionTypeEnum.RemoveRandomLineFromFile;
            }
        }

        public bool ShowTextToRemoveGrid { get { return this.SelectedActionType == FileActionTypeEnum.RemoveSpecificTextFromFile; } }

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

        public bool CaseSensitive
        {
            get { return this.caseSensitive; }
            set
            {
                this.caseSensitive = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool caseSensitive;

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
            else if (this.ShowTextToRemoveGrid)
            {
                this.TransferText = action.TransferText;
                this.CaseSensitive = action.CaseSensitive;
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
            else if (this.ShowTextToRemoveGrid)
            {
                if (string.IsNullOrEmpty(this.TransferText))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.FileActionInvalidSpecialIdentifier));
                }
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new FileActionModel(this.SelectedActionType, this.FilePath, this.TransferText, this.LineIndex, this.CaseSensitive));
        }
    }
}
