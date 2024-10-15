using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayItemAnimationLibraryType
    {
        AnimateCSS,
        WoahCSS,
    }

    public class OverlayAnimationV3ViewModel : UIViewModelBase
    {
        public bool IsCustomizable
        {
            get { return this.isCustomizable; }
            set
            {
                this.isCustomizable = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isCustomizable;

        public bool IsNotCustomizable { get { return !this.IsCustomizable; } }

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

        public double StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private double startTime;

        public string DisplayName { get { return Resources.ResourceManager.GetString(this.Name, Resources.Culture); } }

        public IEnumerable<OverlayItemAnimationLibraryType> AnimationLibraries { get { return EnumLocalizationHelper.GetSortedEnumList<OverlayItemAnimationLibraryType>(); } }

        public OverlayItemAnimationLibraryType SelectedAnimationLibrary
        {
            get { return this.selectedAnimationLibrary; }
            set
            {
                this.selectedAnimationLibrary = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(IsAnimateCSSVisible));
                this.NotifyPropertyChanged(nameof(IsWoahCSSVisible));

                this.SelectedAnimatedCSSAnimation = OverlayAnimateCSSAnimationType.None;
                this.SelectedWoahCSSAnimation = OverlayWoahCSSAnimationType.None;
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
        private static IEnumerable<OverlayAnimateCSSAnimationType> animateCSSAnimations = null;

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

        public bool IsWoahCSSVisible { get { return this.SelectedAnimationLibrary == OverlayItemAnimationLibraryType.WoahCSS; } }

        public IEnumerable<OverlayWoahCSSAnimationType> WoahCSSAnimations
        {
            get
            {
                if (OverlayAnimationV3ViewModel.woahCSSAnimations == null)
                {
                    List<OverlayWoahCSSAnimationType> animations = new List<OverlayWoahCSSAnimationType>(EnumLocalizationHelper.GetSortedEnumList<OverlayWoahCSSAnimationType>());
                    animations.Remove(OverlayWoahCSSAnimationType.None);
                    animations.Insert(0, OverlayWoahCSSAnimationType.None);
                    OverlayAnimationV3ViewModel.woahCSSAnimations = animations;
                }
                return OverlayAnimationV3ViewModel.woahCSSAnimations;
            }
        }
        private static IEnumerable<OverlayWoahCSSAnimationType> woahCSSAnimations = null;

        public OverlayWoahCSSAnimationType SelectedWoahCSSAnimation
        {
            get { return this.selectedWoahCSSAnimation; }
            set
            {
                this.selectedWoahCSSAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWoahCSSAnimationType selectedWoahCSSAnimation;

        public ICommand DeleteCommand { get; set; }
        public event EventHandler OnDeleteRequested = delegate { };

        public OverlayAnimationV3ViewModel(string name) : this(name, new OverlayAnimationV3Model()) { }

        public OverlayAnimationV3ViewModel(OverlayAnimationV3Model animation)
            : this(null, animation)
        {
            this.IsCustomizable = true;
        }

        public OverlayAnimationV3ViewModel(string name, OverlayAnimationV3Model animation)
        {
            this.Name = name;
            if (animation != null)
            {
                this.StartTime = animation.StartTime;

                if (animation.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
                {
                    this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;
                    this.SelectedAnimatedCSSAnimation = animation.AnimateCSSAnimation;
                }
                else if (animation.WoahCSSAnimation != OverlayWoahCSSAnimationType.None)
                {
                    this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.WoahCSS;
                    this.SelectedWoahCSSAnimation = animation.WoahCSSAnimation;
                }
            }
            else
            {
                this.SelectedAnimationLibrary = OverlayItemAnimationLibraryType.AnimateCSS;
                this.SelectedAnimatedCSSAnimation = OverlayAnimateCSSAnimationType.None;
                this.SelectedWoahCSSAnimation = OverlayWoahCSSAnimationType.None;
            }

            this.DeleteCommand = this.CreateCommand(() => this.OnDeleteRequested(this, new EventArgs()));
        }

        public OverlayAnimationV3Model GetAnimation()
        {
            OverlayAnimationV3Model animation = new OverlayAnimationV3Model();

            animation.StartTime = this.StartTime;

            if (this.IsAnimateCSSVisible && this.SelectedAnimatedCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                animation.AnimateCSSAnimation = this.SelectedAnimatedCSSAnimation;
            }
            else if (this.IsWoahCSSVisible && this.SelectedWoahCSSAnimation != OverlayWoahCSSAnimationType.None)
            {
                animation.WoahCSSAnimation = this.SelectedWoahCSSAnimation;
            }

            return animation;
        }
    }
}
