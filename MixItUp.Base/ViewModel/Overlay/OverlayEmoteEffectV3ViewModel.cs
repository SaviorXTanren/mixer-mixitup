using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayEmoteEffectV3ViewModel : OverlayItemV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayEmoteEffectV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayEmoteEffectV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayEmoteEffectV3Model.DefaultJavascript; } }

        public string EmoteText
        {
            get { return this.emoteText; }
            set
            {
                this.emoteText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string emoteText;

        public IEnumerable<OverlayEmoteEffectV3AnimationType> AnimationTypes { get; set; } = EnumHelper.GetEnumList<OverlayEmoteEffectV3AnimationType>();

        public OverlayEmoteEffectV3AnimationType SelectedAnimationType
        {
            get { return this.selectedAnimationType; }
            set
            {
                this.selectedAnimationType = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEmoteEffectV3AnimationType selectedAnimationType;

        public int PerEmoteShown
        {
            get { return this.perEmoteShown; }
            set
            {
                this.perEmoteShown = value > 1 ? value : 1;
                this.NotifyPropertyChanged();

                if (this.MaxAmountShown < this.PerEmoteShown)
                {
                    this.MaxAmountShown = this.PerEmoteShown;
                }
            }
        }
        private int perEmoteShown;
        public int MaxAmountShown
        {
            get { return this.maxAmountShown; }
            set
            {
                this.maxAmountShown = value > this.PerEmoteShown ? value : this.PerEmoteShown;
                this.NotifyPropertyChanged();
            }
        }
        private int maxAmountShown;

        public int EmoteWidth
        {
            get { return this.emoteWidth; }
            set
            {
                this.emoteWidth = value > 1 ? value : 1;
                this.NotifyPropertyChanged();
            }
        }
        private int emoteWidth;

        public int EmoteHeight
        {
            get { return this.emoteHeight; }
            set
            {
                this.emoteHeight = value > 1 ? value : 1;
                this.NotifyPropertyChanged();
            }
        }
        private int emoteHeight;

        public bool AllowURLs
        {
            get { return this.allowURLs; }
            set
            {
                this.allowURLs = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool allowURLs;
        public bool AllowEmoji
        {
            get { return this.allowEmoji; }
            set
            {
                this.allowEmoji = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool allowEmoji;

        public override bool AddPositionedWrappedHTMLCSS { get { return false; } }
        public override bool SupportsStandardActionPositioning { get { return false; } }
        public override bool SupportsStandardActionAnimations { get { return false; } }

        public OverlayEmoteEffectV3ViewModel()
            : base(OverlayItemV3Type.EmoteEffect)
        {
            this.SelectedAnimationType = OverlayEmoteEffectV3AnimationType.Rain;

            this.PerEmoteShown = 1;
            this.MaxAmountShown = 1;

            this.EmoteWidth = 50;
            this.EmoteHeight = 50;
        }

        public OverlayEmoteEffectV3ViewModel(OverlayEmoteEffectV3Model item)
            : base(item)
        {
            this.EmoteText = item.EmoteText;

            this.SelectedAnimationType = item.AnimationType;

            this.PerEmoteShown = item.PerEmoteShown;
            this.MaxAmountShown = item.MaxAmountShown;

            this.emoteWidth = item.EmoteWidth;
            this.emoteHeight = item.EmoteHeight;

            this.AllowURLs = item.AllowURLs;
            this.AllowEmoji = item.AllowEmoji;
        }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.EmoteText))
            {
                return new Result(Resources.OverlayEmoteEffectEmoteTextInvalid);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEmoteEffectV3Model result = new OverlayEmoteEffectV3Model()
            {
                EmoteText = this.EmoteText,

                AnimationType = this.SelectedAnimationType,

                PerEmoteShown = this.PerEmoteShown,
                MaxAmountShown = this.MaxAmountShown,

                EmoteWidth = this.emoteWidth,
                EmoteHeight = this.emoteHeight,

                AllowURLs = this.AllowURLs,
                AllowEmoji = this.AllowEmoji,
            };

            return result;
        }
    }
}
