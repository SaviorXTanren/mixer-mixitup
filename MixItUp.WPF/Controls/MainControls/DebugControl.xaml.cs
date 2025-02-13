using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Twitch.Clients.EventSub;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for DebugControl.xaml
    /// </summary>
    public partial class DebugControl : MainControlBase
    {
        public DebugControl()
        {
            InitializeComponent();
        }

        private async void TriggerGenericDonation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID);
            if (user == null)
            {
                user = ChannelSession.User;
            }

            UserDonationModel donation = new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = Guid.NewGuid().ToString(),
                Username = user.Username,
                Message = "This is a donation message!",

                Amount = 12.34,

                DateTime = DateTimeOffset.Now,

                User = user
            };

            await EventService.ProcessDonationEvent(EventTypeEnum.StreamlabsDonation, donation);
        }

        private async void TriggerTwitchTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
            {
                Metadata = new MessageMetadata()
                {
                    SubscriptionType = "channel.chat.notification"
                },
                Payload = new NotificationMessagePayload()
                {
                    Event = JObject.FromObject(new ChatNotification()
                    {
                        notice_type = "sub",

                        broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        chatter_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        chatter_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        chatter_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        message_id = Guid.NewGuid().ToString(),
                        message = new ChatMessageNotificationMessage()
                        {
                            text = "This is a message",
                            fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                        },

                        sub = new ChatNotificationSub()
                        {
                            sub_tier = "1000"
                        }
                    })
                }
            });
        }

        private async void Twitch1GiftedTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
            {
                Metadata = new MessageMetadata()
                {
                    SubscriptionType = "channel.chat.notification"
                },
                Payload = new NotificationMessagePayload()
                {
                    Event = JObject.FromObject(new ChatNotification()
                    {
                        notice_type = "sub_gift",

                        broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        chatter_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        chatter_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        chatter_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        message_id = Guid.NewGuid().ToString(),
                        message = new ChatMessageNotificationMessage()
                        {
                            text = "This is a message",
                            fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                        },

                        sub_gift = new ChatNotificationSubGift()
                        {
                            sub_tier = "1000",
                            duration_months = 1,

                            recipient_user_id = twitchUser.ID,
                            recipient_user_login = twitchUser.Username,
                            recipient_user_name = twitchUser.DisplayName
                        }
                    })
                }
            });
        }

        private async void Twitch5GiftedTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            string communityGiftID = Guid.NewGuid().ToString();
            for (int i = 0; i < 5; i++)
            {
                await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
                {
                    Metadata = new MessageMetadata()
                    {
                        SubscriptionType = "channel.chat.notification"
                    },
                    Payload = new NotificationMessagePayload()
                    {
                        Event = JObject.FromObject(new ChatNotification()
                        {
                            notice_type = "sub_gift",

                            broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                            broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                            broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                            chatter_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                            chatter_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                            chatter_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                            message_id = Guid.NewGuid().ToString(),
                            message = new ChatMessageNotificationMessage()
                            {
                                text = "This is a message",
                                fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                            },

                            sub_gift = new ChatNotificationSubGift()
                            {
                                sub_tier = "1000",
                                duration_months = 1,
                                community_gift_id = communityGiftID,

                                recipient_user_id = twitchUser.ID,
                                recipient_user_login = twitchUser.Username,
                                recipient_user_name = twitchUser.DisplayName
                            }
                        })
                    }
                });
            }

            await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
            {
                Metadata = new MessageMetadata()
                {
                    SubscriptionType = "channel.chat.notification"
                },
                Payload = new NotificationMessagePayload()
                {
                    Event = JObject.FromObject(new ChatNotification()
                    {
                        notice_type = "community_sub_gift",

                        broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        chatter_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        chatter_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        chatter_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        message_id = Guid.NewGuid().ToString(),
                        message = new ChatMessageNotificationMessage()
                        {
                            text = "This is a message",
                            fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                        },

                        community_sub_gift = new ChatNotificationCommunitySubGift()
                        {
                            id = communityGiftID,
                            total = 5,
                            cumulative_total = 200,
                            sub_tier = "1000",
                        }
                    })
                }
            });
        }


        private async void TwitchAnon5GiftedTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            string communityGiftID = Guid.NewGuid().ToString();
            for (int i = 0; i < 5; i++)
            {
                await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
                {
                    Metadata = new MessageMetadata()
                    {
                        SubscriptionType = "channel.chat.notification"
                    },
                    Payload = new NotificationMessagePayload()
                    {
                        Event = JObject.FromObject(new ChatNotification()
                        {
                            notice_type = "sub_gift",

                            broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                            broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                            broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                            chatter_is_anonymous = true,

                            message_id = Guid.NewGuid().ToString(),
                            message = new ChatMessageNotificationMessage()
                            {
                                text = "This is a message",
                                fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                            },

                            sub_gift = new ChatNotificationSubGift()
                            {
                                sub_tier = "1000",
                                duration_months = 1,
                                community_gift_id = communityGiftID,

                                recipient_user_id = twitchUser.ID,
                                recipient_user_login = twitchUser.Username,
                                recipient_user_name = twitchUser.DisplayName
                            }
                        })
                    }
                });
            }

            await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
            {
                Metadata = new MessageMetadata()
                {
                    SubscriptionType = "channel.chat.notification"
                },
                Payload = new NotificationMessagePayload()
                {
                    Event = JObject.FromObject(new ChatNotification()
                    {
                        notice_type = "community_sub_gift",

                        broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        chatter_is_anonymous = true,

                        message_id = Guid.NewGuid().ToString(),
                        message = new ChatMessageNotificationMessage()
                        {
                            text = "This is a message",
                            fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                        },

                        community_sub_gift = new ChatNotificationCommunitySubGift()
                        {
                            id = communityGiftID,
                            total = 5,
                            cumulative_total = 200,
                            sub_tier = "1000",
                        }
                    })
                }
            });
        }

        private async void Twitch100BitsCheer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            await ServiceManager.Get<TwitchSession>().Client.ProcessMockNotification(new NotificationMessage()
            {
                Metadata = new MessageMetadata()
                {
                    SubscriptionType = "channel.chat.message"
                },
                Payload = new NotificationMessagePayload()
                {
                    Event = JObject.FromObject(new ChatMessageNotification()
                    {
                        broadcaster_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        broadcaster_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        broadcaster_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        chatter_user_id = ServiceManager.Get<TwitchSession>().StreamerModel.id,
                        chatter_user_login = ServiceManager.Get<TwitchSession>().StreamerModel.login,
                        chatter_user_name = ServiceManager.Get<TwitchSession>().StreamerModel.display_name,

                        message_id = Guid.NewGuid().ToString(),
                        message = new ChatMessageNotificationMessage()
                        {
                            text = "This is a message",
                            fragments = new List<ChatMessageNotificationFragment>()
                            {
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "This"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "is"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "a"
                                },
                                new ChatMessageNotificationFragment()
                                {
                                    type = "text",
                                    text = "messge"
                                },
                            }
                        },

                        cheer = new ChatMessageNotificationCheer()
                        {
                            bits = 1234
                        },
                    })
                }
            });
        }
    }
}
