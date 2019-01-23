namespace MixItUp.API.Models
{
    public class MixPlayBroadcastUser : MixPlayBroadcastTargetBase
    {
        public uint UserID { get; set; }

        public MixPlayBroadcastUser(uint userID)
        {
            UserID = userID;
        }

        public override string ScopeString()
        {
            return $"user:{UserID.ToString()}";
        }
    }
}