using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayAnimateCSSAnimationType
    {
        None,

        Random,

        Bounce,
        Flash,
        Pulse,
        RubberBand,
        ShakeX,
        ShakeY,
        HeadShake,
        Swing,
        Tada,
        Wobble,
        Jello,
        HeartBeat,

        BackInDown,
        BackInLeft,
        BackInRight,
        BackInUp,

        BackOutDown,
        BackOutLeft,
        BackOutRight,
        BackOutUp,

        BounceIn,
        BounceInDown,
        BounceInLeft,
        BounceInRight,
        BounceInUp,

        BounceOut,
        BounceOutDown,
        BounceOutLeft,
        BounceOutRight,
        BounceOutUp,

        FadeIn,
        FadeInDown,
        FadeInDownBig,
        FadeInLeft,
        FadeInLeftBig,
        FadeInRight,
        FadeInRightBig,
        FadeInUp,
        FadeInUpBig,
        FadeInTopLeft,
        FadeInTopRight,
        FadeInBottomLeft,
        FadeInBottomRight,

        FadeOut,
        FadeOutDown,
        FadeOutDownBig,
        FadeOutLeft,
        FadeOutLeftBig,
        FadeOutRight,
        FadeOutRightBig,
        FadeOutUp,
        FadeOutUpBig,
        FadeOutTopLeft,
        FadeOutTopRight,
        FadeOutBottomLeft,
        FadeOutBottomRight,

        Flip,
        FlipInX,
        FlipInY,
        FlipOutX,
        FlipOutY,

        LightSpeedInRight,
        LightSpeedInLeft,
        LightSpeedOutRight,
        LightSpeedOutLeft,

        RotateIn,
        RotateInDownLeft,
        RotateInDownRight,
        RotateInUpLeft,
        RotateInUpRight,

        RotateOut,
        RotateOutDownLeft,
        RotateOutDownRight,
        RotateOutUpLeft,
        RotateOutUpRight,

        Hinge,
        JackInTheBox,
        RollIn,
        RollOut,

        ZoomIn,
        ZoomInDown,
        ZoomInLeft,
        ZoomInRight,
        ZoomInUp,

        ZoomOut,
        ZoomOutDown,
        ZoomOutLeft,
        ZoomOutRight,
        ZoomOutUp,

        SlideInDown,
        SlideInLeft,
        SlideInRight,
        SlideInUp,

        SlideOutDown,
        SlideOutLeft,
        SlideOutRight,
        SlideOutUp,
    }

    [DataContract]
    public class OverlayAnimationV3Model
    {
        public const string EntranceAnimationName = "Entrance";
        public const string VisibleAnimationName = "Visible";
        public const string ExitAniamtionName = "Exit";

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double Timing { get; set; }
        [DataMember]
        public int MillisecondTiming { get { return (int)(this.Timing * 1000.0); } set { } }

        [DataMember]
        public OverlayAnimateCSSAnimationType AnimateCSSAnimation { get; set; }
        [DataMember]
        public string AnimateCSSAnimationName
        {
            get
            {
                if (this.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
                {
                    OverlayAnimateCSSAnimationType animation = this.AnimateCSSAnimation;

                    if (animation == OverlayAnimateCSSAnimationType.Random)
                    {
                        HashSet<OverlayAnimateCSSAnimationType> values = new HashSet<OverlayAnimateCSSAnimationType>(EnumHelper.GetEnumList<OverlayAnimateCSSAnimationType>().ToList());
                        values.Remove(OverlayAnimateCSSAnimationType.None);
                        values.Remove(OverlayAnimateCSSAnimationType.Random);
                        animation = values.Random();
                    }

                    string animationName = animation.ToString();
                    return Char.ToLowerInvariant(animationName[0]) + animationName.Substring(1);
                }
                return string.Empty;
            }
        }

        public OverlayAnimationV3Model(string name)
        {
            this.Name = name;
        }

        [Obsolete]
        public OverlayAnimationV3Model() { }

        public string GenerateAnimationJavascript()
        {
            string output = string.Empty;
            if (this.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                output = Resources.OverlayAnimateCSSJavascript;
                output = OverlayItemV3ModelBase.ReplaceProperty(output, nameof(this.AnimateCSSAnimationName), this.AnimateCSSAnimationName);
            }

            if (!string.IsNullOrEmpty(output) && this.MillisecondTiming > 0)
            {
                output = OverlayItemV3ModelBase.ReplaceProperty(Resources.OverlayAnimationTimedWrapperJavascript, "Animation", output);
                output = OverlayItemV3ModelBase.ReplaceProperty(output, nameof(this.MillisecondTiming), this.MillisecondTiming.ToString());
            }

            return output;
        }
    }
}
