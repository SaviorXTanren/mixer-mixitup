using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System.IO;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CounterActionEditorControl.xaml
    /// </summary>
    public partial class CounterActionEditorControl : ActionEditorControlBase
    {
        public CounterActionEditorControl()
        {
            InitializeComponent();
        }

        private async void CounterFolderHyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string counterFolderPath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), CounterModel.CounterFolderName);
            if (!Directory.Exists(counterFolderPath))
            {
                await ChannelSession.Services.FileService.CreateDirectory(counterFolderPath);
            }
            ProcessHelper.LaunchFolder(counterFolderPath);
        }
    }
}
