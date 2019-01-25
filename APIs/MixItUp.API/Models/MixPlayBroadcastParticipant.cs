namespace MixItUp.API.Models
{
    public class MixPlayBroadcastParticipant : MixPlayBroadcastTargetBase
    {
        public string ParticipantID { get; set; }

        public MixPlayBroadcastParticipant(string participantID)
        {
            ParticipantID = participantID;
        }

        public override string ScopeString()
        {
            return $"participant:{ParticipantID}";
        }
    }
}