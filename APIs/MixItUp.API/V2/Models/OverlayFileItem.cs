namespace MixItUp.API.V2.Models
{
    public class OverlayFileItem : OverlayItem
    {
        public string FilePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FileType { get; set; }
        public string FileID { get; set; }
    }
}
