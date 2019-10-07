using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Dashboard
{
    public enum DashboardLayoutTypeEnum
    {
        One,
        Two,
        [Name("Three Right")]
        ThreeRight,
        [Name("Three Left")]
        ThreeLeft,
        Four
    }

    public enum DashboardItemTypeEnum
    {
        None,
        Chat,
        [Name("Notifications")]
        Notifications,
        [Name("Game Queue")]
        GameQueue,
        [Name("Song Requests")]
        SongRequests,
    }

    public class DashboardWindowViewModel : WindowViewModelBase
    {
        public DashboardWindowViewModel()
        {

        }

        public object ChatControl { get; set; }
        public object NotificationsControl { get; set; }
        public object GameQueueControl { get; set; }
        public object SongRequestsControl { get; set; }

        public IEnumerable<string> LayoutTypes { get { return EnumHelper.GetEnumNames<DashboardLayoutTypeEnum>(); } }

        public string LayoutTypeString
        {
            get { return EnumHelper.GetEnumName(this.LayoutType); }
            set
            {
                this.LayoutType = EnumHelper.GetEnumValueFromString<DashboardLayoutTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
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
            }
        }

        public bool IsLayoutOne { get { return this.LayoutType == DashboardLayoutTypeEnum.One; } }
        public bool IsLayoutTwo { get { return this.LayoutType == DashboardLayoutTypeEnum.Two; } }
        public bool IsLayoutThreeRight { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeRight; } }
        public bool IsLayoutThreeLeft { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeLeft; } }
        public bool IsLayoutFour { get { return this.LayoutType == DashboardLayoutTypeEnum.Four; } }

        public IEnumerable<string> ItemTypes { get { return EnumHelper.GetEnumNames<DashboardItemTypeEnum>(); } }

        public string ItemTypeOneString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeOne); }
            set
            {
                this.ItemTypeOne = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public DashboardItemTypeEnum ItemTypeOne
        {
            get { return ChannelSession.Settings.DashboardItems[0]; }
            set
            {
                this.AssignType(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        public object ItemOne { get { return this.GetItemForType(this.ItemTypeOne); } }

        public string ItemTypeTwoString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeTwo); }
            set
            {
                this.ItemTypeTwo = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
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
        public object ItemTwo { get { return this.GetItemForType(this.ItemTypeTwo); } }

        public string ItemTypeThreeString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeThree); }
            set
            {
                this.ItemTypeThree = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
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
        public object ItemThree { get { return this.GetItemForType(this.ItemTypeThree); } }

        public string ItemTypeFourString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeFour); }
            set
            {
                this.ItemTypeFour = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
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
        public object ItemFour { get { return this.GetItemForType(this.ItemTypeFour); } }

        protected override async Task OnLoadedInternal()
        {
            this.NotifyPropertiesChanged();

            await base.OnLoadedInternal();
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

            this.NotifyPropertiesChanged();
        }

        private object GetItemForType(DashboardItemTypeEnum type)
        {
            switch (type)
            {
                case DashboardItemTypeEnum.Chat: return this.ChatControl;
                case DashboardItemTypeEnum.Notifications: return this.NotificationsControl;
                case DashboardItemTypeEnum.GameQueue: return this.GameQueueControl;
                case DashboardItemTypeEnum.SongRequests: return this.SongRequestsControl;
            }
            return null;
        }

        private void NotifyPropertiesChanged()
        {
            this.NotifyPropertyChanged("ItemOne");
            this.NotifyPropertyChanged("ItemTypeOne");
            this.NotifyPropertyChanged("ItemTypeOneString");
            this.NotifyPropertyChanged("ItemTwo");
            this.NotifyPropertyChanged("ItemTypeTwo");
            this.NotifyPropertyChanged("ItemTypeTwoString");
            this.NotifyPropertyChanged("ItemThree");
            this.NotifyPropertyChanged("ItemTypeThree");
            this.NotifyPropertyChanged("ItemTypeThreeString");
            this.NotifyPropertyChanged("ItemFour");
            this.NotifyPropertyChanged("ItemTypeFour");
            this.NotifyPropertyChanged("ItemTypeFourString");
        }
    }
}
