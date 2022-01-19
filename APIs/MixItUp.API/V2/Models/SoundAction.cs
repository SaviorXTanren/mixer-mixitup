namespace MixItUp.API.V2.Models
{
    public class SoundAction : ActionBase
    {
        public string FilePath { get; set; }
        public int VolumeScale { get; set; }
        public string OutputDevice { get; set; }
    }
}
