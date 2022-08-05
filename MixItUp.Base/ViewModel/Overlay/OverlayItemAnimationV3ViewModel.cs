using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayItemAnimationLibraryType
    {
        AnimateCSS,
    }

    public class OverlayItemAnimationV3ViewModel : UIViewModelBase
    {
        public IEnumerable<OverlayItemAnimationLibraryType> AnimationLibraries { get { return EnumLocalizationHelper.GetSortedEnumList<OverlayItemAnimationLibraryType>(); } }

        public OverlayItemAnimationLibraryType SelectedAnimationLibrary
        {
            get { return this.selectedAnimationLibrary; }
            set
            {
                this.selectedAnimationLibrary = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(IsAnimateCSSVisible));

                this.SelectedAnimatedCSSAnimation = OverlayAnimateCSSAnimationType.None;
            }
        }
        private OverlayItemAnimationLibraryType selectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;

        public bool IsAnimateCSSVisible { get { return this.SelectedAnimationLibrary == OverlayItemAnimationLibraryType.AnimateCSS; } }

        public IEnumerable<OverlayAnimateCSSAnimationType> AnimateCSSAnimations { get { return EnumLocalizationHelper.GetSortedEnumList<OverlayAnimateCSSAnimationType>(); } }

        public OverlayAnimateCSSAnimationType SelectedAnimatedCSSAnimation
        {
            get { return this.selectedAnimatedCSSAnimation; }
            set
            {
                this.selectedAnimatedCSSAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayAnimateCSSAnimationType selectedAnimatedCSSAnimation;

        public OverlayItemAnimationV3ViewModel() { }

        public OverlayItemAnimationV3ViewModel(OverlayItemAnimationV3Model animation)
        {
            if (animation.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;
                this.SelectedAnimatedCSSAnimation = animation.AnimateCSSAnimation;
            }
        }

        public void SetAnimation(OverlayItemAnimationV3Model animation)
        {
            if (this.IsAnimateCSSVisible && this.SelectedAnimatedCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                animation.AnimateCSSAnimation = this.SelectedAnimatedCSSAnimation;
            }
        }
    }
}
