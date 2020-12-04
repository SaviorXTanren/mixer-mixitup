using MixItUp.Base.Model.Actions;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TwitchActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Twitch; } }

        public IEnumerable<TwitchActionType> ActionTypes { get { return EnumHelper.GetEnumList<TwitchActionType>(); } }

        public TwitchActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowUsernameGrid");
                this.NotifyPropertyChanged("ShowAdGrid");
                this.NotifyPropertyChanged("ShowClipsGrid");
            }
        }
        private TwitchActionType selectedActionType;

        public bool ShowUsernameGrid
        {
            get
            {
                return this.SelectedActionType == TwitchActionType.Host || this.SelectedActionType == TwitchActionType.Raid ||
                    this.SelectedActionType == TwitchActionType.VIPUser || this.SelectedActionType == TwitchActionType.UnVIPUser;
            }
        }

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;

        public bool ShowAdGrid { get { return this.SelectedActionType == TwitchActionType.RunAd; } }

        public IEnumerable<int> AdLengths { get { return TwitchActionModel.SupportedAdLengths; } }

        public int SelectedAdLength
        {
            get { return this.selectedAdLength; }
            set
            {
                this.selectedAdLength = value;
                this.NotifyPropertyChanged();
            }
        }
        private int selectedAdLength = TwitchActionModel.SupportedAdLengths.FirstOrDefault();

        public bool ShowClipsGrid { get { return this.SelectedActionType == TwitchActionType.Clip; } }

        public bool ClipIncludeDelay
        {
            get { return this.clipIncludeDelay; }
            set
            {
                this.clipIncludeDelay = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool clipIncludeDelay;


        public bool ClipShowInfoInChat
        {
            get { return this.clipShowInfoInChat; }
            set
            {
                this.clipShowInfoInChat = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool clipShowInfoInChat;

        public TwitchActionEditorControlViewModel(TwitchActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowUsernameGrid)
            {
                this.Username = action.Username;
            }
            else if (this.ShowAdGrid)
            {
                this.SelectedAdLength = action.AdLength;
            }
            else if (this.ShowClipsGrid)
            {
                this.ClipIncludeDelay = action.ClipIncludeDelay;
                this.ClipShowInfoInChat = action.ClipShowInfoInChat;
            }
        }

        public TwitchActionEditorControlViewModel() : base() { }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowUsernameGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateUserAction(this.SelectedActionType, this.Username));
            }
            else if (this.ShowAdGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateAdAction(this.SelectedAdLength));
            }
            else if (this.ShowClipsGrid)
            {
                return Task.FromResult<ActionModelBase>(TwitchActionModel.CreateClipAction(this.ClipIncludeDelay, this.ClipShowInfoInChat));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
