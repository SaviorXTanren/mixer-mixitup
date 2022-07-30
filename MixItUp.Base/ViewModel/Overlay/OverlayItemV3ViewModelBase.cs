using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayItemV3ViewModelBase : UIViewModelBase
    {
        public OverlayItemPositionV3ViewModel ItemPosition
        {
            get { return this.itemPosition; }
            set
            {
                this.itemPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemPositionV3ViewModel itemPosition = new OverlayItemPositionV3ViewModel();

        public string ItemDuration
        {
            get { return this.itemDuration; }
            set
            {
                this.itemDuration = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemDuration;

        [Obsolete]
        public IEnumerable<OverlayItemEffectEntranceAnimationTypeEnum> EntranceAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectEntranceAnimationTypeEnum>(); } }

        [Obsolete]
        public OverlayItemEffectEntranceAnimationTypeEnum SelectedEntranceAnimation
        {
            get { return this.selectedEntranceAnimation; }
            set
            {
                this.selectedEntranceAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        [Obsolete]
        private OverlayItemEffectEntranceAnimationTypeEnum selectedEntranceAnimation;

        [Obsolete]
        public IEnumerable<OverlayItemEffectExitAnimationTypeEnum> ExitAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectExitAnimationTypeEnum>(); } }

        [Obsolete]
        public OverlayItemEffectExitAnimationTypeEnum SelectedExitAnimation
        {
            get { return this.selectedExitAnimation; }
            set
            {
                this.selectedExitAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        [Obsolete]
        private OverlayItemEffectExitAnimationTypeEnum selectedExitAnimation;

        [Obsolete]
        public IEnumerable<OverlayItemEffectVisibleAnimationTypeEnum> VisibleAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectVisibleAnimationTypeEnum>(); } }

        [Obsolete]
        public OverlayItemEffectVisibleAnimationTypeEnum SelectedVisibleAnimation
        {
            get { return this.selectedVisibleAnimation; }
            set
            {
                this.selectedVisibleAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        [Obsolete]
        private OverlayItemEffectVisibleAnimationTypeEnum selectedVisibleAnimation;

        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
            }
        }
        private string html;

        public string CSS
        {
            get { return this.css; }
            set
            {
                this.css = value;
                this.NotifyPropertyChanged();
            }
        }
        private string css;

        public string Javascript
        {
            get { return this.javascript; }
            set
            {
                this.javascript = value;
                this.NotifyPropertyChanged();
            }
        }
        private string javascript;

        public OverlayItemV3ViewModelBase(OverlayItemV3Type type)
        {
            switch (type)
            {
                case OverlayItemV3Type.Text: this.HTML = OverlayTextItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.Image: this.HTML = OverlayImageItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.Video: this.HTML = OverlayVideoItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.YouTube: this.HTML = OverlayYouTubeItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.HTML: this.HTML = OverlayHTMLItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.WebPage: this.HTML = OverlayWebPageItemV3Model.DefaultHTML; break;
            }

            if (!string.IsNullOrEmpty(this.HTML))
            {
                this.HTML = OverlayItemV3ModelBase.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, this.HTML);
            }

            this.ItemPosition = new OverlayItemPositionV3ViewModel();
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.HTML = item.HTML;
            this.CSS = item.CSS;
            this.Javascript = item.Javascript;

            this.ItemPosition = new OverlayItemPositionV3ViewModel(item);
        }

        public OverlayItemV3ModelBase GetItem()
        {
            OverlayItemV3ModelBase item = this.GetItemInternal();

            this.ItemPosition.SetPosition(item);

            return item;
        }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
