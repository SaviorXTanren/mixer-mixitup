using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for SongRequestControl.xaml
    /// </summary>
    public partial class SongRequestControl : MainControlBase
    {
        private SongRequestsMainControlViewModel viewModel;

        public SongRequestControl()
        {
            InitializeComponent();
        }
    }
}
