using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Remote
{
    /// <summary>
    /// Interaction logic for RemoteCommandItemControl.xaml
    /// </summary>
    public partial class RemoteCommandItemControl : UserControl
    {
        public RemoteCommandItemControl()
        {
            InitializeComponent();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageCenter.Send<RemoteCommandItemViewModel>(RemoteCommandItemViewModel.RemoteCommandDetailsEventName, (RemoteCommandItemViewModel)this.DataContext);
        }
    }
}
