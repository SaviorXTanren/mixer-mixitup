using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayItemModelTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        [Name("Web Page")]
        WebPage,
        [Name("Goal/Progress Bar")]
        ProgressBar,
        [Name("Event List")]
        EventList,
        [Name("Game Queue")]
        GameQueue,
        [Name("Chat Messages")]
        ChatMessages,
        [Name("Stream Clip Playback")]
        StreamClip,
        Leaderboard,
        Timer,
        [Name("Timer Train")]
        TimerTrain,
        [Name("Stream Boss")]
        StreamBoss,
        [Name("Song Requests")]
        SongRequests,
        [Name("Ticker Tape")]
        TickerTape,
        [Name("Spark Crystal")]
        SparkCrystal
    }

    public enum OverlayItemPositionType
    {
        Percentage,
        Pixel,
    }

    public enum OverlayItemEffectEntranceAnimationTypeEnum
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

    public enum OverlayItemEffectVisibleAnimationTypeEnum
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

    public enum OverlayItemEffectExitAnimationTypeEnum
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
    public abstract class OverlayItemModelBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public OverlayItemModelTypeEnum ItemType { get; set; }

        [DataMember]
        public OverlayItemPositionModel Position { get; set; }

        [DataMember]
        public OverlayItemEffectsModel Effects { get; set; }

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public event EventHandler<bool> OnChangeState = delegate { };
        public event EventHandler OnSendUpdateRequired = delegate { };
        public event EventHandler OnHide = delegate { };

        public OverlayItemModelBase()
        {
            this.ID = Guid.NewGuid();
        }

        public OverlayItemModelBase(OverlayItemModelTypeEnum itemType)
            : this()
        {
            this.ItemType = itemType;
        }

        [DataMember]
        public string ItemTypeName { get { return this.ItemType.ToString(); } set { } }

        [JsonIgnore]
        public virtual bool SupportsTestData { get { return false; } }

        [JsonIgnore]
        public virtual bool SupportsRefreshUpdating { get { return false; } }

        public virtual Task LoadTestData() { return Task.FromResult(0); }

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.FromResult(0);
        }

        public virtual Task Disable()
        {
            if (this.IsInitialized)
            {
                this.IsInitialized = false;
                this.SendChangeState(newState: false);
            }
            return Task.FromResult(0);
        }

        public virtual async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            JObject jobj = JObject.FromObject(this);
            await this.PerformReplacements(jobj, user, arguments, extraSpecialIdentifiers);
            return jobj;
        }

        public string GetFileFullLink(string fileID, string fileType, string filePath)
        {
            if (!Uri.IsWellFormedUriString(filePath, UriKind.RelativeOrAbsolute))
            {
                return string.Format("/overlay/files/{0}/{1}?nonce={2}", fileType, fileID, Guid.NewGuid());
            }
            return filePath;
        }

        protected virtual async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (jobj != null)
            {
                foreach (string key in jobj.GetKeys())
                {
                    if (jobj[key].Type == JTokenType.String)
                    {
                        jobj[key] = await this.ReplaceStringWithSpecialModifiers(jobj[key].ToString(), user, arguments, extraSpecialIdentifiers);
                    }
                    else if (jobj[key].Type == JTokenType.Object)
                    {
                        await this.PerformReplacements((JObject)jobj[key], user, arguments, extraSpecialIdentifiers);
                    }
                }
            }
        }

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, encode: false);
            if (extraSpecialIdentifiers != null)
            {
                foreach (var kvp in extraSpecialIdentifiers)
                {
                    siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
                }
            }
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            return siString.ToString();
        }

        protected void SendChangeState(bool newState) { this.OnChangeState(this, newState); }
        protected void SendUpdateRequired() { this.OnSendUpdateRequired(this, new EventArgs()); }
        protected void SendHide() { this.OnHide(this, new EventArgs()); }
    }

    [DataContract]
    public class OverlayItemPositionModel
    {
        [DataMember]
        public OverlayItemPositionType PositionType;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;
        [DataMember]
        public int Layer;

        [DataMember]
        public bool IsPercentagePosition { get { return this.PositionType == OverlayItemPositionType.Percentage; } }
        [DataMember]
        public bool IsPixelPosition { get { return this.PositionType == OverlayItemPositionType.Pixel; } }

        public OverlayItemPositionModel() { }

        public OverlayItemPositionModel(OverlayItemPositionType positionType, int horizontal, int vertical, int layer)
        {
            this.PositionType = positionType;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.Layer = layer;
        }
    }

    [DataContract]
    public class OverlayItemEffectsModel
    {
        [DataMember]
        public OverlayItemEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return OverlayItemEffectsModel.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayItemEffectVisibleAnimationTypeEnum VisibleAnimation { get; set; }
        [DataMember]
        public string VisibleAnimationName { get { return OverlayItemEffectsModel.GetAnimationClassName(this.VisibleAnimation); } set { } }
        [DataMember]
        public OverlayItemEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return OverlayItemEffectsModel.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public double Duration;

        public OverlayItemEffectsModel() { }

        public OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum entrance, OverlayItemEffectVisibleAnimationTypeEnum visible, OverlayItemEffectExitAnimationTypeEnum exit, double duration)
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
}
