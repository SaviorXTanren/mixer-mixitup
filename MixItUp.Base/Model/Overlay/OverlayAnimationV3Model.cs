using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
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
    }

    [DataContract]
    public class OverlayAnimationV3Model
    {
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
                return null;
            }
        }

        public void AddAnimationProperties(Dictionary<string, object> properties, string name)
        {
            properties[$"{name}Framework"] = this.AnimationFramework;
            properties[$"{name}Name"] = this.AnimationName;
            properties[$"{name}StartTime"] = this.StartTime;
        }
    }
}
