using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class YouTubeActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.YouTube; } }

        public IEnumerable<YouTubeActionType> ActionTypes { get { return EnumHelper.GetEnumList<YouTubeActionType>(); } }

        public YouTubeActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowTitleDescriptionGrid");
                this.NotifyPropertyChanged("ShowAmountGrid");
            }
        }
        private YouTubeActionType selectedActionType;

        public bool ShowTitleDescriptionGrid
        {
            get
            {
                return this.SelectedActionType == YouTubeActionType.SetTitleDescription;
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                this.NotifyPropertyChanged();
            }
        }
        private string title;

        public string Description
        {
            get { return this.description; }
            set
            {
                this.description = value;
                this.NotifyPropertyChanged();
            }
        }
        private string description;

        public bool ShowAmountGrid
        {
            get
            {
                return this.SelectedActionType == YouTubeActionType.RunAdBreak;
            }
        }

        public int Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int amount;

        public YouTubeActionEditorControlViewModel(YouTubeActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;

            if (this.ShowTitleDescriptionGrid)
            {
                this.Title = action.Title;
                this.Description = action.Description;
            }

            if (this.ShowAmountGrid)
            {
                this.Amount = action.Amount;
            }
        }

        public YouTubeActionEditorControlViewModel() : base() { }

        public override async Task<Result> Validate()
        {
            if (this.ShowTitleDescriptionGrid)
            {
                if (string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.Description))
                {
                    return new Result(MixItUp.Base.Resources.YouTubeActionMissingTitleDescription);
                }
            }

            if (this.ShowAmountGrid)
            {
                if (this.Amount <= 0)
                {
                    return new Result(MixItUp.Base.Resources.YouTubeActionMissingAmount);
                }
            }

            return await base.Validate();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == YouTubeActionType.SetTitleDescription)
            {
                return Task.FromResult<ActionModelBase>(YouTubeActionModel.CreateSetTitleDescriptionAction(this.Title, this.Description));
            }
            else if (this.SelectedActionType == YouTubeActionType.RunAdBreak)
            {
                return Task.FromResult<ActionModelBase>(YouTubeActionModel.CreateAdBreakAction(this.Amount));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}