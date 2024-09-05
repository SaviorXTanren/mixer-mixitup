using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayGameQueueV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayGameQueueV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayGameQueueV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayGameQueueV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

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

        public OverlayAnimationV3ViewModel ItemAddedAnimation;
        public OverlayAnimationV3ViewModel ItemRemovedAnimation;

        public OverlayGameQueueV3ViewModel()
            : base(OverlayItemV3Type.GameQueue)
        {
            this.width = 250;
            this.height = 50;

            this.BorderColor = "Black";
            this.BackgroundColor = "White";

            this.TotalToShow = 5;

            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, new OverlayAnimationV3Model());
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, new OverlayAnimationV3Model());

            this.Animations.Add(this.ItemAddedAnimation);
            this.Animations.Add(this.ItemRemovedAnimation);
        }

        public OverlayGameQueueV3ViewModel(OverlayGameQueueV3Model item)
            : base(item)
        {
            this.height = item.Height;

            this.BorderColor = item.BorderColor;
            this.BackgroundColor = item.BackgroundColor;

            this.TotalToShow = item.TotalToShow;
            
            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, item.ItemAddedAnimation);
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, item.ItemRemovedAnimation);

            this.Animations.Add(this.ItemAddedAnimation);
            this.Animations.Add(this.ItemRemovedAnimation);
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayGameQueueV3Model gameQueue = (OverlayGameQueueV3Model)widget.Item;

            await gameQueue.ClearGameQueue();

            List<UserV2ViewModel> users = new List<UserV2ViewModel>();
            foreach (UserV2Model user in await ServiceManager.Get<UserService>().LoadQuantityOfUserData(gameQueue.TotalToShow))
            {
                users.Add(new UserV2ViewModel(user));
            }

            List<UserV2ViewModel> queueUsers = new List<UserV2ViewModel>();
            foreach (UserV2ViewModel user in users)
            {
                queueUsers.Insert(0, user);
                await gameQueue.UpdateGameQueue(queueUsers);
                await Task.Delay(3000);
            }

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayGameQueueV3Model result = new OverlayGameQueueV3Model()
            {
                Height = this.height,

                BorderColor = this.BorderColor,
                BackgroundColor = this.BackgroundColor,

                TotalToShow = this.TotalToShow,
            };

            this.AssignProperties(result);

            result.ItemAddedAnimation = this.ItemAddedAnimation.GetAnimation();
            result.ItemRemovedAnimation = this.ItemRemovedAnimation.GetAnimation();

            return result;
        }
    }
}
