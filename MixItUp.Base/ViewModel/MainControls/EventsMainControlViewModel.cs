using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class EventCommandGroupViewModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string PackIconName { get; set; }

        public ObservableCollection<EventCommandItemViewModel> Commands { get; set; } = new ObservableCollection<EventCommandItemViewModel>();

        public bool ShowImage { get { return !string.IsNullOrEmpty(this.Image); } }

        public bool ShowPackIcon { get { return !this.ShowImage; } }

        public EventCommandGroupViewModel(string name, string image = null, string packIconName = null)
        {
            this.Name = name;
            this.Image = image;
            this.PackIconName = packIconName;
        }
    }

    public class EventCommandItemViewModel : UIViewModelBase
    {
        public EventTypeEnum EventType { get; set; }

        public EventCommandModel Command { get; set; }

        public EventCommandItemViewModel(EventTypeEnum eventType)
        {
            this.EventType = eventType;
            this.RefreshCommand();
        }

        public string Name { get { return EnumLocalizationHelper.GetLocalizedName(this.EventType); } }

        public string Service
        {
            get
            {
                int eventNumber = (int)this.EventType;
                if (this.EventType == EventTypeEnum.StreamlabsDonation)
                {
                    return Resources.Streamlabs;
                }
                else if (this.EventType == EventTypeEnum.TiltifyDonation)
                {
                    return Resources.Tiltify;
                }
                else if (this.EventType == EventTypeEnum.DonorDriveDonation || this.EventType == EventTypeEnum.DonorDriveDonationIncentive || this.EventType == EventTypeEnum.DonorDriveDonationMilestone ||
                    this.EventType == EventTypeEnum.DonorDriveDonationTeamIncentive || this.EventType == EventTypeEnum.DonorDriveDonationTeamMilestone)
                {
                    return Resources.DonorDrive;
                }
                else if (this.EventType == EventTypeEnum.TipeeeStreamDonation)
                {
                    return Resources.TipeeeStream;
                }
                else if (this.EventType == EventTypeEnum.TreatStreamDonation)
                {
                    return Resources.TreatStream;
                }
                else if (this.EventType == EventTypeEnum.RainmakerDonation)
                {
                    return Resources.Rainmaker;
                }
                else if (this.EventType == EventTypeEnum.PatreonSubscribed)
                {
                    return Resources.Patreon;
                }
                else if (this.EventType == EventTypeEnum.JustGivingDonation)
                {
                    return Resources.JustGiving;
                }
                else if (this.EventType == EventTypeEnum.StreamlootsCardRedeemed || this.EventType == EventTypeEnum.StreamlootsPackGifted || this.EventType == EventTypeEnum.StreamlootsPackPurchased)
                {
                    return Resources.Streamloots;
                }
                else if (this.EventType == EventTypeEnum.StreamElementsDonation || this.EventType == EventTypeEnum.StreamElementsMerchPurchase)
                {
                    return Resources.StreamElements;
                }
                else if (eventNumber >= 200 && eventNumber < 300)
                {
                    return Resources.Twitch;
                }
                else if (eventNumber >= 300 && eventNumber < 400)
                {
                    return Resources.YouTube;
                }
                else if (eventNumber >= 400 && eventNumber < 500)
                {
                    return Resources.Trovo;
                }
                else
                {
                    return Resources.Generic;
                }
            }
        }

        public bool IsNewCommand { get { return this.Command == null; } }

        public bool IsExistingCommand { get { return this.Command != null; } }

        public void RefreshCommand()
        {
            this.Command = ServiceManager.Get<EventService>().GetEventCommand(this.EventType);
            this.NotifyPropertyChanged("Command");
            this.NotifyPropertyChanged("Name");
            this.NotifyPropertyChanged("Service");
            this.NotifyPropertyChanged("IsNewCommand");
            this.NotifyPropertyChanged("IsExistingCommand");
        }
    }

    public class EventsMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<EventCommandGroupViewModel> EventCommandGroups { get; set; } = new ObservableCollection<EventCommandGroupViewModel>();

        public Dictionary<EventTypeEnum, EventCommandItemViewModel> EventTypeItems { get; set; } = new Dictionary<EventTypeEnum, EventCommandItemViewModel>();

        public EventsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.EventCommandGroups.Clear();

            List<EventCommandGroupViewModel> commandGroups = new List<EventCommandGroupViewModel>();

            EventCommandGroupViewModel genericCommands = new EventCommandGroupViewModel(Resources.Generic, packIconName: "AlarmLight");
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ApplicationLaunch));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ApplicationExit));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelStreamStart));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelStreamStop));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelFollowed));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelHosted));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelRaided));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelSubscribed));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelResubscribed));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelSubscriptionGifted));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChannelMassSubscriptionsGifted));
            genericCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GenericDonation));
            commandGroups.Add(genericCommands);

            EventCommandGroupViewModel twitchCommands = new EventCommandGroupViewModel(Resources.Twitch, image: StreamingPlatforms.TwitchLogoImageAssetFilePath);
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelStreamStart));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelStreamStop));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelUpdated));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelFollowed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelRaided));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelOutgoingRaidCompleted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelSubscribed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelResubscribed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelSubscriptionGifted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelMassSubscriptionsGifted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHighlightedMessage));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelUserIntro));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelPowerUpMessageEffect));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelPowerUpGigantifiedEmote));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelPowerUpCelebration));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelBitsCheered));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelPointsRedeemed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelCharityDonation));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelAdUpcoming));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelAdStarted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelAdEnded));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHypeTrainBegin));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHypeTrainLevelUp));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHypeTrainEnd));
            commandGroups.Add(twitchCommands);

            EventCommandGroupViewModel youtubeCommands = new EventCommandGroupViewModel(Resources.YouTube, image: StreamingPlatforms.YouTubeLogoImageAssetFilePath);
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelStreamStart));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelStreamStop));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelNewMember));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelMemberMilestone));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelMembershipGifted));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelMassMembershipGifted));
            youtubeCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.YouTubeChannelSuperChat));
            commandGroups.Add(youtubeCommands);

            EventCommandGroupViewModel trovoCommands = new EventCommandGroupViewModel(Resources.Trovo, image: StreamingPlatforms.TrovoLogoImageAssetFilePath);
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelStreamStart));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelStreamStop));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelFollowed));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelRaided));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelSubscribed));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelResubscribed));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelSubscriptionGifted));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelMassSubscriptionsGifted));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelSpellCast));
            trovoCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TrovoChannelMagicChat));
            commandGroups.Add(trovoCommands);

            EventCommandGroupViewModel chatCommands = new EventCommandGroupViewModel(Resources.Chat, packIconName: "Chat");
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserEntranceCommand));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserFirstMessage));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatMessageReceived));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatWhisperReceived));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatMessageDeleted));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserTimeout));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserBan));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserFirstJoin));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserJoined));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserLeft));
            commandGroups.Add(chatCommands);

            EventCommandGroupViewModel donationCommands = new EventCommandGroupViewModel(Resources.Donations, packIconName: "Cash");
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.DonorDriveDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.DonorDriveDonationIncentive));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.DonorDriveDonationMilestone));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.DonorDriveDonationTeamIncentive));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.DonorDriveDonationTeamMilestone));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlabsDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamElementsDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamElementsMerchPurchase));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TipeeeStreamDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TreatStreamDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.RainmakerDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TiltifyDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.JustGivingDonation));
            commandGroups.Add(donationCommands);

            EventCommandGroupViewModel streamlootsCommands = new EventCommandGroupViewModel(Resources.Streamloots, packIconName: "CardsOutline");
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsCardRedeemed));
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsPackPurchased));
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsPackGifted));
            commandGroups.Add(streamlootsCommands);

            EventCommandGroupViewModel crowdControlCommands = new EventCommandGroupViewModel(Resources.CrowdControl, packIconName: "ControllerClassic");
            crowdControlCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.CrowdControlEffectRedeemed));
            commandGroups.Add(crowdControlCommands);

            EventCommandGroupViewModel pulsoidCommands = new EventCommandGroupViewModel(Resources.Pulsoid, packIconName: "HeartPulse");
            pulsoidCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.PulsoidHeartRateChanged));
            commandGroups.Add(pulsoidCommands);

            EventCommandGroupViewModel patreonCommands = new EventCommandGroupViewModel(Resources.Patreon, packIconName: "Patreon");
            patreonCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.PatreonSubscribed));
            commandGroups.Add(patreonCommands);

            this.EventCommandGroups.AddRange(commandGroups);

            foreach (EventCommandGroupViewModel group in this.EventCommandGroups)
            {
                foreach (EventCommandItemViewModel item in group.Commands)
                {
                    this.EventTypeItems[item.EventType] = item;
                }
            }
        }
    }
}
