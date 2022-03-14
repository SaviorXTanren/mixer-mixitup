namespace MixItUp.API.V2.Models
{
    public class StreamingSoftwareAction : ActionBase
    {
        public string StreamingSoftwareType { get; set; }
        public string ActionType { get; set; }
        public string ItemName { get; set; }
        public string ParentName { get; set; }
        public bool Visible { get; set; }
        public string SourceText { get; set; }
        public string SourceTextFilePath { get; set; }
        public string SourceURL { get; set; }
        public StreamingSoftwareSourceDimensions SourceDimensions { get; set; }
    }
}
