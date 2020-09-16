using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class TwitterActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Twitter; } }

        public IEnumerable<TwitterActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<TwitterActionTypeEnum>(); } }

        public TwitterActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowTweetGrid");
                this.NotifyPropertyChanged("ShowUpdateNameGrid");
            }
        }
        private TwitterActionTypeEnum selectedActionType;

        public bool ShowTweetGrid { get { return this.SelectedActionType == TwitterActionTypeEnum.SendTweet; } }

        public string TweetText
        {
            get { return this.tweetText; }
            set
            {
                this.tweetText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string tweetText;

        public string TweetImageURL
        {
            get { return this.tweetImageURL; }
            set
            {
                this.tweetImageURL = value;
                this.NotifyPropertyChanged();
            }
        }
        private string tweetImageURL;

        public bool ShowUpdateNameGrid { get { return this.SelectedActionType == TwitterActionTypeEnum.UpdateName; } }

        public string UpdateNameText
        {
            get { return this.updateNameText; }
            set
            {
                this.updateNameText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string updateNameText;

        public TwitterActionEditorControlViewModel(TwitterActionModel action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowTweetGrid)
            {
                this.TweetText = action.TweetText;
                this.TweetImageURL = action.ImagePath;
            }
            else if (this.ShowUpdateNameGrid)
            {
                this.UpdateNameText = action.NameUpdate;
            }
        }

        public TwitterActionEditorControlViewModel() { }

        public override Task<Result> Validate()
        {
            if (this.ShowTweetGrid)
            {
                if (string.IsNullOrEmpty(this.TweetText))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.TwitterActionMissingTweet));
                }

                if (TwitterActionModel.CheckIfTweetContainsTooManyTags(this.TweetText))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.TwitterActionInvalidTweet));
                }
            }
            else if (this.ShowUpdateNameGrid)
            {
                if (string.IsNullOrEmpty(this.UpdateNameText))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.TwitterActionMissingNameUpdate));
                }
            }
            return Task.FromResult(new Result());
        }

        public override Task<ActionModelBase> GetAction()
        {
            if (this.ShowTweetGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitterActionModel.CreateTweetAction(this.TweetText, this.TweetImageURL));
            }
            else if (this.ShowUpdateNameGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitterActionModel.CreateUpdateProfileNameAction(this.UpdateNameText));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
