namespace MixItUp.API.V2.Models
{
    public class OverlayTextItem : OverlayItem
    {
        public bool Bold { get; set; }
        public string Color { get; set; }
        public string Font { get; set; }
        public bool Italic { get; set; }
        public string ShadowColor { get; set; }
        public int Size { get; set; }
        public string Text { get; set; }
        public bool Underline { get; set; }
    }
}
