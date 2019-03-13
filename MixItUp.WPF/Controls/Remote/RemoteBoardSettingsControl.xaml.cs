using MixItUp.Base.ViewModel.Controls.Remote;
using MixItUp.Base.ViewModel.Remote;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Remote
{
    /// <summary>
    /// Interaction logic for RemoteBoardSettingsControl.xaml
    /// </summary>
    public partial class RemoteBoardSettingsControl : UserControl
    {
        public RemoteBoardSettingsControl(RemoteProfileViewModel profile, RemoteBoardViewModel board)
        {
            this.DataContext = new RemoteBoardSettingsControlViewModel(profile, board);

            InitializeComponent();
        }
    }
}
