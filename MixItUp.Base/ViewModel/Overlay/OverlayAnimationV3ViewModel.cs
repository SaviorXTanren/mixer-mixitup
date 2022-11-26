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
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public string DisplayName { get { return Resources.ResourceManager.GetString(this.Name, Resources.Culture); } }

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

        public OverlayAnimationV3ViewModel(OverlayAnimationV3Model animation)
            : this(animation.Name)
        {
            if (animation.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;
                this.SelectedAnimatedCSSAnimation = animation.AnimateCSSAnimation;
            }
        }

        public OverlayAnimationV3Model GetAnimation()
        {
            OverlayAnimationV3Model animation = new OverlayAnimationV3Model(this.Name);

            if (this.IsAnimateCSSVisible && this.SelectedAnimatedCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                animation.AnimateCSSAnimation = this.SelectedAnimatedCSSAnimation;
            }

            return animation;
        }
    }
}
