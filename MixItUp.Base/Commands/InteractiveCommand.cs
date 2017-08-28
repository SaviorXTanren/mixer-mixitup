using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;

namespace MixItUp.Base.Commands
{
    public enum InteractiveButtonCommandEventType
    {
        [Name("Mouse Down")]
        MouseDown,
        [Name("Mouse Up")]
        MouseUp,
        [Name("Key Up")]
        KeyUp,
        [Name("Key Down")]
        KeyDown,
    }

    public class InteractiveCommand : CommandBase
    {
        public uint GameID { get; set; }

        public string SceneID { get; set; }

        public bool IsJoystick { get; set; }

        public InteractiveButtonCommandEventType EventType { get; set; }

        public InteractiveCommand() { }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandEventType eventType)
            : base(control.controlID, CommandTypeEnum.Interactive, EnumHelper.GetEnumName(eventType))
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.EventType = eventType;
        }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveJoystickControlModel control)
            : base(control.controlID, CommandTypeEnum.Interactive, string.Empty)
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.IsJoystick = true;
        }

        public string EventTypeTransactionString { get { return this.EventType.ToString().ToLower(); } }
    }
}
