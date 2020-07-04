using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChangelogControl.xaml
    /// </summary>
    public partial class ChangelogControl : MainControlBase
    {
        public ChangelogControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.ChangelogWebBrowser.Navigate("file:///" + Path.GetFullPath("Changelog.html"));
            return base.InitializeInternal();
        }
    }
}
