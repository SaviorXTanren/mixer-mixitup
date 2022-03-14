namespace MixItUp.API.V2.Models
{
    public class PixelChatAction : ActionBase
    {
        public string ActionType { get; set; }
        public string SceneID { get; set; }
        public string ComponentID { get; set; }
        public bool SceneComponentVisible { get; set; }
        public string OverlayID { get; set; }
        public string TimeAmount { get; set; }
        public string TargetUsername { get; set; }
    }
}
