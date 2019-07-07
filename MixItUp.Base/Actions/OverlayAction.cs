using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
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
    [DataContract]
    public class OverlayAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OverlayAction.asyncSemaphore; } }

        [DataMember]
        [Obsolete]
        public OverlayEffectBase Effect { get; set; }
        [DataMember]
        [Obsolete]
        public OverlayItemBase Item { get; set; }
        [DataMember]
        [Obsolete]
        public OverlayItemPosition Position { get; set; }
        [DataMember]
        [Obsolete]
        public OverlayItemEffects Effects { get; set; }

        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public Guid WidgetID { get; set; }
        [DataMember]
        public bool ShowWidget { get; set; }

        public OverlayAction() : base(ActionTypeEnum.Overlay) { }

        public OverlayAction(string overlayName, OverlayItemModelBase overlayItem)
            : this()
        {
            this.OverlayName = overlayName;
            this.OverlayItem = overlayItem;
        }

        public OverlayAction(Guid widgetID, bool showWidget)
            : this()
        {
            this.WidgetID = widgetID;
            this.ShowWidget = showWidget;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.WidgetID != Guid.Empty)
            {
                OverlayWidgetModel widget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(this.WidgetID));
                if (widget != null)
                {
                    widget.IsEnabled = this.ShowWidget;
                    if (this.ShowWidget)
                    {
                        await widget.ShowItem(user, arguments, this.extraSpecialIdentifiers);
                    }
                    else
                    {
                        await widget.HideItem();
                    }
                }
            }
            else
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.Item != null)
                {
                    StoreCommandUpgrader.RestructureNewerOverlayActions(new List<ActionBase>() { this });
                }
#pragma warning restore CS0612 // Type or member is obsolete

                string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ChannelSession.Services.OverlayServers.DefaultOverlayName : this.OverlayName;
                IOverlayService overlay = ChannelSession.Services.OverlayServers.GetOverlay(overlayName);
                if (overlay != null)
                {
                    await overlay.ShowItem(this.OverlayItem, user, arguments, this.extraSpecialIdentifiers);
                }
            }
        }
    }

    #region Obsolete Overlay Effect System

    [Obsolete]
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
    }

    [Obsolete]
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
                    return string.Format("http://localhost:8111/overlay/files/{0}?nonce={1}", this.ID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageEffect() { }
    }

    [Obsolete]
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
        public string ID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("http://localhost:8111/overlay/files/{0}", this.ID);
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayVideoEffect() { }
    }

    [Obsolete]
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
    }

    [Obsolete]
    [DataContract]
    public class OverlayHTMLEffect : OverlayEffectBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLEffect() { }
    }

    [Obsolete]
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
    }

    [Obsolete]
    [DataContract]
    public class OverlayEffectBase
    {
        [DataMember]
        public int EntranceAnimation { get; set; }
        [DataMember]
        public int VisibleAnimation { get; set; }
        [DataMember]
        public int ExitAnimation { get; set; }

        [DataMember]
        public double Duration;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        public OverlayEffectBase() { }

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
                name = values[RandomHelper.GenerateRandomNumber(values.Count)].ToString();
            }

            if (!string.IsNullOrEmpty(name) && !name.Equals("None"))
            {
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return string.Empty;
        }
    }

    #endregion Obsolete Overlay Effect System
}
