namespace MixItUp.API.Models
{
    public class MixPlayBroadcastEveryone : MixPlayBroadcastTargetBase
    {
        public MixPlayBroadcastEveryone()
        {
        }

        public override string ScopeString()
        {
            return $"everyone";
        }
    }
}