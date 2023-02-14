using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayEventListV3TypeViewModel : UIViewModelBase
    {
        public OverlayEventListV3Type Type { get; set; }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                this.isSelected = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isSelected;

        public string Name { get { return EnumLocalizationHelper.GetLocalizedName(this.Type); } }

        public OverlayEventListV3TypeViewModel(OverlayEventListV3Type type)
        {
            this.Type = type;
        }

        public override string ToString() { return this.Name; }
    }

    public class OverlayEventListV3ViewModel : OverlayListV3ViewModelBase
    {
        public List<OverlayEventListV3TypeViewModel> EventTypes { get; set; } = new List<OverlayEventListV3TypeViewModel>();

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

        private OverlayEventListV3Model item;

        public OverlayEventListV3ViewModel()
            : base(OverlayItemV3Type.EventList)
        {
            this.AddAnimations(new List<string>() { OverlayEventListV3Model.AddedAnimationName, OverlayEventListV3Model.RemovedAnimationName });
        }

        public OverlayEventListV3ViewModel(OverlayEventListV3Model item)
            : base(item)
        {
            this.item = item;
        }

        protected override async Task OnOpenInternal()
        {
            foreach (OverlayEventListV3Type type in EnumHelper.GetEnumList<OverlayEventListV3Type>())
            {
                this.EventTypes.Add(new OverlayEventListV3TypeViewModel(type));
            }

            if (this.item != null)
            {
                foreach (OverlayEventListV3TypeViewModel type in this.EventTypes)
                {
                    if (this.item.EventTypes.Contains(type.Type))
                    {
                        type.IsSelected = true;
                    }
                }
            }

            await base.OnOpenInternal();
        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {
                if (!this.EventTypes.Any(t => t.IsSelected))
                {
                    return new Result(MixItUp.Base.Resources.OverlayEventListAtLeastOneEventTypeMustBeSelected);
                }
            }

            return result;
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEventListV3Model item = new OverlayEventListV3Model(new HashSet<OverlayEventListV3Type>(this.EventTypes.Where(e => e.IsSelected).Select(e => e.Type)))
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                Text = this.Text,
                FontSize = this.FontSize,
                FontName = this.FontName,
                FontColor = this.FontColor,
                Bold = this.Bold,
                Italics = this.Italics,
                Underline = this.Underline,
                ShadowColor = this.ShadowColor,

                BackgroundColor = this.BackgroundColor,
                BorderColor = this.BorderColor,

                ItemHeight = this.ItemHeight,
                ItemWidth = this.ItemWidth,
                MaxToShow = this.MaxToShow
            };

            foreach (OverlayEventListV3TypeViewModel type in this.EventTypes)
            {
                if (type.IsSelected)
                {
                    item.EventTypes.Add(type.Type);
                }
            }

            return item;
        }
    }
}
