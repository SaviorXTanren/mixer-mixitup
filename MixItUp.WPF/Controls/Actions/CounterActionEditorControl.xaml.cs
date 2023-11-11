using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
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
            string counterFolderPath = Path.Combine(ServiceManager.Get<IFileService>().GetApplicationDirectory(), CounterModel.CounterFolderName);
            if (!Directory.Exists(counterFolderPath))
            {
                await ServiceManager.Get<IFileService>().CreateDirectory(counterFolderPath);
            }
            ServiceManager.Get<IProcessService>().LaunchFolder(counterFolderPath);
        }
    }
}
