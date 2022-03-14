namespace MixItUp.API.V2.Models
{
    public class FileAction : ActionBase
    {
        public string ActionType { get; set; }
        public string FilePath { get; set; }
        public string TransferText { get; set; }
        public string LineIndex { get; set; }
    }
}
