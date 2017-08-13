using Microsoft.Win32;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Events
{
    public class SubscribedEventItem : NotifyPropertyChangedBase, IEquatable<SubscribedEventItem>
    {
        public SubscribedEventItem(SubscribedEventViewModel eventType)
        {
            this.EventType = eventType;
            this.EventsFired = new ObservableCollection<SubscribedFiredEventItem>();
            this.DetailsVisibility = Visibility.Collapsed;
        }

        public SubscribedEventViewModel EventType { get; set; }

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

        public string EventName { get { return this.EventType.ToString(); } }

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

        public bool Equals(SubscribedEventItem other) { return this.EventType.Equals(other.EventType); }

        public override int GetHashCode() { return this.EventType.GetHashCode(); }
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
        private ObservableCollection<string> eventTypes = new ObservableCollection<string>();

        private ObservableCollection<SubscribedEventItem> subscribedEvents = new ObservableCollection<SubscribedEventItem>();

        public EventsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            if (await MixerAPIHandler.InitializeConstellationClient())
            {
                MixerAPIHandler.ConstellationClient.OnSubscribedEventOccurred += ConstellationClient_OnSubscribedEventOccurred;

                foreach (SubscribedEventViewModel subscribedEvent in MixerAPIHandler.Settings.SubscribedEvents)
                {
                    this.subscribedEvents.Add(new SubscribedEventItem(subscribedEvent));
                }

                if (this.subscribedEvents.Count > 0)
                {
                    await MixerAPIHandler.ConstellationClient.LiveSubscribe(this.subscribedEvents.Select(se => se.EventType.GetEventType()));
                }

                this.SubscribedEventsList.ItemsSource = this.subscribedEvents;

                this.EventTypeComboBox.ItemsSource = this.eventTypes;
                foreach (string name in EnumHelper.GetEnumNames<ConstellationEventTypeEnum>())
                {
                    if (name.ToString().Contains("Channel") || name.ToString().Contains("User"))
                    {
                        this.eventTypes.Add(name);
                    }
                }
            }
        }

        private void SubscribedEventsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                SubscribedEventItem eventItem = (SubscribedEventItem)item;
                eventItem.DetailsVisibility = Visibility.Visible;
            }

            foreach (var item in e.RemovedItems)
            {
                SubscribedEventItem eventItem = (SubscribedEventItem)item;
                eventItem.DetailsVisibility = Visibility.Collapsed;
            }
        }

        private async void RemoveSubscribedEvent_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)e.OriginalSource;
            SubscribedEventItem subscribedEventItem = (SubscribedEventItem)button.DataContext;

            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.ConstellationClient.LiveUnsubscribe(new List<ConstellationEventType>() { subscribedEventItem.EventType.GetEventType() });
            });

            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to unsubscribe from event, please try again");
                return;
            }

            this.subscribedEvents.Remove(subscribedEventItem);
            MixerAPIHandler.Settings.SubscribedEvents.Remove(subscribedEventItem.EventType);

            await this.Window.RunAsyncOperation(async () =>
            {
                await MixerAPIHandler.SaveSettings();
            });
        }

        private void EventIDFinderTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.AddEventButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void AddEventButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.EventIDFinderTextBox.Visibility == Visibility.Visible && string.IsNullOrEmpty(this.EventIDFinderTextBox.Text))
            {
                MessageBoxHelper.ShowError("A name must be specified for this event type");
                return;
            }

            ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);
            string eventIDName = this.EventIDFinderTextBox.Text;

            SubscribedEventViewModel newEventType = null;
            await this.Window.RunAsyncOperation(async () =>
            {
                if (eventType.ToString().Contains("channel"))
                {
                    ChannelAdvancedModel channel = await MixerAPIHandler.MixerConnection.Channels.GetChannel(eventIDName);
                    if (channel != null)
                    {
                        newEventType = new SubscribedEventViewModel(eventType, channel);
                    }
                }
                else if (eventType.ToString().Contains("user"))
                {
                    UserModel user = await MixerAPIHandler.MixerConnection.Users.GetUser(eventIDName);
                    if (user != null)
                    {
                        newEventType = new SubscribedEventViewModel(eventType, user);
                    }
                }
            });

            if (newEventType == null)
            {
                MessageBoxHelper.ShowError("Unable to find the specified name, please ensure you typed it correctly");
                return;
            }

            if (this.subscribedEvents.Select(se => se.EventType).Contains(newEventType))
            {
                MessageBoxHelper.ShowError("This event already exists");
                return;
            }

            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.ConstellationClient.LiveSubscribe(new List<ConstellationEventType>() { newEventType.GetEventType() });
            });
            
            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to subscribe to event, please try again");
                return;
            }

            this.subscribedEvents.Add(new SubscribedEventItem(newEventType));
            MixerAPIHandler.Settings.SubscribedEvents.Add(newEventType);

            await this.Window.RunAsyncOperation(async () =>
            {
                await MixerAPIHandler.SaveSettings();
            });

            this.EventTypeComboBox.SelectedIndex = -1;
            this.EventIDFinderTextBox.Clear();
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
                            fileData.AppendLine(string.Format("{0},{1},{2}", EnumHelper.GetEnumName(eventItem.EventType.Type), eventItem.EventType.Name, string.Empty));
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
                if (subscribedEvent.EventType.UniqueEventID.Equals(e.channel))
                {
                    subscribedEvent.AddFiredEvent(e);
                }
            }
        }
    }
}
