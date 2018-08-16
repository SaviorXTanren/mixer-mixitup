using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.ViewModel.User;
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
        private InteractiveConnectedButtonControlModel drawButton;

        public MixerPaintInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();
        }

        protected override async Task GameConnectedInternal()
        {
            InteractiveConnectedSceneGroupCollectionModel sceneGroups = await ChannelSession.Interactive.GetScenes();
            if (sceneGroups != null)
            {
                this.scene = sceneGroups.scenes.FirstOrDefault();
                if (this.scene != null)
                {
                    this.drawButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("draw"));
                }
            }
        }

        protected override Task OnInteractiveControlUsed(UserViewModel user, InteractiveGiveInputModel input, InteractiveConnectedControlCommand command)
        {
            return Task.FromResult(0);
        }
    }
}
