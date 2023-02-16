using MixItUp.Base.Model.Overlay;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayChatV3ViewModel : OverlayListV3ViewModelBase
    {
        public int DelayMessageTiming
        {
            get { return this.delayMessageTiming; }
            set
            {
                this.delayMessageTiming = value;
                this.NotifyPropertyChanged();
            }
        }
        private int delayMessageTiming;

        public int HideMessageTiming
        {
            get { return this.hideMessageTiming; }
            set
            {
                this.hideMessageTiming = value;
                this.NotifyPropertyChanged();
            }
        }
        private int hideMessageTiming;

        public bool ShowMaxMessagesOnly
        {
            get { return this.showMaxMessagesOnly; }
            set
            {
                this.showMaxMessagesOnly = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showMaxMessagesOnly;

        public bool ShowLatestMessagesAtTop
        {
            get { return this.showLatestMessagesAtTop; }
            set
            {
                this.showLatestMessagesAtTop = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showLatestMessagesAtTop;

        private bool DontAnimateEmotes
        {
            get { return this.dontAnimateEmotes; }
            set
            {
                this.dontAnimateEmotes = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool dontAnimateEmotes;

        public bool HideBadgesAndRoles
        {
            get { return this.hideBadgesAndRoles; }
            set
            {
                this.hideBadgesAndRoles = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool hideBadgesAndRoles;

        public bool HideAvatar
        {
            get { return this.hideAvatar; }
            set
            {
                this.hideAvatar = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool hideAvatar;

        public bool HideCommandMessages
        {
            get { return this.hideCommandMessages; }
            set
            {
                this.hideCommandMessages = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool hideCommandMessages;

        public bool HideSpecialtyExcludedUsers
        {
            get { return this.hideSpecialtyExcludedUsers; }
            set
            {
                this.hideSpecialtyExcludedUsers = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool hideSpecialtyExcludedUsers;

        public string ExcludedUsernames
        {
            get { return this.excludedUsernames; }
            set
            {
                this.excludedUsernames = value;
                this.NotifyPropertyChanged();
            }
        }
        private string excludedUsernames;

        private OverlayChatV3Model item;

        public OverlayChatV3ViewModel()
            : base(OverlayItemV3Type.Chat)
        {
            this.AddAnimations(new List<string>() { OverlayEventListV3Model.AddedAnimationName, OverlayEventListV3Model.RemovedAnimationName });
        }

        public OverlayChatV3ViewModel(OverlayChatV3Model item)
            : base(item)
        {
            this.item = item;

            this.DelayMessageTiming = item.DelayMessageTiming;
            this.HideMessageTiming = item.HideMessageTiming;
            this.ShowMaxMessagesOnly = item.ShowMaxMessagesOnly;
            this.ShowLatestMessagesAtTop = item.ShowLatestMessagesAtTop;
            this.DontAnimateEmotes = item.DontAnimateEmotes;
            this.HideBadgesAndRoles = item.HideBadgesAndRoles;
            this.HideAvatar = item.HideAvatar;
            this.HideCommandMessages = item.HideCommandMessages;
            this.HideSpecialtyExcludedUsers = item.HideSpecialtyExcludedUsers;
            this.ExcludedUsernames = string.Join(",", item.ExcludedUsernames);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            HashSet<string> excludedUsernames = new HashSet<string>();
            string[] splits = this.ExcludedUsernames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (splits != null && splits.Length > 0)
            {
                foreach (string split in splits)
                {
                    excludedUsernames.Add(split);
                }    
            }

            OverlayChatV3Model item = new OverlayChatV3Model(this.DelayMessageTiming, this.HideMessageTiming, this.ShowMaxMessagesOnly, this.ShowLatestMessagesAtTop, this.DontAnimateEmotes, this.HideBadgesAndRoles,
                this.HideAvatar, this.HideCommandMessages, this.HideSpecialtyExcludedUsers, excludedUsernames)
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                Text = this.Text,
                FontSize = this.FontSize,
                FontName = this.FontName,
                FontColor = this.FontColor,
                Bold = this.Bold,
                Italics = this.Italics,
                Underline = this.Underline,
                ShadowColor = this.ShadowColor,

                BackgroundColor = this.BackgroundColor,
                BorderColor = this.BorderColor,

                ItemHeight = this.ItemHeight,
                ItemWidth = this.ItemWidth,
                MaxToShow = this.MaxToShow
            };

            return item;
        }
    }
}
