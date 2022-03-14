namespace MixItUp.API.V2.Models
{
    public class SpecialIdentifierAction : ActionBase
    {
        public string SpecialIdentifierName { get; set; }
        public string ReplacementText { get; set; }
        public bool MakeGloballyUsable { get; set; }
        public bool ShouldProcessMath { get; set; }
    }
}
