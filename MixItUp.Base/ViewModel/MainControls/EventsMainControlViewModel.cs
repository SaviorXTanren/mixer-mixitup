using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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

        public bool ShowPackIcon { get { return !string.IsNullOrEmpty(this.PackIconName); } }

        public EventCommandGroupViewModel(string name, string image = null, string packIconName = null)
        {
            this.Name = name;
            this.Image = image;
            this.PackIconName = packIconName;
        }
    }

    public class EventCommandItemViewModel
    {
        public EventTypeEnum EventType { get; set; }

        public EventCommandModel Command { get; set; }

        public EventCommandItemViewModel(EventCommandModel command)
        {
            this.Command = command;
            this.EventType = this.Command.EventType;
        }

        public EventCommandItemViewModel(EventTypeEnum eventType) { this.EventType = eventType; }

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
    }

    public class EventsMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<EventCommandGroupViewModel> EventCommandGroups { get; set; } = new ThreadSafeObservableCollection<EventCommandGroupViewModel>();

        public EventsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.RefreshCommands();
        }

        public void RefreshCommands()
        {
            this.EventCommandGroups.Clear();

            List<EventCommandGroupViewModel> commandGroups = new List<EventCommandGroupViewModel>();

            EventCommandGroupViewModel genericCommands = new EventCommandGroupViewModel(Resources.Generic, packIconName: "AlarmLight");
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelStreamStart));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelStreamStop));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelFollowed));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelHosted));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelRaided));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelSubscribed));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelResubscribed));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelSubscriptionGifted));
            genericCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChannelMassSubscriptionsGifted));
            commandGroups.Add(genericCommands);

            EventCommandGroupViewModel twitchCommands = new EventCommandGroupViewModel(Resources.Twitch, image: StreamingPlatforms.TwitchLogoImageAssetFilePath);
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelStreamStart));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelStreamStop));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelFollowed));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHosted));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelRaided));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelSubscribed));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelResubscribed));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelSubscriptionGifted));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelMassSubscriptionsGifted));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelBitsCheered));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelPointsRedeemed));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHypeTrainBegin));
            twitchCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHypeTrainEnd));
            commandGroups.Add(twitchCommands);

            EventCommandGroupViewModel trovoCommands = new EventCommandGroupViewModel(Resources.Trovo, image: StreamingPlatforms.TrovoLogoImageAssetFilePath);
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelFollowed));
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelRaided));
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelSubscribed));
            //trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelResubscribed));
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelSubscriptionGifted));
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoChannelMassSubscriptionsGifted));
            trovoCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TrovoSpellCast));
            commandGroups.Add(trovoCommands);

            EventCommandGroupViewModel glimeshCommands = new EventCommandGroupViewModel(Resources.Glimesh, image: StreamingPlatforms.GlimeshLogoImageAssetFilePath);
            glimeshCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.GlimeshChannelStreamStart));
            glimeshCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.GlimeshChannelStreamStop));
            glimeshCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.GlimeshChannelFollowed));
            commandGroups.Add(glimeshCommands);

            EventCommandGroupViewModel chatCommands = new EventCommandGroupViewModel(Resources.Chat, packIconName: "Chat");
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserFirstJoin));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserJoined));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserLeft));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserTimeout));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatEntranceCommand));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserBan));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatMessageReceived));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatWhisperReceived));
            chatCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ChatMessageDeleted));
            commandGroups.Add(chatCommands);

            EventCommandGroupViewModel donationCommands = new EventCommandGroupViewModel(Resources.Donations, packIconName: "Cash");
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.GenericDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamlabsDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamElementsDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamElementsMerchPurchase));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TipeeeStreamDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TreatStreamDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.RainmakerDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.TiltifyDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.ExtraLifeDonation));
            donationCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.JustGivingDonation));
            commandGroups.Add(donationCommands);

            EventCommandGroupViewModel patreonCommands = new EventCommandGroupViewModel(Resources.Patreon, packIconName: "Patreon");
            patreonCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.PatreonSubscribed));
            commandGroups.Add(patreonCommands);

            EventCommandGroupViewModel streamlootsCommands = new EventCommandGroupViewModel(Resources.Streamloots, packIconName: "CardsOutline");
            streamlootsCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsCardRedeemed));
            streamlootsCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsPackPurchased));
            streamlootsCommands.Commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsPackGifted));
            commandGroups.Add(streamlootsCommands);

            this.EventCommandGroups.AddRange(commandGroups);
        }

        private EventCommandItemViewModel GetEventCommand(EventTypeEnum eventType)
        {
            EventCommandModel command = ServiceManager.Get<EventService>().GetEventCommand(eventType);
            if (command != null)
            {
                return new EventCommandItemViewModel(command);
            }
            else
            {
                return new EventCommandItemViewModel(eventType);
            }
        }
    }
}
