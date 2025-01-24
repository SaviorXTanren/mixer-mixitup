using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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

        RandomIn = 1000,
        RandomOut = 1001,
        RandomVisible = 1002,
    }

    public enum OverlayWoahCSSAnimationType
    {
        None,

        Random,

        DealWithIt,

        FlyIn,
        FlyOut,

        LeaveInStyle,

        RotateComplex,
        RotateComplexOut,

        Spin3D,

        Wowzors,
    }

    [DataContract]
    public class OverlayAnimationV3Model
    {
        private static readonly List<OverlayAnimateCSSAnimationType> AnimateCSSRandomInAnimations = new List<OverlayAnimateCSSAnimationType>()
        {
            OverlayAnimateCSSAnimationType.BackInDown,
            OverlayAnimateCSSAnimationType.BackInLeft,
            OverlayAnimateCSSAnimationType.BackInRight,
            OverlayAnimateCSSAnimationType.BackInUp,

            OverlayAnimateCSSAnimationType.BounceIn,
            OverlayAnimateCSSAnimationType.BounceInDown,
            OverlayAnimateCSSAnimationType.BounceInLeft,
            OverlayAnimateCSSAnimationType.BounceInRight,
            OverlayAnimateCSSAnimationType.BounceInUp,

            OverlayAnimateCSSAnimationType.FadeIn,
            OverlayAnimateCSSAnimationType.FadeInDown,
            OverlayAnimateCSSAnimationType.FadeInDownBig,
            OverlayAnimateCSSAnimationType.FadeInLeft,
            OverlayAnimateCSSAnimationType.FadeInLeftBig,
            OverlayAnimateCSSAnimationType.FadeInRight,
            OverlayAnimateCSSAnimationType.FadeInRightBig,
            OverlayAnimateCSSAnimationType.FadeInUp,
            OverlayAnimateCSSAnimationType.FadeInUpBig,
            OverlayAnimateCSSAnimationType.FadeInTopLeft,
            OverlayAnimateCSSAnimationType.FadeInTopRight,
            OverlayAnimateCSSAnimationType.FadeInBottomLeft,
            OverlayAnimateCSSAnimationType.FadeInBottomRight,

            OverlayAnimateCSSAnimationType.FlipInX,
            OverlayAnimateCSSAnimationType.FlipInY,

            OverlayAnimateCSSAnimationType.LightSpeedInRight,
            OverlayAnimateCSSAnimationType.LightSpeedInLeft,

            OverlayAnimateCSSAnimationType.RotateIn,
            OverlayAnimateCSSAnimationType.RotateInDownLeft,
            OverlayAnimateCSSAnimationType.RotateInDownRight,
            OverlayAnimateCSSAnimationType.RotateInUpLeft,
            OverlayAnimateCSSAnimationType.RotateInUpRight,

            OverlayAnimateCSSAnimationType.SlideInDown,
            OverlayAnimateCSSAnimationType.SlideInLeft,
            OverlayAnimateCSSAnimationType.SlideInRight,
            OverlayAnimateCSSAnimationType.SlideInUp,

            OverlayAnimateCSSAnimationType.ZoomIn,
            OverlayAnimateCSSAnimationType.ZoomInDown,
            OverlayAnimateCSSAnimationType.ZoomInLeft,
            OverlayAnimateCSSAnimationType.ZoomInRight,
            OverlayAnimateCSSAnimationType.ZoomInUp,
        };

        private static readonly List<OverlayAnimateCSSAnimationType> AnimateCSSRandomOutAnimations = new List<OverlayAnimateCSSAnimationType>()
        {
            OverlayAnimateCSSAnimationType.BackOutDown,
            OverlayAnimateCSSAnimationType.BackOutLeft,
            OverlayAnimateCSSAnimationType.BackOutRight,
            OverlayAnimateCSSAnimationType.BackOutUp,

            OverlayAnimateCSSAnimationType.BounceOut,
            OverlayAnimateCSSAnimationType.BounceOutDown,
            OverlayAnimateCSSAnimationType.BounceOutLeft,
            OverlayAnimateCSSAnimationType.BounceOutRight,
            OverlayAnimateCSSAnimationType.BounceOutUp,

            OverlayAnimateCSSAnimationType.FadeOut,
            OverlayAnimateCSSAnimationType.FadeOutDown,
            OverlayAnimateCSSAnimationType.FadeOutDownBig,
            OverlayAnimateCSSAnimationType.FadeOutLeft,
            OverlayAnimateCSSAnimationType.FadeOutLeftBig,
            OverlayAnimateCSSAnimationType.FadeOutRight,
            OverlayAnimateCSSAnimationType.FadeOutRightBig,
            OverlayAnimateCSSAnimationType.FadeOutUp,
            OverlayAnimateCSSAnimationType.FadeOutUpBig,
            OverlayAnimateCSSAnimationType.FadeOutTopLeft,
            OverlayAnimateCSSAnimationType.FadeOutTopRight,
            OverlayAnimateCSSAnimationType.FadeOutBottomLeft,
            OverlayAnimateCSSAnimationType.FadeOutBottomRight,

            OverlayAnimateCSSAnimationType.FlipOutX,
            OverlayAnimateCSSAnimationType.FlipOutY,

            OverlayAnimateCSSAnimationType.LightSpeedOutRight,
            OverlayAnimateCSSAnimationType.LightSpeedOutLeft,

            OverlayAnimateCSSAnimationType.RotateOut,
            OverlayAnimateCSSAnimationType.RotateOutDownLeft,
            OverlayAnimateCSSAnimationType.RotateOutDownRight,
            OverlayAnimateCSSAnimationType.RotateOutUpLeft,
            OverlayAnimateCSSAnimationType.RotateOutUpRight,

            OverlayAnimateCSSAnimationType.SlideOutDown,
            OverlayAnimateCSSAnimationType.SlideOutLeft,
            OverlayAnimateCSSAnimationType.SlideOutRight,
            OverlayAnimateCSSAnimationType.SlideOutUp,

            OverlayAnimateCSSAnimationType.ZoomOut,
            OverlayAnimateCSSAnimationType.ZoomOutDown,
            OverlayAnimateCSSAnimationType.ZoomOutLeft,
            OverlayAnimateCSSAnimationType.ZoomOutRight,
            OverlayAnimateCSSAnimationType.ZoomOutUp,
        };

        private static readonly List<OverlayAnimateCSSAnimationType> AnimateCSSRandomVisibleAnimations = new List<OverlayAnimateCSSAnimationType>()
        {
            OverlayAnimateCSSAnimationType.Bounce,
            OverlayAnimateCSSAnimationType.Flash,
            OverlayAnimateCSSAnimationType.Pulse,
            OverlayAnimateCSSAnimationType.RubberBand,
            OverlayAnimateCSSAnimationType.ShakeX,
            OverlayAnimateCSSAnimationType.ShakeY,
            OverlayAnimateCSSAnimationType.HeadShake,
            OverlayAnimateCSSAnimationType.Swing,
            OverlayAnimateCSSAnimationType.Tada,
            OverlayAnimateCSSAnimationType.Wobble,
            OverlayAnimateCSSAnimationType.Jello,
            OverlayAnimateCSSAnimationType.HeartBeat,
            OverlayAnimateCSSAnimationType.Flip,
            OverlayAnimateCSSAnimationType.Hinge,
            OverlayAnimateCSSAnimationType.JackInTheBox,
            OverlayAnimateCSSAnimationType.RollIn,
            OverlayAnimateCSSAnimationType.RollOut,
        };

        [DataMember]
        public double StartTime { get; set; }

        [DataMember]
        public OverlayAnimateCSSAnimationType AnimateCSSAnimation { get; set; }
        [JsonIgnore]
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
                        values.Remove(OverlayAnimateCSSAnimationType.RandomIn);
                        values.Remove(OverlayAnimateCSSAnimationType.RandomOut);
                        values.Remove(OverlayAnimateCSSAnimationType.RandomVisible);
                        animation = values.Random();
                    }
                    else if (animation == OverlayAnimateCSSAnimationType.RandomIn)
                    {
                        animation = AnimateCSSRandomInAnimations.Random();
                    }
                    else if (animation == OverlayAnimateCSSAnimationType.RandomOut)
                    {
                        animation = AnimateCSSRandomOutAnimations.Random();
                    }
                    else if (animation == OverlayAnimateCSSAnimationType.RandomVisible)
                    {
                        animation = AnimateCSSRandomVisibleAnimations.Random();
                    }

                    string animationName = animation.ToString();
                    return Char.ToLowerInvariant(animationName[0]) + animationName.Substring(1);
                }
                return string.Empty;
            }
        }

        [DataMember]
        public OverlayWoahCSSAnimationType WoahCSSAnimation { get; set; }
        [JsonIgnore]
        public string WoahCSSAnimationName
        {
            get
            {
                if (this.WoahCSSAnimation != OverlayWoahCSSAnimationType.None)
                {
                    OverlayWoahCSSAnimationType animation = this.WoahCSSAnimation;

                    if (animation == OverlayWoahCSSAnimationType.Random)
                    {
                        HashSet<OverlayWoahCSSAnimationType> values = new HashSet<OverlayWoahCSSAnimationType>(EnumHelper.GetEnumList<OverlayWoahCSSAnimationType>().ToList());
                        values.Remove(OverlayWoahCSSAnimationType.None);
                        values.Remove(OverlayWoahCSSAnimationType.Random);
                        animation = values.Random();
                    }

                    string animationName = animation.ToString();
                    return Char.ToLowerInvariant(animationName[0]) + animationName.Substring(1);
                }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public string AnimationFramework
        {
            get
            {
                if (this.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
                {
                    return "animate.css";
                }
                if (this.WoahCSSAnimation != OverlayWoahCSSAnimationType.None)
                {
                    return "Woah.css";
                }
                return null;
            }
        }

        [JsonIgnore]
        public string AnimationName
        {
            get
            {
                if (this.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
                {
                    return this.AnimateCSSAnimationName;
                }
                if (this.WoahCSSAnimation != OverlayWoahCSSAnimationType.None)
                {
                    return this.WoahCSSAnimationName;
                }
                return null;
            }
        }

        public void AddAnimationProperties(Dictionary<string, object> properties, string name)
        {
            properties[$"{name}Framework"] = this.AnimationFramework;
            properties[$"{name}Name"] = this.AnimationName;
            properties[$"{name}StartTime"] = this.StartTime.ToString(CultureInfo.InvariantCulture);
        }
    }
}
