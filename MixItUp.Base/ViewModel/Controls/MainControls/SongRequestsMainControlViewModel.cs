using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class SongRequestsMainControlViewModel : WindowControlViewModelBase
    {
        public SongRequestsMainControlViewModel(WindowViewModelBase windowViewModel)
            : base(windowViewModel)
        {
        }
    }
}
