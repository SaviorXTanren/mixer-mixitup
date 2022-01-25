namespace MixItUp.API.V2.Models
{
    public class ExternalProgramAction : ActionBase
    {
        public string FilePath { get; set; }
        public string Arguments { get; set; }
        public bool ShowWindow { get; set; }
        public bool WaitForFinish { get; set; }
        public bool SaveOutput { get; set; }
    }
}
