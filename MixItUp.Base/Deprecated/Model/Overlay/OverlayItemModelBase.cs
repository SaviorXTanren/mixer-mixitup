using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public enum OverlayItemModelTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        WebPage,
        ProgressBar,
        EventList,
        GameQueue,
        ChatMessages,
        [Obsolete]
        StreamClip,
        Leaderboard,
        Timer,
        TimerTrain,
        StreamBoss,
        [Obsolete]
        SongRequests,
        TickerTape,
        [Obsolete]
        SparkCrystal,
        EndCredits,
        Sound,
        ClipPlayback,
    }

    [Obsolete]
    public enum OverlayItemPositionType
    {
        Percentage,
        Pixel,
    }

    [Obsolete]
    public enum OverlayItemEffectEntranceAnimationTypeEnum
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
    public enum OverlayItemEffectVisibleAnimationTypeEnum
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
    public enum OverlayItemEffectExitAnimationTypeEnum
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
    public abstract class OverlayItemModelBase
    {
        public static string GetFileFullLink(string fileID, string fileType, string filePath)
        {
            if (!ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                return string.Format("/overlay/files/{0}/{1}?nonce={2}", fileType, fileID, Guid.NewGuid());
            }
            return filePath;
        }

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

        [JsonIgnore]
        public bool IsEnabled { get; private set; }

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

        public virtual Task LoadTestData() { return Task.CompletedTask; }

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.CompletedTask;
        }

        public virtual Task Reset() { return Task.CompletedTask; }

        public virtual Task Enable()
        {
            this.IsEnabled = true;
            return Task.CompletedTask;
        }

        public virtual Task Disable()
        {
            if (this.IsEnabled)
            {
                this.IsEnabled = false;
                this.SendChangeState(newState: false);
            }
            return Task.CompletedTask;
        }

        public virtual async Task<JObject> GetProcessedItem(CommandParametersModel parameters)
        {
            JObject jobj = JObject.FromObject(this);
            await this.PerformReplacements(jobj, parameters);
            return jobj;
        }

        public virtual Task LoadCachedData() { return Task.CompletedTask; }

        protected virtual async Task PerformReplacements(JObject jobj, CommandParametersModel parameters)
        {
            if (jobj != null)
            {
                foreach (string key in jobj.GetKeys())
                {
                    if (jobj[key].Type == JTokenType.String)
                    {
                        jobj[key] = await ReplaceStringWithSpecialModifiers(jobj[key].ToString(), parameters);
                    }
                    else if (jobj[key].Type == JTokenType.Object)
                    {
                        await this.PerformReplacements((JObject)jobj[key], parameters);
                    }
                }
            }
        }

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, CommandParametersModel parameters)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, encode: false);
            await siString.ReplaceCommonSpecialModifiers(parameters);
            return siString.ToString();
        }

        protected void SendChangeState(bool newState) { this.OnChangeState(this, newState); }
        protected void SendUpdateRequired() { this.OnSendUpdateRequired(this, new EventArgs()); }
        protected void SendHide() { this.OnHide(this, new EventArgs()); }
    }

    [Obsolete]
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

    [Obsolete]
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

    #region Obsolete

    [Obsolete]
    public class OverlayStreamClipItemModel : OverlayFileItemModelBase
    {
        public OverlayStreamClipItemModel() : base() { }

        public OverlayStreamClipItemModel(int width, int height, int volume, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation) { }

        public override string FileType { get { return "video"; } set { } }
    }

    #endregion Obsolete
}
