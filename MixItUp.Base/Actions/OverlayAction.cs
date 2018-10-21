using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum OverlayEffectTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        [Name("Web Page")]
        WebPage
    }

    public enum OverlayEffectPositionType
    {
        Percentage,
        Pixel,
    }

    public enum OverlayEffectEntranceAnimationTypeEnum
    {
        None,

        [Name("Bounce In")]
        BounceIn,
        [Name("Bounce In Up")]
        BounceInUp,
        [Name("Bounce In Down")]
        BounceInDown,
        [Name("Bounce In Left")]
        BounceInLeft,
        [Name("Bounce In Right")]
        BounceInRight,

        [Name("Fade In")]
        FadeIn,
        [Name("Fade In Up")]
        FadeInUp,
        [Name("Fade In Down")]
        FadeInDown,
        [Name("Fade In Left")]
        FadeInLeft,
        [Name("Fade In Right")]
        FadeInRight,

        [Name("Flip In X")]
        FlipInX,
        [Name("Flip In Y")]
        FlipInY,

        [Name("Light Speed In")]
        LightSpeedIn,

        [Name("Rotate In")]
        RotateIn,


        [Name("Rotate In Up")]
        [Obsolete]
        RotateInUp,
        [Name("Rotate In Down")]
        [Obsolete]
        RotateInDown,
        [Name("Rotate In Left")]
        [Obsolete]
        RotateInLeft,
        [Name("Rotate In Right")]
        [Obsolete]
        RotateInRight,


        [Name("Slide In Up")]
        SlideInUp,
        [Name("Slide In Down")]
        SlideInDown,
        [Name("Slide In Left")]
        SlideInLeft,
        [Name("Slide In Right")]
        SlideInRight,

        [Name("Zoom In")]
        ZoomIn,
        [Name("Zoom In Up")]
        ZoomInUp,
        [Name("Zoom In Down")]
        ZoomInDown,
        [Name("Zoom In Left")]
        ZoomInLeft,
        [Name("Zoom In Right")]
        ZoomInRight,

        [Name("Jack In The Box")]
        JackInTheBox,

        [Name("Roll In")]
        RollIn,

        Random,
    }

    public enum OverlayEffectVisibleAnimationTypeEnum
    {
        None,

        Bounce,
        Flash,
        Pulse,
        [Name("Rubber Band")]
        RubberBand,
        Shake,
        Swing,
        Tada,
        Wobble,
        Jello,
        Flip,

        Random,
    }

    public enum OverlayEffectExitAnimationTypeEnum
    {
        None,

        [Name("Bounce Out")]
        BounceOut,
        [Name("Bounce Out Up")]
        BounceOutUp,
        [Name("Bounce Out Down")]
        BounceOutDown,
        [Name("Bounce Out Left")]
        BounceOutLeft,
        [Name("Bounce Out Right")]
        BounceOutRight,

        [Name("Fade Out")]
        FadeOut,
        [Name("Fade Out Up")]
        FadeOutUp,
        [Name("Fade Out Down")]
        FadeOutDown,
        [Name("Fade Out Left")]
        FadeOutLeft,
        [Name("Fade Out Right")]
        FadeOutRight,

        [Name("Flip Out X")]
        FlipOutX,
        [Name("Flip Out Y")]
        FlipOutY,

        [Name("Light Speed Out")]
        LightSpeedOut,

        [Name("Rotate Out")]
        RotateOut,


        [Name("Rotate Out Up")]
        [Obsolete]
        RotateOutUp,
        [Name("Rotate Out Down")]
        [Obsolete]
        RotateOutDown,
        [Name("Rotate Out Left")]
        [Obsolete]
        RotateOutLeft,
        [Name("Rotate Out Right")]
        [Obsolete]
        RotateOutRight,


        [Name("Slide Out Up")]
        SlideOutUp,
        [Name("Slide Out Down")]
        SlideOutDown,
        [Name("Slide Out Left")]
        SlideOutLeft,
        [Name("Slide Out Right")]
        SlideOutRight,

        [Name("Zoom Out")]
        ZoomOut,
        [Name("Zoom Out Up")]
        ZoomOutUp,
        [Name("Zoom Out Down")]
        ZoomOutDown,
        [Name("Zoom Out Left")]
        ZoomOutLeft,
        [Name("Zoom Out Right")]
        ZoomOutRight,

        Hinge,

        [Name("Roll Out")]
        RollOut,

        Random,
    }

    [DataContract]
    public class OverlayTextEffect : OverlayEffectBase
    {
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int Size { get; set; }

        public OverlayTextEffect() { }

        public OverlayTextEffect(string text, string color, int size, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.Text, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.Text = text;
            this.Color = color;
            this.Size = size;
        }
    }

    [DataContract]
    public class OverlayImageEffect : OverlayEffectBase
    {
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}?nonce={1}", this.ID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageEffect() { }

        public OverlayImageEffect(string filepath, int width, int height, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.Image, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.ID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }

    [DataContract]
    public class OverlayVideoEffect : OverlayEffectBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}", this.ID);
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoEffect() { this.Volume = 100; }

        public OverlayVideoEffect(string filepath, int width, int height, int volume, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.Video, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.ID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }

    [DataContract]
    public class OverlayYoutubeEffect : OverlayEffectBase
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public int StartTime { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayYoutubeEffect() { }

        public OverlayYoutubeEffect(string id, int startTime, int width, int height, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.YouTube, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.ID = id;
            this.StartTime = startTime;
            this.Width = width;
            this.Height = height;
        }
    }

    [DataContract]
    public class OverlayHTMLEffect : OverlayEffectBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLEffect() { }

        public OverlayHTMLEffect(string htmlText, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible, OverlayEffectExitAnimationTypeEnum exit,
            double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.HTML, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.HTMLText = htmlText;
        }
    }

    [DataContract]
    public class OverlayWebPageEffect : OverlayEffectBase
    {
        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayWebPageEffect() { }

        public OverlayWebPageEffect(string url, int width, int height, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
            : base(OverlayEffectTypeEnum.WebPage, entrance, visible, exit, duration, positionType, horizontal, vertical)
        {
            this.URL = url;
            this.Width = width;
            this.Height = height;
        }
    }

    [DataContract]
    public class OverlayEffectBase
    {
        private static readonly Random Random = new Random();

        [DataMember]
        public OverlayEffectTypeEnum EffectType { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return this.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayEffectVisibleAnimationTypeEnum VisibleAnimation { get; set; }
        [DataMember]
        public string VisibleAnimationName { get { return this.GetAnimationClassName(this.VisibleAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return this.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public double Duration;

        [DataMember]
        public OverlayEffectPositionType PositionType;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        [DataMember]
        public bool IsPercentagePosition { get { return this.PositionType == OverlayEffectPositionType.Percentage; } }
        [DataMember]
        public bool IsPixelPosition { get { return this.PositionType == OverlayEffectPositionType.Pixel; } }

        public OverlayEffectBase() { }

        public OverlayEffectBase(OverlayEffectTypeEnum effectType, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible,
            OverlayEffectExitAnimationTypeEnum exit, double duration, OverlayEffectPositionType positionType, int horizontal, int vertical)
        {
            this.EffectType = effectType;
            this.EntranceAnimation = entrance;
            this.VisibleAnimation = visible;
            this.ExitAnimation = exit;
            this.Duration = duration;
            this.PositionType = positionType;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }

        public T Copy<T>()
        {
            JObject jobj = JObject.FromObject(this);
            return jobj.ToObject<T>();
        }

        private string GetAnimationClassName<T>(T animationType)
        {
            string name = animationType.ToString();

            if (EnumHelper.IsObsolete(animationType))
            {
                name = string.Empty;
            }

            if (!string.IsNullOrEmpty(name) && name.Equals("Random"))
            {
                List<T> values = EnumHelper.GetEnumList<T>().ToList();
                values.RemoveAll(v => v.ToString().Equals("None") || v.ToString().Equals("Random"));
                name = values[Random.Next(values.Count)].ToString();
            }

            if (!string.IsNullOrEmpty(name) && !name.Equals("None"))
            {
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return string.Empty;
        }
    }

    [DataContract]
    public class OverlayAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OverlayAction.asyncSemaphore; } }

        [DataMember]
        public OverlayEffectBase Effect { get; set; }

        public OverlayAction() : base(ActionTypeEnum.Overlay) { }

        public OverlayAction(OverlayEffectBase effect)
            : this()
        {
            this.Effect = effect;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                if (this.Effect is OverlayImageEffect)
                {
                    OverlayImageEffect imageEffect = (OverlayImageEffect)this.Effect;
                    string imageFilePath = await this.ReplaceStringWithSpecialModifiers(imageEffect.FilePath, user, arguments);
                    if (!Uri.IsWellFormedUriString(imageFilePath, UriKind.RelativeOrAbsolute))
                    {
                        imageFilePath = imageFilePath.ToFilePathString();
                    }

                    if (!string.IsNullOrEmpty(imageFilePath))
                    {
                        OverlayImageEffect copy = imageEffect.Copy<OverlayImageEffect>();
                        copy.FilePath = imageFilePath;
                        await ChannelSession.Services.OverlayServer.SendImage(copy);
                    }
                }
                else if (this.Effect is OverlayTextEffect)
                {
                    OverlayTextEffect textEffect = (OverlayTextEffect)this.Effect;
                    string text = await this.ReplaceStringWithSpecialModifiers(textEffect.Text, user, arguments);
                    OverlayTextEffect copy = textEffect.Copy<OverlayTextEffect>();
                    copy.Text = text;
                    await ChannelSession.Services.OverlayServer.SendText(copy);
                }
                else if (this.Effect is OverlayYoutubeEffect)
                {
                    await ChannelSession.Services.OverlayServer.SendYoutubeVideo((OverlayYoutubeEffect)this.Effect);
                }
                else if (this.Effect is OverlayVideoEffect)
                {
                    OverlayVideoEffect videoEffect = (OverlayVideoEffect)this.Effect;
                    string videoFilePath = await this.ReplaceStringWithSpecialModifiers(videoEffect.FilePath, user, arguments);
                    if (!Uri.IsWellFormedUriString(videoFilePath, UriKind.RelativeOrAbsolute))
                    {
                        videoFilePath = videoFilePath.ToFilePathString();
                    }

                    if (!string.IsNullOrEmpty(videoFilePath))
                    {
                        OverlayVideoEffect copy = videoEffect.Copy<OverlayVideoEffect>();
                        copy.FilePath = videoFilePath;
                        await ChannelSession.Services.OverlayServer.SendLocalVideo(copy);
                    }
                }
                else if (this.Effect is OverlayHTMLEffect)
                {
                    OverlayHTMLEffect htmlEffect = (OverlayHTMLEffect)this.Effect;
                    string htmlText = await this.ReplaceStringWithSpecialModifiers(htmlEffect.HTMLText, user, arguments);
                    OverlayHTMLEffect copy = htmlEffect.Copy<OverlayHTMLEffect>();
                    copy.HTMLText = htmlText;
                    await ChannelSession.Services.OverlayServer.SendHTML(copy);
                }
                else if (this.Effect is OverlayWebPageEffect)
                {
                    OverlayWebPageEffect webPageEffect = (OverlayWebPageEffect)this.Effect;
                    string url = await this.ReplaceStringWithSpecialModifiers(webPageEffect.URL, user, arguments);
                    OverlayWebPageEffect copy = webPageEffect.Copy<OverlayWebPageEffect>();
                    copy.URL = url;
                    await ChannelSession.Services.OverlayServer.SendWebPage(copy);
                }
            }
        }
    }
}
