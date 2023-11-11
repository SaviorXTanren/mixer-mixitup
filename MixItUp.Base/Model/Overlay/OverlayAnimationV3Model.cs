using MixItUp.Base.Services;
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
        private const string AnimationElementID = "AnimationElementID";
        private const string PostAnimation = "PostAnimation";

        private const string PostTimeout = "PostTimeout";
        private const string MillisecondTiming = "MillisecondTiming";

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

        //public string GenerateTimedAnimationJavascript(double seconds)
        //{
        //    string output = this.GenerateAnimationJavascript();
        //    if (!string.IsNullOrEmpty(output))
        //    {
        //        output = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayAnimationTimedWrapperJavascript, "Animation", output);
        //        output = OverlayV3Service.ReplaceProperty(output, "MillisecondTiming", $"({seconds} * 1000)");
        //    }
        //    return output;
        //}

        //public string GenerateRemoveAnimationJavascript(string id, double seconds)
        //{
        //    string output = this.GenerateAnimationJavascript(includePostProcessingFunction: true);
        //    if (!string.IsNullOrEmpty(output))
        //    {
        //        output = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayAnimationTimedWrapperJavascript, "Animation", output);
        //        output = OverlayV3Service.ReplaceProperty(output, "PostAnimation", OverlayResources.OverlayItemHideAndSendParentMessageRemoveJavascript);
        //    }
        //    else
        //    {
        //        output = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayAnimationTimedWrapperJavascript, "Animation", OverlayResources.OverlayItemHideAndSendParentMessageRemoveJavascript);
        //    }
        //    output = OverlayV3Service.ReplaceProperty(output, "MillisecondTiming", $"({seconds} * 1000)");
        //    output = OverlayV3Service.ReplaceProperty(output, "ID", id);
        //    return output;
        //}

        public string GenerateAnimationJavascript(string animationElementID, double preTimeout = 0.0, string postAnimation = "")
        {
            string output = string.Empty;
            if (this.AnimateCSSAnimation != OverlayAnimateCSSAnimationType.None)
            {
                output = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayAnimateCSSJavascript, nameof(this.AnimateCSSAnimationName), this.AnimateCSSAnimationName);
            }

            if (!string.IsNullOrEmpty(output))
            {
                output = OverlayV3Service.ReplaceProperty(output, PostAnimation, postAnimation);
            }
            else
            {
                output = postAnimation;
            }
            output = OverlayV3Service.ReplaceProperty(output, AnimationElementID, animationElementID);

            if (preTimeout > 0.0)
            {
                output = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayTimeoutWrapperJavascript, PostTimeout, output);
                output = OverlayV3Service.ReplaceProperty(output, MillisecondTiming, ((int)Math.Round(preTimeout * 1000)).ToString());
            }

            return output;
        }
    }
}
