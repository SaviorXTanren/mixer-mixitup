using Mixer.Base.Model.Interactive;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveBoardControl.xaml
    /// </summary>
    public partial class InteractiveBoardControl : UserControl
    {
        private const int LargeWidth = 80;
        private const int LargeHeight = 22;

        private const int MediumWidth = 40;
        private const int MediumHeight = 25;

        private const int SmallWidth = 30;
        private const int SmallHeight = 40;

        private InteractiveSceneModel scene;

        public InteractiveBoardControl()
        {
            InitializeComponent();

            this.Loaded += InteractiveBoardControl_Loaded;
        }

        private void InteractiveBoardControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.scene = (InteractiveSceneModel)this.DataContext;
                this.RefreshScene();
            }
        }

        public void RefreshScene()
        {

        }
    }
}
