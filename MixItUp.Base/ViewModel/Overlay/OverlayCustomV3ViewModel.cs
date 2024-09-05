using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Trovo.Base.Models.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayCustomV3TestTypeEnum
    {
        UserTimedOut,
        UserBanned,

        ChatMessage,
        ChatMessageDeleted,
        ChatCleared,

        Follow,
        Raid,

        Subscribe,
        Resubscribe,
        SubscriptionGifted,
        MassSubscriptionGifted,

        Donation,

        TwitchBits,

        YouTubeSuperChat,

        TrovoElixirSpell,
    }

    public class OverlayCustomPropertyV3ViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value;

        public ICommand DeleteCommand { get; set; }

        private OverlayCustomV3ViewModel viewModel;

        public OverlayCustomPropertyV3ViewModel(OverlayCustomV3ViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.DeleteProperty(this);
            });
        }

        public OverlayCustomPropertyV3ViewModel(OverlayCustomV3ViewModel viewModel, string name, string value)
            : this(viewModel)
        {
            this.Name = name;
            this.Value = value;
        }

        public OverlayCustomPropertyV3ViewModel(OverlayCustomV3ViewModel viewModel, KeyValuePair<string, string> kvp)
            : this(viewModel)
        {
            this.Name = kvp.Key;
            this.Value = kvp.Value;
        }
    }

    public class OverlayCustomV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayCustomV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayCustomV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayCustomV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public ObservableCollection<OverlayCustomPropertyV3ViewModel> Properties { get; set; } = new ObservableCollection<OverlayCustomPropertyV3ViewModel>();

        public ICommand AddPropertyCommand { get; set; }

        public OverlayCustomV3ViewModel()
            : base(OverlayItemV3Type.Custom)
        {
            this.Properties.Add(new OverlayCustomPropertyV3ViewModel(this, "TestProperty", "Hello World"));

            this.InitializeInternal();
        }

        public OverlayCustomV3ViewModel(OverlayCustomV3Model item)
            : base(item)
        {
            foreach (var property in item.Properties)
            {
                this.Properties.Add(new OverlayCustomPropertyV3ViewModel(this, property));
            }

            foreach (var property in this.Properties)
            {
                property.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            }

            this.InitializeInternal();
        }

        public override Result Validate()
        {
            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            object result = await DialogHelper.ShowEnumDropDown(EnumHelper.GetEnumList<OverlayCustomV3TestTypeEnum>());
            if (result != null)
            {
                OverlayCustomV3Model custom = (OverlayCustomV3Model)widget.Item;

                OverlayCustomV3TestTypeEnum type = (OverlayCustomV3TestTypeEnum)result;
                if (type == OverlayCustomV3TestTypeEnum.UserTimedOut)
                {
                    custom.OnChatUserTimedOut(this, ChannelSession.User);
                }
                else if (type == OverlayCustomV3TestTypeEnum.UserBanned)
                {
                    custom.OnChatUserBanned(this, ChannelSession.User);
                }
                else if (type == OverlayCustomV3TestTypeEnum.ChatMessage || type == OverlayCustomV3TestTypeEnum.ChatMessageDeleted)
                {
                    string messageID = Guid.NewGuid().ToString();
                    ChatMessageViewModel message = new ChatMessageViewModel(messageID, ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.User);
                    message.AddStringMessagePart("Hello World! This is a test message so you can see how chat looks");
                    custom.OnChatMessageReceived(this, message);

                    if (type == OverlayCustomV3TestTypeEnum.ChatMessageDeleted)
                    {
                        await Task.Delay(3000);
                        custom.OnChatMessageDeleted(this, messageID);
                    }
                }
                else if (type == OverlayCustomV3TestTypeEnum.ChatCleared)
                {
                    custom.OnChatCleared(this, new EventArgs());
                }
                else if (type == OverlayCustomV3TestTypeEnum.Follow)
                {
                    custom.OnFollow(this, ChannelSession.User);
                }
                else if (type == OverlayCustomV3TestTypeEnum.Raid)
                {
                    custom.OnRaid(this, new Tuple<UserV2ViewModel, int>(ChannelSession.User, 10));
                }
                else if (type == OverlayCustomV3TestTypeEnum.Subscribe)
                {
                    custom.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayCustomV3TestTypeEnum.Resubscribe)
                {
                    custom.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayCustomV3TestTypeEnum.SubscriptionGifted)
                {
                    custom.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayCustomV3TestTypeEnum.MassSubscriptionGifted)
                {
                    List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        subscriptions.Add(new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                    }
                    custom.OnMassSubscription(this, subscriptions);
                }
                else if (type == OverlayCustomV3TestTypeEnum.Donation)
                {
                    custom.OnDonation(this, new UserDonationModel()
                    {
                        Source = UserDonationSourceEnum.Streamlabs,

                        User = ChannelSession.User,
                        Username = ChannelSession.User.Username,

                        Message = "Text",

                        Amount = 12.34,

                        DateTime = DateTimeOffset.Now,
                    });
                }
                else if (type == OverlayCustomV3TestTypeEnum.TwitchBits)
                {
                    custom.OnTwitchBits(this, new TwitchUserBitsCheeredModel(ChannelSession.User, new PubSubBitsEventV2Model()
                    {
                        bits_used = 100,
                        chat_message = "Hello World"
                    }));
                }
                else if (type == OverlayCustomV3TestTypeEnum.YouTubeSuperChat)
                {
                    custom.OnYouTubeSuperChat(this, new YouTubeSuperChatViewModel(new LiveChatSuperChatDetails()
                    {
                        AmountDisplayString = "$12.34",
                        UserComment = "Hello World"
                    }, ChannelSession.User));
                }
                else if (type == OverlayCustomV3TestTypeEnum.TrovoElixirSpell)
                {
                    custom.OnTrovoSpellCast(this, new TrovoChatSpellViewModel(ChannelSession.User, new ChatMessageModel() { content = "" })
                    {
                        Contents = new TrovoChatSpellContentModel()
                        {
                            gift = "Foobar",
                            value_type = TrovoChatSpellViewModel.ElixirValueType,
                            num = 10,
                            gift_value = 10,
                        }
                    });
                }
            }

            await base.TestWidget(widget);
        }

        public void DeleteProperty(OverlayCustomPropertyV3ViewModel property)
        {
            this.Properties.Remove(property);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayCustomV3Model result = new OverlayCustomV3Model()
            {
            };

            foreach (var property in this.Properties)
            {
                if (!string.IsNullOrWhiteSpace(property.Name))
                {
                    result.Properties[property.Name] = property.Value;
                }
            }

            this.AssignProperties(result);

            return result;
        }

        private void InitializeInternal()
        {
            this.AddPropertyCommand = this.CreateCommand(() =>
            {
                OverlayCustomPropertyV3ViewModel property = new OverlayCustomPropertyV3ViewModel(this);
                this.Properties.Add(property);
                property.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            });
        }
    }
}
