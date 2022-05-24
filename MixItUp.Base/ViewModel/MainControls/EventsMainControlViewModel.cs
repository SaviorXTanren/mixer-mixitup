using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class EventCommandGroupViewModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string PackIconName { get; set; }

        public ThreadSafeObservableCollection<EventCommandItemViewModel> Commands { get; set; } = new ThreadSafeObservableCollection<EventCommandItemViewModel>();

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
                else if (this.EventType == EventTypeEnum.ExtraLifeDonation)
                {
                    return Resources.ExtraLife;
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
                else if (eventNumber >= 500 && eventNumber < 600)
                {
                    return Resources.Glimesh;
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
        public ThreadSafeObservableCollection<EventCommandGroupViewModel> EventCommandGroups { get; set; } = new ThreadSafeObservableCollection<EventCommandGroupViewModel>();

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
            commandGroups.Add(genericCommands);

            EventCommandGroupViewModel twitchCommands = new EventCommandGroupViewModel(Resources.Twitch, image: StreamingPlatforms.TwitchLogoImageAssetFilePath);
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelStreamStart));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelStreamStop));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelFollowed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHosted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelRaided));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelSubscribed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelResubscribed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelSubscriptionGifted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelMassSubscriptionsGifted));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelBitsCheered));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelPointsRedeemed));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHypeTrainBegin));
            twitchCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TwitchChannelHypeTrainEnd));
            commandGroups.Add(twitchCommands);

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

            EventCommandGroupViewModel glimeshCommands = new EventCommandGroupViewModel(Resources.Glimesh, image: StreamingPlatforms.GlimeshLogoImageAssetFilePath);
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelStreamStart));
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelStreamStop));
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelFollowed));
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelSubscribed));
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelSubscriptionGifted));
            glimeshCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GlimeshChannelDonation));
            commandGroups.Add(glimeshCommands);

            EventCommandGroupViewModel chatCommands = new EventCommandGroupViewModel(Resources.Chat, packIconName: "Chat");
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserEntranceCommand));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserFirstJoin));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserJoined));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserLeft));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatMessageReceived));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatWhisperReceived));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatMessageDeleted));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserTimeout));
            chatCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ChatUserBan));
            commandGroups.Add(chatCommands);

            EventCommandGroupViewModel donationCommands = new EventCommandGroupViewModel(Resources.Donations, packIconName: "Cash");
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.GenericDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlabsDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamElementsDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamElementsMerchPurchase));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TipeeeStreamDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TreatStreamDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.RainmakerDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.TiltifyDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.ExtraLifeDonation));
            donationCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.JustGivingDonation));
            commandGroups.Add(donationCommands);

            EventCommandGroupViewModel patreonCommands = new EventCommandGroupViewModel(Resources.Patreon, packIconName: "Patreon");
            patreonCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.PatreonSubscribed));
            commandGroups.Add(patreonCommands);

            EventCommandGroupViewModel streamlootsCommands = new EventCommandGroupViewModel(Resources.Streamloots, packIconName: "CardsOutline");
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsCardRedeemed));
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsPackPurchased));
            streamlootsCommands.Commands.Add(new EventCommandItemViewModel(EventTypeEnum.StreamlootsPackGifted));
            commandGroups.Add(streamlootsCommands);

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
