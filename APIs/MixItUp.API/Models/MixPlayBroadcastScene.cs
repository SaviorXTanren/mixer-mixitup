namespace MixItUp.API.Models
{
    public class MixPlayBroadcastScene : MixPlayBroadcastTargetBase
    {
        public string SceneID { get; set; }

        public MixPlayBroadcastScene(string sceneID)
        {
            SceneID = sceneID;
        }

        public override string ScopeString()
        {
            return $"scene:{SceneID}";
        }
    }
}