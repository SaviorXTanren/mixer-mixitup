using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls
{
    public class StreamingPlatformStatusAlertViewModel : UIViewModelBase
    {
        public bool Show { get { return !string.IsNullOrWhiteSpace(this.ToolTipText); } }

        public string ToolTipText
        {
            get { return this.toolTipText; }
            set
            {
                this.toolTipText = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("Show");
            }
        }
        private string toolTipText;

        public ICommand LaunchStatusPageCommand { get; private set; }

        private List<StreamingPlatformStatusModel> incidents = new List<StreamingPlatformStatusModel>();

        public StreamingPlatformStatusAlertViewModel()
        {
            this.LaunchStatusPageCommand = this.CreateCommand((parameter) =>
            {
                if (this.Show)
                {
                    StreamingPlatformStatusModel status = this.incidents.FirstOrDefault();
                    if (status != null && !string.IsNullOrEmpty(status.Link))
                    {
                        ServiceManager.Get<IProcessService>().LaunchLink(status.Link);
                    }
                }
                return Task.CompletedTask;
            });
        }

        protected override Task OnOpenInternal()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        List<Task<IEnumerable<StreamingPlatformStatusModel>>> incidentTasks = new List<Task<IEnumerable<StreamingPlatformStatusModel>>>();

                        incidentTasks.Add(ServiceManager.Get<TwitchStatusService>().GetCurrentIncidents());

                        await Task.WhenAll(incidentTasks);

                        this.incidents.Clear();
                        this.incidents.AddRange(incidentTasks.SelectMany(t => t.Result));
                        if (this.incidents.Count > 0)
                        {
                            List<string> incidentGroups = new List<string>();
                            foreach (var incidents in this.incidents.GroupBy(g => g.Platform))
                            {
                                List<string> incidentGroup = new List<string>();
                                incidentGroup.Add($"{incidents.Key} Status Alert:");
                                foreach (StreamingPlatformStatusModel incident in incidents)
                                {
                                    incidentGroup.Add($"{incident.Title} ({incident.LastUpdated.ToString("G")}): {incident.Description}".AddNewLineEveryXCharacters(70));
                                }
                                incidentGroups.Add(string.Join(Environment.NewLine + Environment.NewLine, incidentGroup));
                            }
                            this.ToolTipText = string.Join(Environment.NewLine + Environment.NewLine, incidentGroups);
                        }
                        else
                        {
                            this.ToolTipText = string.Empty;
                        }

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            if (!string.IsNullOrWhiteSpace(this.ToolTipText))
                            {
                                this.ToolTipText += Environment.NewLine + Environment.NewLine;
                            }
                            this.ToolTipText += MixItUp.Base.Resources.DiagnosticLoggingEnabledWarningTooltip;
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }

                    await Task.Delay(60000);
                }
            });
            return Task.CompletedTask;
        }

        private void GlobalEvents_OnRefreshWarningUI(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for StreamingPlatformStatusAlertControl.xaml
    /// </summary>
    public partial class StreamingPlatformStatusAlertControl : UserControl
    {
        private StreamingPlatformStatusAlertViewModel viewModel;

        public StreamingPlatformStatusAlertControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new StreamingPlatformStatusAlertViewModel();

            this.Loaded += StreamingPlatformStatusAlertControl_Loaded;
        }

        private async void StreamingPlatformStatusAlertControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.viewModel.OnOpen();
        }
    }
}
