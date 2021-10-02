using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayCustomHTMLItem : OverlayItemBase
    {
        public const string CustomItemType = "custom";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayCustomHTMLItem() : base(OverlayCustomHTMLItem.CustomItemType) { }

        public OverlayCustomHTMLItem(string htmlTemplate) : this(OverlayCustomHTMLItem.CustomItemType, htmlTemplate) { }

        public OverlayCustomHTMLItem(string type, string htmlTemplate)
            : base(type)
        {
            this.HTMLText = htmlTemplate;
        }

        public virtual OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayCustomHTMLItem>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayCustomHTMLItem item = this.GetCopy();
            item.HTMLText = await this.PerformReplacement(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }

        protected virtual async Task<string> PerformReplacement(string text, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            foreach (var kvp in await this.GetReplacementSets(user, arguments, extraSpecialIdentifiers))
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return await ReplaceStringWithSpecialModifiers(text, user, arguments, extraSpecialIdentifiers);
        }

        protected virtual Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayHTMLItem : OverlayItemBase
    {
        public const string HTMLItemType = "html";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLItem() : base(HTMLItemType) { }

        public OverlayHTMLItem(string htmlText)
            : base(HTMLItemType)
        {
            this.HTMLText = htmlText;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayHTMLItem item = this.Copy<OverlayHTMLItem>();
            item.HTMLText = await ReplaceStringWithSpecialModifiers(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayImageItem : OverlayItemBase
    {
        public const string ImageItemType = "image";

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}?nonce={1}", this.FileID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageItem() : base(ImageItemType) { }

        public OverlayImageItem(string filepath, int width, int height)
            : base(ImageItemType)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayImageItem item = this.Copy<OverlayImageItem>();
            item.FilePath = await ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public abstract class OverlayItemBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string ItemType { get; set; }

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public OverlayItemBase()
        {
            this.ID = Guid.NewGuid();
        }

        public OverlayItemBase(string itemType)
            : this()
        {
            this.ItemType = itemType;
        }

        [JsonIgnore]
        public virtual bool SupportsTestButton { get { return false; } }

        public virtual Task LoadTestData() { return Task.FromResult(0); }

        public abstract Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.FromResult(0);
        }

        public virtual Task Disable()
        {
            this.IsInitialized = false;
            return Task.FromResult(0);
        }

        public T Copy<T>() { return JSONSerializerHelper.DeserializeFromString<T>(JSONSerializerHelper.SerializeToString(this)); }

        protected Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            return Task.FromResult(string.Empty);
        }
    }

    [Obsolete]
    public enum OverlayEffectEntranceAnimationTypeEnum
    {
        None,

        BounceIn,
        BounceInUp,
        BounceInDown,
        BounceInLeft,
        BounceInRight,

        FadeIn,
        FadeInUp,
        FadeInDown,
        FadeInLeft,
        FadeInRight,

        FlipInX,
        FlipInY,

        LightSpeedIn,

        RotateIn,


        [Obsolete]
        RotateInUp,
        [Obsolete]
        RotateInDown,
        [Obsolete]
        RotateInLeft,
        [Obsolete]
        RotateInRight,


        SlideInUp,
        SlideInDown,
        SlideInLeft,
        SlideInRight,

        ZoomIn,
        ZoomInUp,
        ZoomInDown,
        ZoomInLeft,
        ZoomInRight,

        JackInTheBox,

        RollIn,

        Random,
    }

    [Obsolete]
    public enum OverlayEffectVisibleAnimationTypeEnum
    {
        None,

        Bounce,
        Flash,
        Pulse,
        RubberBand,
        Shake,
        Swing,
        Tada,
        Wobble,
        Jello,
        Flip,

        Random,
    }

    [Obsolete]
    public enum OverlayEffectExitAnimationTypeEnum
    {
        None,

        BounceOut,
        BounceOutUp,
        BounceOutDown,
        BounceOutLeft,
        BounceOutRight,

        FadeOut,
        FadeOutUp,
        FadeOutDown,
        FadeOutLeft,
        FadeOutRight,

        FlipOutX,
        FlipOutY,

        LightSpeedOut,

        RotateOut,


        [Obsolete]
        RotateOutUp,
        [Obsolete]
        RotateOutDown,
        [Obsolete]
        RotateOutLeft,
        [Obsolete]
        RotateOutRight,


        SlideOutUp,
        SlideOutDown,
        SlideOutLeft,
        SlideOutRight,

        ZoomOut,
        ZoomOutUp,
        ZoomOutDown,
        ZoomOutLeft,
        ZoomOutRight,

        Hinge,

        RollOut,

        Random,
    }

    [Obsolete]
    [DataContract]
    public class OverlayItemEffects
    {
        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayEffectVisibleAnimationTypeEnum VisibleAnimation { get; set; }
        [DataMember]
        public string VisibleAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.VisibleAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public double Duration;

        public OverlayItemEffects() { }

        public OverlayItemEffects(OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible, OverlayEffectExitAnimationTypeEnum exit, double duration)
        {
            this.EntranceAnimation = entrance;
            this.VisibleAnimation = visible;
            this.ExitAnimation = exit;
            this.Duration = duration;
        }

        public static string GetAnimationClassName<T>(T animationType)
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
                name = values[RandomHelper.GenerateRandomNumber(values.Count)].ToString();
            }

            if (!string.IsNullOrEmpty(name) && !name.Equals("None"))
            {
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return string.Empty;
        }
    }

    [Obsolete]
    public enum OverlayEffectPositionType
    {
        Percentage,
        Pixel,
    }

    [Obsolete]
    public class OverlayItemPosition
    {
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

        public OverlayItemPosition() { }

        public OverlayItemPosition(OverlayEffectPositionType positionType, int horizontal, int vertical)
        {
            this.PositionType = positionType;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayTextItem : OverlayItemBase
    {
        public const string TextItemType = "text";

        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int Size { get; set; }
        [DataMember]
        public string Font { get; set; }
        [DataMember]
        public bool Bold { get; set; }
        [DataMember]
        public bool Underline { get; set; }
        [DataMember]
        public bool Italic { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }

        public OverlayTextItem() : base(TextItemType) { }

        public OverlayTextItem(string text, string color, int size, string font, bool bold, bool italic, bool underline, string shadowColor)
            : base(TextItemType)
        {
            this.Text = text;
            this.Color = color;
            this.Size = size;
            this.Font = font;
            this.Bold = bold;
            this.Underline = underline;
            this.Italic = italic;
            this.ShadowColor = shadowColor;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayTextItem item = this.Copy<OverlayTextItem>();
            item.Text = await ReplaceStringWithSpecialModifiers(item.Text, user, arguments, extraSpecialIdentifiers);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayVideoItem : OverlayItemBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        public const string VideoItemType = "video";

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}", this.FileID);
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItem() : base(VideoItemType) { this.Volume = 100; }

        public OverlayVideoItem(string filepath, int width, int height, int volume)
            : base(VideoItemType)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayVideoItem item = this.Copy<OverlayVideoItem>();
            item.FilePath = await ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayWebPageItem : OverlayItemBase
    {
        public const string WebPageItemType = "webpage";

        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayWebPageItem() : base(WebPageItemType) { }

        public OverlayWebPageItem(string url, int width, int height)
            : base(WebPageItemType)
        {
            this.URL = url;
            this.Width = width;
            this.Height = height;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayWebPageItem item = this.Copy<OverlayWebPageItem>();
            item.URL = await ReplaceStringWithSpecialModifiers(item.URL, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayYouTubeItem : OverlayItemBase
    {
        private const string YouTubeItemType = "youtube";

        [DataMember]
        public string VideoID { get; set; }
        [DataMember]
        public int StartTime { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        public OverlayYouTubeItem() : base(YouTubeItemType) { this.Volume = 100; }

        public OverlayYouTubeItem(string id, int startTime, int width, int height, int volume)
            : base(YouTubeItemType)
        {
            this.VideoID = id;
            this.StartTime = startTime;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayYouTubeItem item = this.Copy<OverlayYouTubeItem>();
            item.VideoID = await ReplaceStringWithSpecialModifiers(item.VideoID, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayClipPlaybackItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public bool Muted { get; set; }

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.None;

        private Twitch.Base.Models.NewAPI.Clips.ClipModel lastTwitchClip = null;
        private string lastClipURL = null;

        public OverlayClipPlaybackItemModel() : base() { }

        public OverlayClipPlaybackItemModel(int width, int height, bool muted, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.ClipPlayback, string.Empty, width, height)
        {
            this.Width = width;
            this.Height = height;
            this.Muted = muted;
            this.Effects = new OverlayItemEffectsModel(entranceAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, exitAnimation, 0);
        }

        [DataMember]
        public override string FullLink { get { return this.lastClipURL; } set { } }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public string PlatformName { get { return this.Platform.ToString(); } set { } }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override Task LoadTestData()
        {
            this.Platform = StreamingPlatformTypeEnum.All;
            this.Effects.Duration = this.Duration = 3;
            this.lastClipURL = "https://clips.twitch.tv/embed?clip=HotAmazonianKoupreyNotATK&parent=localhost";

            return Task.FromResult(0);
        }

        public override async Task Enable()
        {
            GlobalEvents.OnTwitchClipCreated += GlobalEvents_OnTwitchClipCreated;

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnTwitchClipCreated -= GlobalEvents_OnTwitchClipCreated;

            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(CommandParametersModel parameters)
        {
            JObject jobj = null;
            if (this.lastTwitchClip != null)
            {
                this.Platform = StreamingPlatformTypeEnum.Twitch;
                this.lastClipURL = this.lastTwitchClip.embed_url + "&parent=localhost";
                if (this.Muted)
                {
                    this.lastClipURL += "&muted=true";
                }
                this.Effects.Duration = this.Duration = 30;

                this.lastTwitchClip = null;
            }

            if (!string.IsNullOrEmpty(this.lastClipURL))
            {
                jobj = await base.GetProcessedItem(parameters);
            }

            this.Platform = StreamingPlatformTypeEnum.None;
            this.lastClipURL = null;

            return jobj;
        }

        private void GlobalEvents_OnTwitchClipCreated(object sender, Twitch.Base.Models.NewAPI.Clips.ClipModel clip)
        {
            this.lastTwitchClip = clip;
            this.SendUpdateRequired();
        }
    }
}
