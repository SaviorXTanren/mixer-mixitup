using MixItUp.Base.ViewModels;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Dashboard
{
    public enum DashboardLayoutTypeEnum
    {
        One,
        Two,
        ThreeRight,
        ThreeLeft,
        Four
    }

    public enum DashboardItemTypeEnum
    {
        None,
        Chat,
        Alerts,
        [Obsolete]
        Statistics,
        GameQueue,
        [Obsolete]
        SongRequests,
        QuickCommands,
        RedemptionStore
    }

    public class LayoutItemsViewModel : UIViewModelBase
    {
        public object ItemOne
        {
            get { return this.itemOne; }
            set
            {
                this.itemOne = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemOne;

        public object ItemTwo
        {
            get { return this.itemTwo; }
            set
            {
                this.itemTwo = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemTwo;

        public object ItemThree
        {
            get { return this.itemThree; }
            set
            {
                this.itemThree = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemThree;

        public object ItemFour
        {
            get { return this.itemFour; }
            set
            {
                this.itemFour = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemFour;

        public void ClearItems()
        {
            this.ItemOne = null;
            this.ItemTwo = null;
            this.ItemThree = null;
            this.ItemFour = null;
        }

        public void NotifyItemPropertiesChanged()
        {
            this.NotifyPropertyChanged("ItemOne");
            this.NotifyPropertyChanged("ItemTwo");
            this.NotifyPropertyChanged("ItemThree");
            this.NotifyPropertyChanged("ItemFour");
        }
    }

    public class DashboardWindowViewModel : UIViewModelBase
    {
        public DashboardWindowViewModel() { }

        public object ChatControl { get; set; }
        public object AlertsControl { get; set; }
        public object StatisticsControl { get; set; }
        public object GameQueueControl { get; set; }
        public object QuickCommandsControl { get; set; }
        public object RedemptionStoreControl { get; set; }

        public IEnumerable<DashboardLayoutTypeEnum> LayoutTypes { get { return EnumHelper.GetEnumList<DashboardLayoutTypeEnum>(); } }

        public DashboardLayoutTypeEnum LayoutType
        {
            get { return ChannelSession.Settings.DashboardLayout; }
            set
            {
                ChannelSession.Settings.DashboardLayout = value;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsLayoutOne");
                this.NotifyPropertyChanged("IsLayoutTwo");
                this.NotifyPropertyChanged("IsLayoutThreeRight");
                this.NotifyPropertyChanged("IsLayoutThreeLeft");
                this.NotifyPropertyChanged("IsLayoutFour");

                this.NotifyItemPropertiesChanged();
            }
        }

        private bool m_IsPinned = false;
        public bool IsPinned 
        {
            get { return m_IsPinned; } 
            set
            {
                m_IsPinned = value;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsNotPinned");
            }
        }
        public bool IsNotPinned { get { return !IsPinned; } }

        public bool IsLayoutOne { get { return this.LayoutType == DashboardLayoutTypeEnum.One; } }
        public bool IsLayoutTwo { get { return this.LayoutType == DashboardLayoutTypeEnum.Two; } }
        public bool IsLayoutThreeRight { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeRight; } }
        public bool IsLayoutThreeLeft { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeLeft; } }
        public bool IsLayoutFour { get { return this.LayoutType == DashboardLayoutTypeEnum.Four; } }

        public DashboardItemTypeEnum ItemTypeOne
        {
            get { return ChannelSession.Settings.DashboardItems[0]; }
            set
            {
                this.AssignType(value, 0);
                this.NotifyPropertyChanged();
            }
        }

        public DashboardItemTypeEnum ItemTypeTwo
        {
            get { return ChannelSession.Settings.DashboardItems[1]; }
            set
            {
                this.AssignType(value, 1);
                this.NotifyPropertyChanged();
            }
        }

        public DashboardItemTypeEnum ItemTypeThree
        {
            get { return ChannelSession.Settings.DashboardItems[2]; }
            set
            {
                this.AssignType(value, 2);
                this.NotifyPropertyChanged();
            }
        }

        public DashboardItemTypeEnum ItemTypeFour
        {
            get { return ChannelSession.Settings.DashboardItems[3]; }
            set
            {
                this.AssignType(value, 3);
                this.NotifyPropertyChanged();
            }
        }

        public LayoutItemsViewModel LayoutOne { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutTwo { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutThreeRight { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutThreeLeft { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutFour { get; set; } = new LayoutItemsViewModel();

        public IEnumerable<DashboardItemTypeEnum> ItemTypes { get { return EnumHelper.GetEnumList<DashboardItemTypeEnum>(); } }

        protected override async Task OnOpenInternal()
        {
            this.NotifyItemPropertiesChanged();

            await base.OnOpenInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.NotifyItemPropertiesChanged();

            await base.OnVisibleInternal();
        }

        private void NotifyItemPropertiesChanged()
        {
            this.NotifyPropertyChanged("ItemTypeOne");
            this.NotifyPropertyChanged("ItemTypeTwo");
            this.NotifyPropertyChanged("ItemTypeThree");
            this.NotifyPropertyChanged("ItemTypeFour");

            this.LayoutOne.ClearItems();
            this.LayoutTwo.ClearItems();
            this.LayoutThreeLeft.ClearItems();
            this.LayoutThreeRight.ClearItems();
            this.LayoutFour.ClearItems();

            if (this.LayoutType == DashboardLayoutTypeEnum.One)
            {
                this.AssignItemsToLayout(this.LayoutOne);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.Two)
            {
                this.AssignItemsToLayout(this.LayoutTwo);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.ThreeLeft)
            {
                this.AssignItemsToLayout(this.LayoutThreeLeft);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.ThreeRight)
            {
                this.AssignItemsToLayout(this.LayoutThreeRight);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.Four)
            {
                this.AssignItemsToLayout(this.LayoutFour);
            }

            this.LayoutOne.NotifyItemPropertiesChanged();
            this.LayoutTwo.NotifyItemPropertiesChanged();
            this.LayoutThreeLeft.NotifyItemPropertiesChanged();
            this.LayoutThreeRight.NotifyItemPropertiesChanged();
            this.LayoutFour.NotifyItemPropertiesChanged();
        }

        private void AssignType(DashboardItemTypeEnum type, int index)
        {
            for (int i = 0; i < ChannelSession.Settings.DashboardItems.Count; i++)
            {
                if (ChannelSession.Settings.DashboardItems[i] == type)
                {
                    ChannelSession.Settings.DashboardItems[i] = DashboardItemTypeEnum.None;
                }
            }
            ChannelSession.Settings.DashboardItems[index] = type;

            this.NotifyItemPropertiesChanged();
        }

        private void AssignItemsToLayout(LayoutItemsViewModel layout)
        {
            layout.ItemOne = this.GetItemForType(this.ItemTypeOne);
            layout.ItemTwo = this.GetItemForType(this.ItemTypeTwo);
            layout.ItemThree = this.GetItemForType(this.ItemTypeThree);
            layout.ItemFour = this.GetItemForType(this.ItemTypeFour);
        }

        private object GetItemForType(DashboardItemTypeEnum type)
        {
            switch (type)
            {
                case DashboardItemTypeEnum.Chat: return this.ChatControl;
                case DashboardItemTypeEnum.Alerts: return this.AlertsControl;
                
                case DashboardItemTypeEnum.GameQueue: return this.GameQueueControl;
                case DashboardItemTypeEnum.QuickCommands: return this.QuickCommandsControl;
                case DashboardItemTypeEnum.RedemptionStore: return this.RedemptionStoreControl;
            }
            return null;
        }
    }
}
