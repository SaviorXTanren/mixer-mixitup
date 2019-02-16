using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Remote.Items
{
    /// <summary>
    /// Interaction logic for RemoteFolderItemControl.xaml
    /// </summary>
    public partial class RemoteFolderItemControl : UserControl
    {
        public RemoteFolderItemControl()
        {
            InitializeComponent();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageCenter.Send<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderDetailsEventName, (RemoteFolderItemViewModel)this.DataContext);
        }
    }
}
