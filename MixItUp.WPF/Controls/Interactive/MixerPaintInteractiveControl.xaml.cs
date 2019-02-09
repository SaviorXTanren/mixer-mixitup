using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for MixerPaintInteractiveControl.xaml
    /// </summary>
    public partial class MixerPaintInteractiveControl : CustomInteractiveGameControl
    {
        private InteractiveConnectedSceneModel scene;
        private InteractiveConnectedButtonControlModel sendButton;
        private InteractiveConnectedButtonControlModel presentButton;

        private Dictionary<UserViewModel, string> userDrawings = new Dictionary<UserViewModel, string>();

        public MixerPaintInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();
        }

        protected override async Task<bool> GameConnectedInternal()
        {
            InteractiveConnectedSceneGroupCollectionModel sceneGroups = await ChannelSession.Interactive.GetScenes();
            if (sceneGroups != null)
            {
                this.scene = sceneGroups.scenes.FirstOrDefault();
                if (this.scene != null)
                {
                    this.sendButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("send"));
                    this.presentButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("present"));
                    if (this.sendButton != null && this.presentButton != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override async Task OnInteractiveControlUsed(UserViewModel user, InteractiveGiveInputModel input, InteractiveConnectedControlCommand command)
        {
            if (user != null && !user.IsAnonymous && input.input.meta.ContainsKey("image"))
            {
                this.userDrawings[user] = input.input.meta["image"].ToString();

                InteractiveConnectedButtonControlModel control = new InteractiveConnectedButtonControlModel() { controlID = this.presentButton.controlID };
                control.meta["map"] = this.userDrawings[user];
                await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });
            }
        }
    }
}
