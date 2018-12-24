using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Clips;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Util
{
    public static class GlobalEvents
    {
        public static event EventHandler<string> OnShowMessageBox;
        public static void ShowMessageBox(string message)
        {
            if (GlobalEvents.OnShowMessageBox != null)
            {
                GlobalEvents.OnShowMessageBox(null, message);
            }
        }

        public static event EventHandler<string> OnServiceDisconnect;
        public static void ServiceDisconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceDisconnect != null)
            {
                GlobalEvents.OnServiceDisconnect(null, serviceName);
            }
        }

        public static event EventHandler<string> OnServiceReconnect;
        public static void ServiceReconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceReconnect != null)
            {
                GlobalEvents.OnServiceReconnect(null, serviceName);
            }
        }

        public static event EventHandler OnChatFontSizeChanged;
        public static void ChatFontSizeChanged()
        {
            if (GlobalEvents.OnChatFontSizeChanged != null)
            {
                GlobalEvents.OnChatFontSizeChanged(null, new EventArgs());
            }
        }

        public static event EventHandler<ChatMessageViewModel> OnChatMessageReceived;
        public static void ChatMessageReceived(ChatMessageViewModel chatMessage)
        {
            if (GlobalEvents.OnChatMessageReceived != null)
            {
                GlobalEvents.OnChatMessageReceived(null, chatMessage);
            }
        }

        public static event EventHandler<Guid> OnChatMessageDeleted;
        public static void ChatMessageDeleted(Guid messageID)
        {
            if (GlobalEvents.OnChatMessageDeleted != null)
            {
                GlobalEvents.OnChatMessageDeleted(null, messageID);
            }
        }

        public static event EventHandler<ChatMessageViewModel> OnChatCommandMessageReceived;
        public static void ChatCommandMessageReceived(ChatMessageViewModel chatMessage)
        {
            if (GlobalEvents.OnChatCommandMessageReceived != null)
            {
                GlobalEvents.OnChatCommandMessageReceived(null, chatMessage);
            }
        }

        public static event EventHandler<InteractiveSharedProjectModel> OnInteractiveSharedProjectAdded;
        public static void InteractiveSharedProjectAdded(InteractiveSharedProjectModel sharedProject)
        {
            if (GlobalEvents.OnInteractiveSharedProjectAdded != null)
            {
                GlobalEvents.OnInteractiveSharedProjectAdded(null, sharedProject);
            }
        }

        public static event EventHandler<InteractiveGameModel> OnInteractiveConnected;
        public static void InteractiveConnected(InteractiveGameModel game)
        {
            if (GlobalEvents.OnInteractiveConnected != null)
            {
                GlobalEvents.OnInteractiveConnected(null, game);
            }
        }

        public static event EventHandler OnInteractiveDisconnected;
        public static void InteractiveDisconnected()
        {
            if (GlobalEvents.OnInteractiveDisconnected != null)
            {
                GlobalEvents.OnInteractiveDisconnected(null, new EventArgs());
            }
        }

        public static event EventHandler OnGameQueueUpdated;
        public static void GameQueueUpdated()
        {
            if (GlobalEvents.OnGameQueueUpdated != null)
            {
                GlobalEvents.OnGameQueueUpdated(null, new EventArgs());
            }
        }

        public static event EventHandler<UserCurrencyDataViewModel> OnRankChanged;
        public static void RankChanged(UserCurrencyDataViewModel currency)
        {
            if (GlobalEvents.OnRankChanged != null)
            {
                GlobalEvents.OnRankChanged(null, currency);
            }
        }

        public static event EventHandler<UserQuoteViewModel> OnQuoteAdded;
        public static void QuoteAdded(UserQuoteViewModel quote)
        {
            if (GlobalEvents.OnQuoteAdded != null)
            {
                GlobalEvents.OnQuoteAdded(null, quote);
            }
        }

        public static event EventHandler<UserViewModel> OnFollowOccurred;
        public static void FollowOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnFollowOccurred != null)
            {
                GlobalEvents.OnFollowOccurred(null, user);
            }
        }

        public static event EventHandler<UserViewModel> OnUnfollowOccurred;
        public static void UnfollowOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnUnfollowOccurred != null)
            {
                GlobalEvents.OnUnfollowOccurred(null, user);
            }
        }

        public static event EventHandler<UserViewModel> OnSubscribeOccurred;
        public static void SubscribeOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnSubscribeOccurred != null)
            {
                GlobalEvents.OnSubscribeOccurred(null, user);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnResubscribeOccurred;
        public static void ResubscribeOccurred(Tuple<UserViewModel, int> user)
        {
            if (GlobalEvents.OnResubscribeOccurred != null)
            {
                GlobalEvents.OnResubscribeOccurred(null, user);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnHostOccurred;
        public static void HostOccurred(Tuple<UserViewModel, int> user)
        {
            if (GlobalEvents.OnHostOccurred != null)
            {
                GlobalEvents.OnHostOccurred(null, user);
            }
        }

        public static event EventHandler<UserDonationModel> OnDonationOccurred;
        public static void DonationOccurred(UserDonationModel donation)
        {
            if (GlobalEvents.OnDonationOccurred != null)
            {
                GlobalEvents.OnDonationOccurred(null, donation);
            }
        }

        public static event EventHandler OnSongRequestsChangedOccurred;
        public static void SongRequestsChangedOccurred()
        {
            if (GlobalEvents.OnSongRequestsChangedOccurred != null)
            {
                GlobalEvents.OnSongRequestsChangedOccurred(null, new EventArgs());
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnSparkUseOccurred;
        public static void SparkUseOccurred(Tuple<UserViewModel, int> spark)
        {
            if (GlobalEvents.OnSparkUseOccurred != null)
            {
                GlobalEvents.OnSparkUseOccurred(null, spark);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, ChatSkillModel>> OnChatSkillOccurred;
        public static void ChatSkillOccurred(Tuple<UserViewModel, ChatSkillModel> skill)
        {
            if (GlobalEvents.OnChatSkillOccurred != null)
            {
                GlobalEvents.OnChatSkillOccurred(null, skill);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, SkillInstanceModel>> OnSkillOccurred;
        public static void SkillOccurred(Tuple<UserViewModel, SkillInstanceModel> skill)
        {
            if (GlobalEvents.OnSkillOccurred != null)
            {
                GlobalEvents.OnSkillOccurred(null, skill);
            }
        }

        public static event EventHandler<PatronageStatusModel> OnPatronageUpdateOccurred;
        public static void PatronageUpdateOccurred(PatronageStatusModel patronageStatus)
        {
            if (GlobalEvents.OnPatronageUpdateOccurred != null)
            {
                GlobalEvents.OnPatronageUpdateOccurred(null, patronageStatus);
            }
        }

        public static event EventHandler<PatronageMilestoneModel> OnPatronageMilestoneReachedOccurred;
        public static void PatronageMilestoneReachedOccurred(PatronageMilestoneModel patronageMilestone)
        {
            if (GlobalEvents.OnPatronageMilestoneReachedOccurred != null)
            {
                GlobalEvents.OnPatronageMilestoneReachedOccurred(null, patronageMilestone);
            }
        }

        public static event EventHandler<ClipModel> OnMixerClipCreated;
        public static void MixerClipCreated(ClipModel clip)
        {
            if (GlobalEvents.OnMixerClipCreated != null)
            {
                GlobalEvents.OnMixerClipCreated(null, clip);
            }
        }
    }
}
