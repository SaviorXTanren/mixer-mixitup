using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlayTextItemModel : OverlayItemModelBase
    {
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

        public OverlayTextItemModel() : base() { }

        public OverlayTextItemModel(string text, string color, int size, string font, bool bold, bool italic, bool underline, string shadowColor)
            : base(OverlayItemModelTypeEnum.Text)
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

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
