using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class InfiniteAlbumActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public IEnumerable<InfiniteAlbumActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<InfiniteAlbumActionTypeEnum>(); } }

        public InfiniteAlbumActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(SelectedCollection));

                this.SelectedCommand = this.SelectedCollection.FirstOrDefault();
            }
        }
        private InfiniteAlbumActionTypeEnum selectedActionType = InfiniteAlbumActionTypeEnum.Styles;

        public InfiniteAlbumCommand SelectedCommand
        {
            get { return this.selectedCommand; }
            set
            {
                this.selectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private InfiniteAlbumCommand selectedCommand;

        public ObservableCollection<InfiniteAlbumCommand> SelectedCollection
        {
            get
            {
                switch (this.SelectedActionType)
                {
                    case InfiniteAlbumActionTypeEnum.Styles: return this.Styles;
                    case InfiniteAlbumActionTypeEnum.Emotions: return this.Emotions;
                    case InfiniteAlbumActionTypeEnum.Instruments: return this.Instruments;
                    case InfiniteAlbumActionTypeEnum.SoundEffects: return this.SoundEffects;
                }

                return null;
            }
        }

        public override ActionTypeEnum Type { get { return ActionTypeEnum.InfiniteAlbum; } }

        public bool InfiniteAlbumConnected { get { return ServiceManager.Get<InfiniteAlbumService>().IsConnected; } }
        public bool InfiniteAlbumNotConnected { get { return !this.InfiniteAlbumConnected; } }

        public ObservableCollection<InfiniteAlbumCommand> Styles { get; set; } = new ObservableCollection<InfiniteAlbumCommand>();
        public ObservableCollection<InfiniteAlbumCommand> Emotions { get; set; } = new ObservableCollection<InfiniteAlbumCommand>();
        public ObservableCollection<InfiniteAlbumCommand> Instruments { get; set; } = new ObservableCollection<InfiniteAlbumCommand>();
        public ObservableCollection<InfiniteAlbumCommand> SoundEffects { get; set; } = new ObservableCollection<InfiniteAlbumCommand>();

        public InfiniteAlbumActionEditorControlViewModel(InfiniteAlbumActionModel action)
            : base(action)
        {
            this.selectedActionType = action.ActionType;
            this.selectedCommand = action.Command;
        }

        public InfiniteAlbumActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.SelectedCommand == null)
            {
                return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.InfiniteAlbumActionMissingCommand));
            }

            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(InfiniteAlbumActionModel.Create(this.SelectedActionType, this.SelectedCommand));
        }

        protected override async Task OnOpenInternal()
        {
            if (ChannelSession.Settings.InfiniteAlbumOAuthToken != null && !this.InfiniteAlbumConnected)
            {
                Result result = await ServiceManager.Get<InfiniteAlbumService>().Connect(ChannelSession.Settings.InfiniteAlbumOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (this.InfiniteAlbumConnected)
            {
                Dictionary<string, List<InfiniteAlbumCommand>> config = await ServiceManager.Get<InfiniteAlbumService>().GetCommands();

                Styles.ClearAndAddRange(config["styles"]);
                Emotions.ClearAndAddRange(config["emotions"]);
                Instruments.ClearAndAddRange(config["instruments"]);
                SoundEffects.ClearAndAddRange(config["soundEffects"]);

                this.NotifyPropertyChanged(nameof(SelectedCollection));

                InfiniteAlbumCommand foundCommand = this.SelectedCollection.FirstOrDefault(
                    c => c.Type == this.selectedCommand?.Type &&
                         c.Data == this.selectedCommand?.Data &&
                         c.Category == this.selectedCommand?.Category);
                this.selectedCommand = foundCommand ?? this.SelectedCollection.FirstOrDefault();

            }

            await base.OnOpenInternal();
        }
    }
}