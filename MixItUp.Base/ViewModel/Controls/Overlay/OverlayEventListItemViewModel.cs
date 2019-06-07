using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayEventListItemViewModel : OverlayListItemViewModelBase
    {
        public bool ShowFollows
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Followers); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Followers, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowHosts
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Hosts); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Hosts, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowSubscribers
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Subscribers); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Subscribers, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowDonations
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Donations); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Donations, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowSparks
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Sparks); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Sparks, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowEmbers
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Embers); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Embers, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ShowMilestones
        {
            get { return this.eventListTypes.Contains(EventListItemTypeEnum.Milestones); }
            set
            {
                this.UpdateEventListItem(EventListItemTypeEnum.Milestones, value);
                this.NotifyPropertyChanged();
            }
        }

        public bool ResetOnLoad
        {
            get { return this.resetOnLoad; }
            set
            {
                this.resetOnLoad = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool resetOnLoad;

        private HashSet<EventListItemTypeEnum> eventListTypes = new HashSet<EventListItemTypeEnum>();

        public OverlayEventListItemViewModel()
            : base()
        {
            this.HTML = OverlayEventList.HTMLTemplate;
        }

        public OverlayEventListItemViewModel(OverlayEventList item)
            : base(item.TotalToShow, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.AddEventAnimation, item.RemoveEventAnimation, item.HTMLText)
        {
            this.eventListTypes = new HashSet<EventListItemTypeEnum>(item.ItemTypes);
            this.ResetOnLoad = item.ResetOnLoad;
        }

        public override OverlayItemBase GetItem()
        {
            if (this.Validate() && this.eventListTypes.Count > 0)
            {
                return new OverlayEventList(this.HTML, this.eventListTypes, totalToShow, this.ResetOnLoad, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor, this.TextColor, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }

        private void UpdateEventListItem(EventListItemTypeEnum type, bool value)
        {
            if (value)
            {
                this.eventListTypes.Add(type);
            }
            else
            {
                this.eventListTypes.Remove(type);
            }
        }
    }
}
