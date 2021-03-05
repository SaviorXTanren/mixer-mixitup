using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
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

    public class UserDataEditorWindowViewModel : UIViewModelBase
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

        public ObservableCollection<UserOnlyChatCommandModel> UserOnlyChatCommands { get; set; } = new ObservableCollection<UserOnlyChatCommandModel>().EnableSync();
        public bool HasUserOnlyChatCommands { get { return this.UserOnlyChatCommands.Count > 0; } }

        public CommandModelBase EntranceCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.User.Data.EntranceCommandID); }
            set
            {
                if (value == null)
                {
                    ChannelSession.Settings.RemoveCommand(this.User.Data.EntranceCommandID);
                    this.User.Data.EntranceCommandID = Guid.Empty;
                }
                else
                {
                    this.User.Data.EntranceCommandID = value.ID;
                }
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasEntranceCommand");
                this.NotifyPropertyChanged("DoesNotHaveEntranceCommand");
            }
        }
        public bool HasEntranceCommand { get { return this.EntranceCommand != null; } }
        public bool DoesNotHaveEntranceCommand { get { return !this.HasEntranceCommand; } }

        public bool IsPatreonConnected { get { return ServiceManager.Get<PatreonService>().IsConnected; } }
        public IEnumerable<PatreonCampaignMember> PatreonUsers { get { return ServiceManager.Get<PatreonService>().CampaignMembers.ToList(); } }
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

        public ObservableCollection<UserMetricViewModel> Metrics1 { get; private set; } = new ObservableCollection<UserMetricViewModel>().EnableSync();
        public ObservableCollection<UserMetricViewModel> Metrics2 { get; private set; } = new ObservableCollection<UserMetricViewModel>().EnableSync();

        public UserDataEditorWindowViewModel(UserDataModel user)
        {
            this.User = ServiceManager.Get<UserService>().GetUserByID(user.ID);
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
            this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.BitsCheered, this.User.Data.TotalBitsCheered.ToString()));
        }

        public void AddUserOnlyChatCommand(UserOnlyChatCommandModel command)
        {
            this.User.Data.CustomCommandIDs.Add(command.ID);
            this.RefreshUserOnlyChatCommands();
        }

        public void RemoveUserOnlyChatCommand(UserOnlyChatCommandModel command)
        {
            this.User.Data.CustomCommandIDs.Remove(command.ID);
            ChannelSession.Settings.RemoveCommand(command.ID);
            this.RefreshUserOnlyChatCommands();
        }

        public void RefreshUserOnlyChatCommands()
        {
            this.UserOnlyChatCommands.Clear();
            foreach (Guid commandID in this.User.Data.CustomCommandIDs)
            {
                UserOnlyChatCommandModel command = ChannelSession.Settings.GetCommand<UserOnlyChatCommandModel>(commandID);
                if (command != null)
                {
                    this.UserOnlyChatCommands.Add(command);
                }
            }
            this.NotifyPropertyChanged("HasUserOnlyChatCommands");
        }
    }
}
