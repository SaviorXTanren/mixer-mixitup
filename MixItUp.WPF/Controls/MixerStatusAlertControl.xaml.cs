using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for MixerStatusAlertControl.xaml
    /// </summary>
    public partial class MixerStatusAlertControl : UserControl
    {
        public MixerStatusAlertControl()
        {
            InitializeComponent();

            this.Loaded += MixerStatusAlertControl_Loaded;
        }

        private void MixerStatusAlertControl_Loaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await this.CheckMixerStatus());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void MixerStatusAlertButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://status.mixer.com/"); }

        private async Task CheckMixerStatus()
        {
            while (true)
            {
                try
                {
                    IEnumerable<MixerIncident> incidents = await ChannelSession.Services.MixerStatus.GetCurrentIncidents();

                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        if (incidents != null && incidents.Count() > 0)
                        {
                            this.MixerStatusAlertButton.Visibility = Visibility.Visible;
                            StringBuilder tooltip = new StringBuilder();
                            tooltip.AppendLine("Mixer Status Alert:");
                            foreach (MixerIncident incident in incidents)
                            {
                                tooltip.AppendLine();
                                tooltip.AppendLine(string.Format("{0} - Last Updated: {1}", incident.Title, incident.LastUpdate.ToString("G")));
                            }
                            this.MixerStatusAlertButton.ToolTip = tooltip.ToString();
                        }
                        else
                        {
                            this.MixerStatusAlertButton.Visibility = Visibility.Hidden;
                        }
                    });

                    await Task.Delay(60000);
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }
    }
}
