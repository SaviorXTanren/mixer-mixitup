using MixItUp.Base.Model;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Services.Trovo.New;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayChatApplicableStreamingPlatformV3ViewModel : UIViewModelBase
    {
        public StreamingPlatformTypeEnum StreamingPlatform { get; set; }

        public string Name { get { return Resources.ResourceManager.GetString(this.StreamingPlatform.ToString()); } }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isEnabled;

        public OverlayChatApplicableStreamingPlatformV3ViewModel(StreamingPlatformTypeEnum streamingPlatform, bool isEnabled = true)
        {
            this.StreamingPlatform = streamingPlatform;
            this.IsEnabled = isEnabled;
        }
    }

    public class OverlayChatV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayChatV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayChatV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayChatV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public int MessageDelayTime
        {
            get { return this.messageDelayTime; }
            set
            {
                this.messageDelayTime = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int messageDelayTime;

        public int MessageRemovalTime
        {
            get { return this.messageRemovalTime; }
            set
            {
                this.messageRemovalTime = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int messageRemovalTime;

        public bool AddMessagesToTop
        {
            get { return this.addMessagesToTop; }
            set
            {
                this.addMessagesToTop = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool addMessagesToTop;

        public bool HideExclamationMessages
        {
            get { return this.hideExclamationMessages; }
            set
            {
                this.hideExclamationMessages = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool hideExclamationMessages;

        public bool DisplayAlejoPronouns
        {
            get { return this.displayAlejoPronouns; }
            set
            {
                this.displayAlejoPronouns = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool displayAlejoPronouns;

        public bool IgnoreSpecialtyExcludedUsers
        {
            get { return this.ignoreSpecialtyExcludedUsers; }
            set
            {
                this.ignoreSpecialtyExcludedUsers = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool ignoreSpecialtyExcludedUsers;

        public string UsernamesToIgnore
        {
            get { return this.usernamesToIgnore; }
            set
            {
                this.usernamesToIgnore = value;
                this.NotifyPropertyChanged();
            }
        }
        private string usernamesToIgnore;

        public bool ShowPlatformBadge
        {
            get { return this.showPlatformBadge; }
            set
            {
                this.showPlatformBadge = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showPlatformBadge;

        public bool ShowRoleBadge
        {
            get { return this.showRoleBadge; }
            set
            {
                this.showRoleBadge = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showRoleBadge;

        public bool ShowSubscriberBadge
        {
            get { return this.showSubscriberBadge; }
            set
            {
                this.showSubscriberBadge = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showSubscriberBadge;

        public bool ShowSpecialtyBadge
        {
            get { return this.showSpecialtyBadge; }
            set
            {
                this.showSpecialtyBadge = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showSpecialtyBadge;

        public ObservableCollection<OverlayChatApplicableStreamingPlatformV3ViewModel> ApplicableStreamingPlatforms { get; set; } = new ObservableCollection<OverlayChatApplicableStreamingPlatformV3ViewModel>();

        public OverlayAnimationV3ViewModel MessageAddedAnimation;
        public OverlayAnimationV3ViewModel MessageRemovedAnimation;

        public OverlayChatV3ViewModel()
            : base(OverlayItemV3Type.Chat)
        {
            this.width = 400;
            this.height = 600;

            this.BackgroundColor = "Wheat";
            this.BorderColor = "Black";

            this.ShowPlatformBadge = true;
            this.ShowRoleBadge = true;
            this.ShowSubscriberBadge = true;
            this.ShowSpecialtyBadge = true;

            foreach (StreamingPlatformTypeEnum streamingPlatform in StreamingPlatforms.SupportedPlatforms)
            {
                this.ApplicableStreamingPlatforms.Add(new OverlayChatApplicableStreamingPlatformV3ViewModel(streamingPlatform));
            }

            this.MessageAddedAnimation = new OverlayAnimationV3ViewModel(Resources.MessageAdded, new OverlayAnimationV3Model());
            this.MessageRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.MessageRemoved, new OverlayAnimationV3Model());

            this.Animations.Add(this.MessageAddedAnimation);
            this.Animations.Add(this.MessageRemovedAnimation);

            this.InitializeInternal();
        }

        public OverlayChatV3ViewModel(OverlayChatV3Model item)
            : base(item)
        {
            this.height = item.Height;

            this.BackgroundColor = item.BackgroundColor;
            this.BorderColor = item.BorderColor;

            this.MessageDelayTime = item.MessageDelayTime;
            this.MessageRemovalTime = item.MessageRemovalTime;
            this.AddMessagesToTop = item.AddMessagesToTop;

            this.HideExclamationMessages = item.HideExclamationMessages;
            this.DisplayAlejoPronouns = item.DisplayAlejoPronouns;

            this.IgnoreSpecialtyExcludedUsers = item.IgnoreSpecialtyExcludedUsers;
            this.UsernamesToIgnore = string.Join(Environment.NewLine, item.UsernamesToIgnore);

            this.ShowPlatformBadge = item.ShowPlatformBadge;
            this.ShowRoleBadge = item.ShowRoleBadge;
            this.ShowSubscriberBadge = item.ShowSubscriberBadge;
            this.ShowSpecialtyBadge = item.ShowSpecialtyBadge;

            foreach (StreamingPlatformTypeEnum streamingPlatform in StreamingPlatforms.SupportedPlatforms)
            {
                this.ApplicableStreamingPlatforms.Add(new OverlayChatApplicableStreamingPlatformV3ViewModel(streamingPlatform, item.ApplicableStreamingPlatforms.Contains(streamingPlatform)));
            }

            this.MessageAddedAnimation = new OverlayAnimationV3ViewModel(Resources.MessageAdded, item.MessageAddedAnimation);
            this.MessageRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.MessageRemoved, item.MessageRemovedAnimation);

            this.Animations.Add(this.MessageAddedAnimation);
            this.Animations.Add(this.MessageRemovedAnimation);

            this.InitializeInternal();
        }

        public override Result Validate()
        {
            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayChatV3Model chat = (OverlayChatV3Model)widget.Item;

            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.GetConnectedPlatforms())
            {
                if (platform == StreamingPlatformTypeEnum.Twitch)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: ServiceManager.Get<TwitchSession>().StreamerID);
                    if (user == null)
                    {
                        user = ChannelSession.User;
                    }

                    TwitchChatMessageViewModel message = new TwitchChatMessageViewModel(user, "Hello World! This is a test message from Twitch so you can see how chat looks Kappa");
                    await chat.AddMessage(message);
                }
                else if (platform == StreamingPlatformTypeEnum.YouTube)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: ServiceManager.Get<YouTubeSession>().StreamerID);
                    if (user == null)
                    {
                        user = ChannelSession.User;
                    }

                    YouTubeChatMessageViewModel message = new YouTubeChatMessageViewModel(user, "Hello World! This is a test message from YouTube so you can see how chat looks :grinning_face:");
                    await chat.AddMessage(message);
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: ServiceManager.Get<TrovoSession>().StreamerID);
                    if (user == null)
                    {
                        user = ChannelSession.User;
                    }

                    TrovoChatMessageViewModel message = new TrovoChatMessageViewModel(user, "Hello World! This is a test message from Trovo so you can see how chat looks :smile");
                    await chat.AddMessage(message);
                }
                else
                {
                    ChatMessageViewModel message = new ChatMessageViewModel(Guid.NewGuid().ToString(), platform, ChannelSession.User);
                    message.AddStringMessagePart("Hello World! This is a test message so you can see how chat looks");
                    await chat.AddMessage(message);
                }
            }

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayChatV3Model result = new OverlayChatV3Model()
            {
                Height = this.height,

                BackgroundColor = this.BackgroundColor,
                BorderColor = this.BorderColor,

                MessageDelayTime = this.MessageDelayTime,
                MessageRemovalTime = this.MessageRemovalTime,
                AddMessagesToTop = this.AddMessagesToTop,

                HideExclamationMessages = this.HideExclamationMessages,
                DisplayAlejoPronouns = this.DisplayAlejoPronouns,

                IgnoreSpecialtyExcludedUsers = this.IgnoreSpecialtyExcludedUsers,

                ShowPlatformBadge = this.ShowPlatformBadge,
                ShowRoleBadge = this.ShowRoleBadge,
                ShowSubscriberBadge = this.ShowSubscriberBadge,
                ShowSpecialtyBadge = this.ShowSpecialtyBadge,
            };

            if (!string.IsNullOrWhiteSpace(this.UsernamesToIgnore))
            {
                string[] splits = this.UsernamesToIgnore.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (splits != null && splits.Length > 0)
                {
                    result.UsernamesToIgnore = new List<string>(splits);
                }
            }

            foreach (OverlayChatApplicableStreamingPlatformV3ViewModel streamingPlatform in this.ApplicableStreamingPlatforms)
            {
                if (streamingPlatform.IsEnabled)
                {
                    result.ApplicableStreamingPlatforms.Add(streamingPlatform.StreamingPlatform);
                }
                else
                {
                    result.ApplicableStreamingPlatforms.Remove(streamingPlatform.StreamingPlatform);
                }
            }

            result.MessageAddedAnimation = this.MessageAddedAnimation.GetAnimation();
            result.MessageRemovedAnimation = this.MessageRemovedAnimation.GetAnimation();

            this.AssignProperties(result);

            return result;
        }

        private void InitializeInternal()
        {
            foreach (OverlayChatApplicableStreamingPlatformV3ViewModel streamingPlatform in this.ApplicableStreamingPlatforms)
            {
                streamingPlatform.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            }
        }
    }
}
