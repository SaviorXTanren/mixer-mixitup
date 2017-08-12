using Mixer.Base.Clients;
using MixItUp.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using Mixer.Base.Util;

namespace MixItUp.WPF.Controls.Events
{
    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        private ObservableCollection<string> eventTypes = new ObservableCollection<string>();

        private List<ConstellationEventType> subscribedEvents = new List<ConstellationEventType>();

        public EventsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            if (await MixerAPIHandler.InitializeConstellationClient())
            {
                if (this.subscribedEvents.Count > 0)
                {
                    await MixerAPIHandler.ConstellationClient.LiveSubscribe(this.subscribedEvents);
                }


                foreach (string name in EnumHelper.GetEnumNames<ConstellationEventTypeEnum>())
                {
                    eventTypes.Add(name);
                }
            }
        }

        private void ExportDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
