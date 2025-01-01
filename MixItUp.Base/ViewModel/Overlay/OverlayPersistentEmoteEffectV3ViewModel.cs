using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayPersistentEmoteEffectV3ViewModel : OverlayItemV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayPersistentEmoteEffectV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayPersistentEmoteEffectV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayPersistentEmoteEffectV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

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

        public int Duration
        {
            get { return this.duration; }
            set
            {
                this.duration = value > 1 ? value : 1;
                this.NotifyPropertyChanged();
            }
        }
        private int duration = 1;

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

        public int PerEmoteShown
        {
            get { return this.perEmoteShown; }
            set
            {
                this.perEmoteShown = value > 1 ? value : 1;
                this.NotifyPropertyChanged();
            }
        }
        private int perEmoteShown;

        public int ComboCount
        {
            get { return this.comboCount; }
            set
            {
                this.comboCount = value > 0 ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int comboCount;

        public int ComboTimeframe
        {
            get { return this.comboTimeframe; }
            set
            {
                this.comboTimeframe = value > 0 ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int comboTimeframe;

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

        public bool IgnoreDuplicates
        {
            get { return this.ignoreDuplicates; }
            set
            {
                this.ignoreDuplicates = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool ignoreDuplicates;

        public override bool AddPositionedWrappedHTMLCSS { get { return false; } }
        public override bool SupportsStandardActionPositioning { get { return false; } }
        public override bool SupportsStandardActionAnimations { get { return false; } }

        public OverlayPersistentEmoteEffectV3ViewModel()
            : base(OverlayItemV3Type.PersistentEmoteEffect)
        {
            this.SelectedAnimationType = OverlayEmoteEffectV3AnimationType.Rain;

            this.EmoteWidth = 50;
            this.EmoteHeight = 50;

            this.PerEmoteShown = 1;

            this.ComboCount = 0;
            this.ComboTimeframe = 0;

            this.IgnoreDuplicates = true;
        }

        public OverlayPersistentEmoteEffectV3ViewModel(OverlayPersistentEmoteEffectV3Model item)
            : base(item)
        {
            this.SelectedAnimationType = item.AnimationType;

            this.Duration = item.Duration;
            this.emoteWidth = item.EmoteWidth;
            this.emoteHeight = item.EmoteHeight;

            this.PerEmoteShown = item.PerEmoteShown;

            this.ComboCount = item.ComboCount;
            this.ComboTimeframe = item.ComboTimeframe;

            this.AllowEmoji = item.AllowEmoji;

            this.IgnoreDuplicates = item.IgnoreDuplicates;
        }

        public override Result Validate()
        {
            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayPersistentEmoteEffectV3Model emoteEffect = (OverlayPersistentEmoteEffectV3Model)widget.Item;

            await emoteEffect.ShowEmote(OverlayEmoteEffectV3Model.EmojiURLPrefix + "😀", 1);

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayPersistentEmoteEffectV3Model result = new OverlayPersistentEmoteEffectV3Model()
            {
                AnimationType = this.SelectedAnimationType,

                Duration = this.Duration,
                EmoteWidth = this.emoteWidth,
                EmoteHeight = this.emoteHeight,

                PerEmoteShown = this.PerEmoteShown,

                ComboCount = this.ComboCount,
                ComboTimeframe = this.ComboTimeframe,

                AllowEmoji = this.AllowEmoji,

                IgnoreDuplicates = this.IgnoreDuplicates,
            };

            return result;
        }
    }
}
