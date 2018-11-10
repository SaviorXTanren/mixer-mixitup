using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for GifSkillControl.xaml
    /// </summary>
    public partial class GifSkillControl : UserControl
    {
        public GifSkillControl()
        {
            InitializeComponent();
        }

        public GifSkillControl(string imageURL)
            : base()
        {
            this.DataContext = imageURL;
        }
    }
}
