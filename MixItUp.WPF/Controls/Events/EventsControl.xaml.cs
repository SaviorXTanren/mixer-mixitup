using Microsoft.Win32;
using Mixer.Base.Clients;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Events
{
    public class SubscribedEventItem : NotifyPropertyChangedBase, IEquatable<SubscribedEventItem>
    {
        public SubscribedEventItem(EventCommand command)
        {
            this.Command = command;
            this.EventsFired = new ObservableCollection<SubscribedFiredEventItem>();
            this.DetailsVisibility = Visibility.Collapsed;
        }

        public EventCommand Command { get; set; }

        public ObservableCollection<SubscribedFiredEventItem> EventsFired { get; set; }

        public Visibility DetailsVisibility
        {
            get { return this.detailsVisibility; }
            set
            {
                this.detailsVisibility = value;
                this.NotifyPropertyChanged("DetailsVisibility");
            }
        }
        private Visibility detailsVisibility;

        public string EventName { get { return this.Command.ToString(); } }

        public string EventsFiredCount { get { return this.EventsFired.Count.ToString(); } }

        public void AddFiredEvent(ConstellationLiveEventModel eventFired)
        {
            this.EventsFired.Add(new SubscribedFiredEventItem(eventFired));
            this.NotifyPropertyChanged("EventsFiredCount");
            this.NotifyPropertyChanged("EventsFired");
        }

        public override bool Equals(object obj)
        {
            if (obj is SubscribedEventItem)
            {
                return this.Equals((SubscribedEventItem)obj);
            }
            return false;
        }

        public bool Equals(SubscribedEventItem other) { return this.Command.Equals(other.Command); }

        public override int GetHashCode() { return this.Command.GetHashCode(); }
    }

    public class SubscribedFiredEventItem : NotifyPropertyChangedBase
    {
        public SubscribedFiredEventItem(ConstellationLiveEventModel eventFired)
        {
            this.EventFired = eventFired;
        }

        public ConstellationLiveEventModel EventFired { get; set; }

        public string EventContents { get { return this.EventFired.payload.ToString(); } }
    }

    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        private ObservableCollection<SubscribedEventItem> subscribedEvents = new ObservableCollection<SubscribedEventItem>();

        public EventsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.EventsCommandsListView.ItemsSource = this.subscribedEvents;

            if (await ChannelSession.InitializeConstellationClient())
            {
                ChannelSession.ConstellationClient.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;
            }

            await this.RefreshList();
        }

        private async Task RefreshList()
        {
            List<SubscribedEventItem> newEvents = new List<SubscribedEventItem>();
            foreach (EventCommand eventCommand in ChannelSession.Settings.EventCommands)
            {
                if (!this.subscribedEvents.Any(se => se.Command.Equals(eventCommand)))
                {
                    newEvents.Add(new SubscribedEventItem(eventCommand));
                }
            }

            if (newEvents.Count > 0)
            {
                await ChannelSession.ConstellationClient.SubscribeToEvents(this.subscribedEvents.Select(se => se.Command.GetEventType()));
                foreach (SubscribedEventItem newEvent in newEvents)
                {
                    this.subscribedEvents.Add(newEvent);
                }
            }
        }

        private async void CommandTestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SubscribedEventItem item = (SubscribedEventItem)button.DataContext;

            await this.Window.RunAsyncOperation(async () =>
            {
                await item.Command.Perform();
            });
        }

        private void CommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SubscribedEventItem item = (SubscribedEventItem)button.DataContext;

            CommandWindow window = new CommandWindow(new EventCommandDetailsControl(item.Command));
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SubscribedEventItem item = (SubscribedEventItem)button.DataContext;
            ChannelSession.Settings.EventCommands.Remove(item.Command);
            this.subscribedEvents.Remove(item);

            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConstellationClient.UnsubscribeToEvents(new List<ConstellationEventType>() { item.Command.GetEventType() });
            });

            await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

            this.EventsCommandsListView.SelectedIndex = -1;

            await this.RefreshList();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new EventCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.RefreshList();
            await ChannelSession.Settings.Save();
        }

        private async void ExportDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Mixer Events";
            dialog.DefaultExt = ".csv";
            dialog.Filter = "CSV Files (.csv)|*.csv";
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    StringBuilder fileData = new StringBuilder();
                    fileData.AppendLine("Type,ID,Event");

                    foreach (SubscribedEventItem eventItem in this.subscribedEvents)
                    {
                        foreach (SubscribedFiredEventItem firedEventItem in eventItem.EventsFired)
                        {
                            fileData.AppendLine(string.Format("{0},{1},{2}", EnumHelper.GetEnumName(eventItem.Command.EventType), eventItem.Command.CommandsString, string.Empty));
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(File.Open(dialog.FileName, FileMode.Create)))
                    {
                        await writer.WriteAsync(fileData.ToString());
                    }
                });
            }
        }

        private void ConstellationClient_OnSubscribedEventOccurred(object sender, ConstellationLiveEventModel e)
        {
            foreach (SubscribedEventItem subscribedEvent in this.subscribedEvents)
            {
                if (subscribedEvent.Command.UniqueEventID.Equals(e.channel))
                {
                    subscribedEvent.AddFiredEvent(e);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    subscribedEvent.Command.Perform();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }
    }
}
