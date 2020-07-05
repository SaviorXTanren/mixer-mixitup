using MixItUp.Base.Util;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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

            GlobalEvents.OnMainMenuStateChanged += GlobalEvents_OnMainMenuStateChanged;
        }

        protected override Task InitializeInternal()
        {
            this.ChangelogWebBrowser.Navigate("file:///" + Path.GetFullPath("Changelog.html"));
            return base.InitializeInternal();
        }

        private void GlobalEvents_OnMainMenuStateChanged(object sender, bool state)
        {
            this.ChangelogWebBrowser.Visibility = (state) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
