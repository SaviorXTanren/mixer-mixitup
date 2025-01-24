using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLeaderboardHeaderV3ViewModel : OverlayHeaderV3ViewModelBase
    {
        public OverlayLeaderboardHeaderV3ViewModel() { }

        public OverlayLeaderboardHeaderV3ViewModel(OverlayLeaderboardHeaderV3Model model) : base(model) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLeaderboardHeaderV3Model result = new OverlayLeaderboardHeaderV3Model();
            this.AssignProperties(result);
            return result;
        }
    }

    public class OverlayLeaderboardV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayLeaderboardV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayLeaderboardV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayLeaderboardV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public OverlayLeaderboardHeaderV3ViewModel Header
        {
            get { return this.header; }
            set
            {
                this.header = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayLeaderboardHeaderV3ViewModel header;

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public int TotalToShow
        {
            get { return this.totalToShow; }
            set
            {
                this.totalToShow = value;
                this.NotifyPropertyChanged();
            }
        }
        private int totalToShow;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public IEnumerable<OverlayLeaderboardTypeV3Enum> LeaderboardTypes { get; set; } = EnumHelper.GetEnumList<OverlayLeaderboardTypeV3Enum>();

        public OverlayLeaderboardTypeV3Enum SelectedLeaderboardType
        {
            get { return this.selectedLeaderboardType; }
            set
            {
                this.selectedLeaderboardType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsSelectedLeaderboardTypeConsumable));
                this.NotifyPropertyChanged(nameof(this.IsSelectedLeaderboardTypeTwitchBits));
            }
        }
        private OverlayLeaderboardTypeV3Enum selectedLeaderboardType;

        public bool IsSelectedLeaderboardTypeConsumable { get { return this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.Consumable; } }

        public ObservableCollection<CurrencyModel> Consumables { get; set; } = new ObservableCollection<CurrencyModel>();

        public CurrencyModel SelectedConsumable
        {
            get { return this.selectedConsumable; }
            set
            {
                this.selectedConsumable = value;
                this.NotifyPropertyChanged();
            }
        }
        private CurrencyModel selectedConsumable;

        public bool IsSelectedLeaderboardTypeTwitchBits { get { return this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.TwitchBits; } }

        public IEnumerable<OverlayLeaderboardDateRangeV3Enum> TwitchBitsDataRanges { get; set; } = EnumHelper.GetEnumList<OverlayLeaderboardDateRangeV3Enum>();

        public OverlayLeaderboardDateRangeV3Enum SelectedTwitchBitsDataRange
        {
            get { return this.selectedTwitchBitsDataRange; }
            set
            {
                this.selectedTwitchBitsDataRange = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayLeaderboardDateRangeV3Enum selectedTwitchBitsDataRange;

        public OverlayAnimationV3ViewModel ItemAddedAnimation;
        public OverlayAnimationV3ViewModel ItemRemovedAnimation;

        private Guid consumableID;

        public OverlayLeaderboardV3ViewModel()
            : base(OverlayItemV3Type.Leaderboard)
        {
            this.Header = new OverlayLeaderboardHeaderV3ViewModel();

            this.width = 250;
            this.height = 100;

            this.BorderColor = "Black";
            this.BackgroundColor = "White";

            this.TotalToShow = 5;

            this.SelectedLeaderboardType = OverlayLeaderboardTypeV3Enum.ViewingTime;

            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, new OverlayAnimationV3Model());
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, new OverlayAnimationV3Model());

            this.Initialize();
        }

        public OverlayLeaderboardV3ViewModel(OverlayLeaderboardV3Model item)
            : base(item)
        {
            this.Header = new OverlayLeaderboardHeaderV3ViewModel(item.Header);

            this.height = item.Height;

            this.BorderColor = item.BorderColor;
            this.BackgroundColor = item.BackgroundColor;

            this.TotalToShow = item.TotalToShow;

            this.SelectedLeaderboardType = item.LeaderboardType;
            if (this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.Consumable)
            {
                this.consumableID = item.ConsumableID;
            }
            else if (this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.TwitchBits)
            {
                this.SelectedTwitchBitsDataRange = item.TwitchBitsDataRange;
            }

            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, item.ItemAddedAnimation);
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, item.ItemRemovedAnimation);

            this.Initialize();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayLeaderboardV3Model gameQueue = (OverlayLeaderboardV3Model)widget.Item;

            await gameQueue.ClearLeaderboard();

            List<UserV2ViewModel> users = new List<UserV2ViewModel>();
            foreach (UserV2Model user in await ServiceManager.Get<UserService>().LoadQuantityOfUserData(gameQueue.TotalToShow))
            {
                users.Add(new UserV2ViewModel(user));
            }

            List<Tuple<UserV2ViewModel, long>> queueUsers = new List<Tuple<UserV2ViewModel, long>>();
            long amount = 0;
            foreach (UserV2ViewModel user in users)
            {
                amount += RandomHelper.GenerateRandomNumber(1, 100);
                queueUsers.Insert(0, new Tuple<UserV2ViewModel, long>(user, amount + 1));
            }
            await gameQueue.UpdateLeaderboard(queueUsers);

            await base.TestWidget(widget);
        }

        public override Result Validate()
        {
            if (this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.Consumable)
            {
                if (this.SelectedConsumable == null)
                {
                    return new Result(Resources.OverlayLeaderboardValidConsumableMustBeSelected);
                }
            }

            return base.Validate();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLeaderboardV3Model result = new OverlayLeaderboardV3Model()
            {
                Height = this.height,

                BorderColor = this.BorderColor,
                BackgroundColor = this.BackgroundColor,

                TotalToShow = this.TotalToShow,

                LeaderboardType = this.SelectedLeaderboardType,
                TwitchBitsDataRange = this.SelectedTwitchBitsDataRange,
            };

            this.AssignProperties(result);

            result.Header = (OverlayLeaderboardHeaderV3Model)this.Header.GetItem();

            if (this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.Consumable)
            {
                result.ConsumableID = this.SelectedConsumable.ID;
            }

            result.ItemAddedAnimation = this.ItemAddedAnimation.GetAnimation();
            result.ItemRemovedAnimation = this.ItemRemovedAnimation.GetAnimation();

            return result;
        }

        private void Initialize()
        {
            this.Animations.Add(this.ItemAddedAnimation);
            this.Animations.Add(this.ItemRemovedAnimation);

            this.Header.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };

            foreach (var consumable in ChannelSession.Settings.Currency.OrderBy(c => c.Value.Name))
            {
                this.Consumables.Add(consumable.Value);
            }

            if (this.SelectedLeaderboardType == OverlayLeaderboardTypeV3Enum.Consumable)
            {
                this.SelectedConsumable = this.Consumables.FirstOrDefault(c => c.ID == this.consumableID);
            }
        }
    }
}
