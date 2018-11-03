using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetEditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetEditorWindow : LoadingWindowBase
    {
        public OverlayWidget Widget { get; private set; }

        public OverlayWidgetEditorWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public OverlayWidgetEditorWindow(OverlayWidget widget)
            : this()
        {
            this.Widget = widget;
        }

        protected override async Task OnLoaded()
        {
            await base.OnLoaded();
        }
    }
}
