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

    public class OverlayAnimationV3ViewModel : UIViewModelBase
    {
        public string Name { get; set; }

        private static IEnumerable<OverlayAnimateCSSAnimationType> animateCSSAnimations = null;

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

        public IEnumerable<OverlayAnimateCSSAnimationType> AnimateCSSAnimations
        {
            get
            {
                if (OverlayAnimationV3ViewModel.animateCSSAnimations == null)
                {
                    List<OverlayAnimateCSSAnimationType> animations = new List<OverlayAnimateCSSAnimationType>(EnumLocalizationHelper.GetSortedEnumList<OverlayAnimateCSSAnimationType>());
                    animations.Remove(OverlayAnimateCSSAnimationType.None);
                    animations.Insert(0, OverlayAnimateCSSAnimationType.None);
                    OverlayAnimationV3ViewModel.animateCSSAnimations = animations;
                }
                return OverlayAnimationV3ViewModel.animateCSSAnimations;
            }
        }

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

        public OverlayAnimationV3ViewModel(string name)
        {
            this.Name = name;
        }

        public OverlayAnimationV3ViewModel(string name, OverlayAnimationV3Model animation)
            : this(name)
        {
            if (animation.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;
                this.SelectedAnimatedCSSAnimation = animation.AnimateCSSAnimation;
            }
        }

        public void SetAnimation(OverlayAnimationV3Model animation)
        {
            if (this.IsAnimateCSSVisible && this.SelectedAnimatedCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                animation.AnimateCSSAnimation = this.SelectedAnimatedCSSAnimation;
            }
        }
    }
}
