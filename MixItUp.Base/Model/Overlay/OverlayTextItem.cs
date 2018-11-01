using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayTextItem : OverlayItemBase
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

        public OverlayTextItem() { }

        public OverlayTextItem(string text, string color, int size, string font, bool bold, bool italic, bool underline, string shadowColor)
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
    }
}
