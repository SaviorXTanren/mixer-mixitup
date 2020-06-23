using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.User
{
    public class UserMetricViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public UserMetricViewModel(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class UserDataEditorWindowViewModel : WindowViewModelBase
    {
        public const string UserEntranceCommandName = "Entrance Command";

        public UserViewModel User { get; private set; }

        public int ViewingHours
        {
            get { return this.User.Data.ViewingHoursPart; }
            set
            {
                this.User.Data.ViewingHoursPart = value;
                this.NotifyPropertyChanged();
            }
        }

        public int ViewingMinutes
        {
            get { return this.User.Data.ViewingMinutesPart; }
            set
            {
                this.User.Data.ViewingMinutesPart = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ViewingHours");
            }
        }

        public ObservableCollection<ChatCommand> UserOnlyChatCommands { get; set; } = new ObservableCollection<ChatCommand>();
        public bool HasUserOnlyChatCommands { get { return this.UserOnlyChatCommands.Count > 0; } }

        public CustomCommand EntranceCommand
        {
            get { return this.User.Data.EntranceCommand; }
            set
            {
                this.User.Data.EntranceCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasEntranceCommand");
                this.NotifyPropertyChanged("DoesNotHaveEntranceCommand");
            }
        }
        public bool HasEntranceCommand { get { return this.EntranceCommand != null; } }
        public bool DoesNotHaveEntranceCommand { get { return !this.HasEntranceCommand; } }

        public bool IsPatreonConnected { get { return ChannelSession.Services.Patreon.IsConnected; } }
        public IEnumerable<PatreonCampaignMember> PatreonUsers { get { return ChannelSession.Services.Patreon.CampaignMembers; } }
        public PatreonCampaignMember PatreonUser
        {
            get { return this.User.Data.PatreonUser; }
            set
            {
                this.User.Data.PatreonUser = value;
                if (this.User.Data.PatreonUser != null)
                {
                    this.User.Data.PatreonUserID = value.UserID;
                }
                else
                {
                    this.User.Data.PatreonUserID = null;
                }
                this.NotifyPropertyChanged();
            }
        }

        public bool CurrencyRankExempt
        {
            get { return this.User.Data.IsCurrencyRankExempt; }
            set
            {
                this.User.Data.IsCurrencyRankExempt = value;
                if (this.CurrencyRankExempt)
                {
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.ResetAmount(this.User.Data);
                    }
                    ChannelSession.Settings.UserData.ManualValueChanged(this.User.ID);
                }
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<UserMetricViewModel> Metrics1 { get; private set; } = new ObservableCollection<UserMetricViewModel>();
        public ObservableCollection<UserMetricViewModel> Metrics2 { get; private set; } = new ObservableCollection<UserMetricViewModel>();

        public UserDataEditorWindowViewModel(UserDataModel user)
        {
            this.User = ChannelSession.Services.User.GetUserByID(user.ID);
            if (this.User == null)
            {
                this.User = new UserViewModel(user);
            }
        }

        public async Task Load()
        {
            await this.User.RefreshDetails(force: true);

            this.RefreshUserOnlyChatCommands();

            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.StreamsWatched, this.User.Data.TotalStreamsWatched.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.CumulativeMonthsSubbed, this.User.Data.TotalMonthsSubbed.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.SubsGifted, this.User.Data.TotalSubsGifted.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.SubsReceived, this.User.Data.TotalSubsReceived.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.ChatMessagesSent, this.User.Data.TotalChatMessageSent.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.CommandsRun, this.User.Data.TotalCommandsRun.ToString()));

            this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.TaggedInChat, this.User.Data.TotalTimesTagged.ToString()));
            this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.AmountDonated, string.Format("{0:C}", Math.Round(this.User.Data.TotalAmountDonated, 2))));
        }

        public void AddUserOnlyChatCommand(ChatCommand command)
        {
            this.User.Data.CustomCommands.Add(command);
            this.RefreshUserOnlyChatCommands();
        }

        public void RemoveUserOnlyChatCommand(ChatCommand command)
        {
            this.User.Data.CustomCommands.Remove(command);
            this.RefreshUserOnlyChatCommands();
        }

        public void RefreshUserOnlyChatCommands()
        {
            this.UserOnlyChatCommands.Clear();
            foreach (ChatCommand command in this.User.Data.CustomCommands)
            {
                this.UserOnlyChatCommands.Add(command);
            }
            this.NotifyPropertyChanged("HasUserOnlyChatCommands");
        }
    }
}
