namespace MixItUp.API.V2.Models
{
    public class VTubeStudioAction : ActionBase
    {
        public string ActionType { get; set; }
        public string ModelID { get; set; }
        public double MovementTimeInSeconds { get; set; }
        public bool MovementRelative { get; set; }
        public double? MovementX { get; set; }
        public double? MovementY { get; set; }
        public double? Rotation { get; set; }
        public double? Size { get; set; }
        public string HotKeyID { get; set; }
    }
}
