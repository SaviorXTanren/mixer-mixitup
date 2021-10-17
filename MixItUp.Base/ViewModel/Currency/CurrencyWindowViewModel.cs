using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.ViewModel.Currency
{
    public enum CurrencyAcquireRateTypeEnum
    {
        [Name("1PerMinute")]
        Minutes,
        [Name("1PerHour")]
        Hours,
        [Name("1PerBit")]
        Bits,
        Custom,
        Disabled,
    }

    public class CurrencyWindowViewModel : UIViewModelBase
    {
        public CurrencyModel Currency { get; set; }

        public bool IsNew { get { return this.Currency == null; } }
        public bool IsExisting { get { return !this.IsNew; } }

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

        public bool IsPrimary
        {
            get { return this.isPrimary; }
            set
            {
                this.isPrimary = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isPrimary;

        public bool IsRank
        {
            get { return this.isRank; }
            set
            {
                this.isRank = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CurrencyRankIdentifierString");
            }
        }
        private bool isRank;

        public int MaxAmount
        {
            get { return this.maxAmount; }
            set
            {
                this.maxAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxAmount;

        public CurrencyAcquireRateTypeEnum OnlineRate
        {
            get { return this.onlineRate; }
            set
            {
                this.onlineRate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsOnlineRateTimeBased");
                this.NotifyPropertyChanged("IsCustomOnlineRate");

                if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Minutes || this.OnlineRate == CurrencyAcquireRateTypeEnum.Hours)
                {
                    this.OnlineRateAmount = 1;
                    if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Minutes)
                    {
                        this.OnlineRateInterval = 1;
                    }
                    else if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Hours)
                    {
                        this.OnlineRateInterval = 60;
                    }
                }
                else if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Bits)
                {
                    this.OnlineRateAmount = 1;
                    this.OnlineRateInterval = 1;

                    this.OfflineRate = CurrencyAcquireRateTypeEnum.Disabled;

                    this.RegularBonus = 0;
                    this.SubscriberBonus = 0;
                    this.ModeratorBonus = 0;
                    this.OnFollowBonus = 0;
                    this.OnHostBonus = 0;
                    this.OnSubscribeBonus = 0;

                    this.MinimumActiveRate = 0;
                }
                else
                {
                    this.OnlineRateAmount = 0;
                    this.OnlineRateInterval = 0;
                }
            }
        }
        private CurrencyAcquireRateTypeEnum onlineRate;
        public List<CurrencyAcquireRateTypeEnum> OnlineRates { get; private set; } = new List<CurrencyAcquireRateTypeEnum>(EnumHelper.GetEnumList<CurrencyAcquireRateTypeEnum>());
        public bool IsOnlineRateTimeBased { get { return this.OnlineRate != CurrencyAcquireRateTypeEnum.Bits; } }
        public bool IsCustomOnlineRate { get { return this.OnlineRate == CurrencyAcquireRateTypeEnum.Custom; } }
        public int OnlineRateAmount
        {
            get { return this.onlineRateAmount; }
            set
            {
                this.onlineRateAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int onlineRateAmount = 0;
        public int OnlineRateInterval
        {
            get { return this.onlineRateInterval; }
            set
            {
                this.onlineRateInterval = value;
                this.NotifyPropertyChanged();
            }
        }
        private int onlineRateInterval = 0;

        public CurrencyAcquireRateTypeEnum OfflineRate
        {
            get { return this.offlineRate; }
            set
            {
                this.offlineRate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsOnlineRateTimeBased");
                this.NotifyPropertyChanged("IsCustomOfflineRate");

                if (this.OfflineRate == CurrencyAcquireRateTypeEnum.Minutes || this.OfflineRate == CurrencyAcquireRateTypeEnum.Hours)
                {
                    this.OfflineRateAmount = 1;
                    if (this.OfflineRate == CurrencyAcquireRateTypeEnum.Minutes)
                    {
                        this.OfflineRateInterval = 1;
                    }
                    else if (this.OfflineRate == CurrencyAcquireRateTypeEnum.Hours)
                    {
                        this.OfflineRateInterval = 60;
                    }
                }
                else
                {
                    this.OfflineRateAmount = 0;
                    this.OfflineRateInterval = 0;
                }
            }
        }
        private CurrencyAcquireRateTypeEnum offlineRate;
        public List<CurrencyAcquireRateTypeEnum> OfflineRates { get; private set; } = new List<CurrencyAcquireRateTypeEnum>() { CurrencyAcquireRateTypeEnum.Minutes, CurrencyAcquireRateTypeEnum.Hours, CurrencyAcquireRateTypeEnum.Custom, CurrencyAcquireRateTypeEnum.Disabled };
        public bool IsCustomOfflineRate { get { return this.OfflineRate == CurrencyAcquireRateTypeEnum.Custom; } }
        public int OfflineRateAmount
        {
            get { return this.offlineRateAmount; }
            set
            {
                this.offlineRateAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int offlineRateAmount = 0;
        public int OfflineRateInterval
        {
            get { return this.offlineRateInterval; }
            set
            {
                this.offlineRateInterval = value;
                this.NotifyPropertyChanged();
            }
        }
        private int offlineRateInterval = 0;

        public int RegularBonus
        {
            get { return this.regularBonus; }
            set
            {
                this.regularBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int regularBonus = 0;
        public int SubscriberBonus
        {
            get { return this.subscriberBonus; }
            set
            {
                this.subscriberBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int subscriberBonus = 0;
        public int ModeratorBonus
        {
            get { return this.moderatorBonus; }
            set
            {
                this.moderatorBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int moderatorBonus = 0;

        public int OnFollowBonus
        {
            get { return this.onFollowBonus; }
            set
            {
                this.onFollowBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int onFollowBonus = 0;
        public int OnHostBonus
        {
            get { return this.onHostBonus; }
            set
            {
                this.onHostBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int onHostBonus = 0;
        public int OnSubscribeBonus
        {
            get { return this.onSubscribeBonus; }
            set
            {
                this.onSubscribeBonus = value;
                this.NotifyPropertyChanged();
            }
        }
        private int onSubscribeBonus = 0;

        public int MinimumActiveRate
        {
            get { return this.minimumActiveRate; }
            set
            {
                this.minimumActiveRate = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimumActiveRate = 0;

        public CurrencyResetRateEnum AutomaticResetRate
        {
            get { return this.automaticResetRate; }
            set
            {
                this.automaticResetRate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("AutomaticResetStartTimeSelectable");
            }
        }
        private CurrencyResetRateEnum automaticResetRate;
        public List<CurrencyResetRateEnum> AutomaticResetRates { get; private set; } = new List<CurrencyResetRateEnum>(EnumHelper.GetEnumList<CurrencyResetRateEnum>());
        public DateTimeOffset AutomaticResetStartTime
        {
            get { return this.automaticResetStartTime; }
            set
            {
                this.automaticResetStartTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private DateTimeOffset automaticResetStartTime = DateTimeOffset.Now;
        public bool AutomaticResetStartTimeSelectable { get { return this.AutomaticResetRate == CurrencyResetRateEnum.Weekly || this.AutomaticResetRate == CurrencyResetRateEnum.Monthly || this.AutomaticResetRate == CurrencyResetRateEnum.Yearly; } }

        public ICommand ManualResetCommand { get; private set; }
        public ICommand RetroactivelyGivePointsCommand { get; private set; }

        public string ImportFromFileText
        {
            get { return this.importFromFileText; }
            set
            {
                this.importFromFileText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string importFromFileText = MixItUp.Base.Resources.ImportFromFile;
        public ICommand ImportFromFileCommand { get; private set; }

        public ICommand ExportToFileCommand { get; private set; }

        public CommandModelBase RankChangedCommand
        {
            get { return this.rankChangedCommand; }
            set
            {
                this.rankChangedCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasRankChangedCommand");
                this.NotifyPropertyChanged("DoesNotHaveRankChangedCommand");
            }
        }
        private CommandModelBase rankChangedCommand;
        public bool HasRankChangedCommand { get { return this.RankChangedCommand != null; } }
        public bool DoesNotHaveRankChangedCommand { get { return !this.HasRankChangedCommand; } }

        public CommandModelBase RankDownCommand
        {
            get { return this.rankDownCommand; }
            set
            {
                this.rankDownCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasRankDownCommand");
                this.NotifyPropertyChanged("DoesNotHaveRankDownCommand");
            }
        }
        private CommandModelBase rankDownCommand;
        public bool HasRankDownCommand { get { return this.RankDownCommand != null; } }
        public bool DoesNotHaveRankDownCommand { get { return !this.HasRankDownCommand; } }

        public ThreadSafeObservableCollection<RankModel> Ranks { get; set; } = new ThreadSafeObservableCollection<RankModel>();

        public string NewRankName
        {
            get { return this.newRankName; }
            set
            {
                this.newRankName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string newRankName;
        public int NewRankAmount
        {
            get { return this.newRankAmount; }
            set
            {
                this.newRankAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int newRankAmount;
        public ICommand AddRankCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public ICommand HelpCommand { get; private set; }

        public string CurrencyRankIdentifierString { get { return (this.IsRank) ? MixItUp.Base.Resources.Rank.ToLower() : MixItUp.Base.Resources.Currency.ToLower(); } }

        private Dictionary<Guid, int> userImportData = new Dictionary<Guid, int>();

        public CurrencyWindowViewModel(CurrencyModel currency)
            : this()
        {
            this.Currency = currency;

            this.Name = this.Currency.Name;
            this.IsPrimary = this.Currency.IsPrimary;
            if (this.Currency.MaxAmount != int.MaxValue)
            {
                this.MaxAmount = this.Currency.MaxAmount;
            }

            if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.Bits)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Bits;
            }
            else if (this.Currency.IsOnlineIntervalMinutes)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Minutes;
            }
            else if (this.Currency.IsOnlineIntervalHours)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Hours;
            }
            else if (this.Currency.IsOnlineIntervalDisabled)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Disabled;
            }
            else
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Custom;
            }
            this.OnlineRateAmount = this.Currency.AcquireAmount;
            this.OnlineRateInterval = this.Currency.AcquireInterval;

            if (this.Currency.IsOfflineIntervalMinutes)
            {
                this.OfflineRate = CurrencyAcquireRateTypeEnum.Minutes;
            }
            else if (this.Currency.IsOfflineIntervalHours)
            {
                this.OfflineRate = CurrencyAcquireRateTypeEnum.Hours;
            }
            else if (this.Currency.IsOfflineIntervalDisabled)
            {
                this.OfflineRate = CurrencyAcquireRateTypeEnum.Disabled;
            }
            else
            {
                this.OfflineRate = CurrencyAcquireRateTypeEnum.Custom;
            }
            this.OfflineRateAmount = this.Currency.OfflineAcquireAmount;
            this.OfflineRateInterval = this.Currency.OfflineAcquireInterval;

            this.RegularBonus = this.Currency.RegularBonus;
            this.SubscriberBonus = this.Currency.SubscriberBonus;
            this.ModeratorBonus = this.Currency.ModeratorBonus;

            this.OnFollowBonus = this.Currency.OnFollowBonus;
            this.OnHostBonus = this.Currency.OnHostBonus;
            this.OnSubscribeBonus = this.Currency.OnSubscribeBonus;

            this.MinimumActiveRate = this.Currency.MinimumActiveRate;

            this.AutomaticResetRate = this.Currency.ResetInterval;
            if (this.Currency.ResetStartCadence != DateTimeOffset.MinValue)
            {
                this.AutomaticResetStartTime = this.Currency.ResetStartCadence;
            }

            this.IsRank = this.Currency.IsRank;
            if (this.IsRank)
            {
                this.RankChangedCommand = this.Currency.RankChangedCommand;
                this.RankDownCommand = this.Currency.RankDownCommand;
                this.Ranks.AddRange(this.Currency.Ranks.OrderBy(r => r.Amount));
            }
        }

        public CurrencyWindowViewModel()
        {
            if (ChannelSession.Settings.Currency.All(c => !c.Value.IsPrimary))
            {
                this.IsPrimary = true;
            }

            this.OnlineRate = CurrencyAcquireRateTypeEnum.Minutes;
            this.OfflineRate = CurrencyAcquireRateTypeEnum.Disabled;

            this.AutomaticResetRate = CurrencyResetRateEnum.Never;

            this.AddRankCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.NewRankName))
                {
                    await DialogHelper.ShowMessage(Resources.RankRequired);
                    return;
                }

                if (this.NewRankAmount < 0)
                {
                    await DialogHelper.ShowMessage(Resources.MinimumAmountRequired);
                    return;
                }

                if (this.Ranks.Any(r => r.Name.Equals(this.NewRankName) || r.Amount == this.NewRankAmount))
                {
                    await DialogHelper.ShowMessage(Resources.UniqueRankNameAndMinimumAmountRequired);
                    return;
                }

                RankModel newRank = new RankModel(this.NewRankName, this.NewRankAmount);
                this.Ranks.Add(newRank);

                var tempRanks = this.Ranks.ToList();

                this.Ranks.ClearAndAddRange(tempRanks.OrderBy(r => r.Amount));

                this.NewRankName = string.Empty;
                this.NewRankAmount = 0;
            });

            this.ManualResetCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format(Resources.ResetCurrencyRankPointsPrompt, this.CurrencyRankIdentifierString)))
                {
                    if (this.Currency != null)
                    {
                        await this.Currency.Reset();
                    }
                }
            });

            this.RetroactivelyGivePointsCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format(Resources.RetroactivelyGivePointsPrompt1 +
                    Environment.NewLine + Environment.NewLine + Resources.RetroactivelyGivePointsPrompt2 +
                    Environment.NewLine + Environment.NewLine + Resources.RetroactivelyGivePointsPrompt3, this.CurrencyRankIdentifierString)))
                {
                    if (this.Currency != null && this.Currency.AcquireInterval > 0)
                    {
                        if (this.Currency.SpecialTracking != CurrencySpecialTrackingEnum.None)
                        {
                            await DialogHelper.ShowMessage(Resources.RetroactiveUnsupported);
                            return;
                        }

                        await ChannelSession.Settings.LoadAllUserData();

                        await this.Currency.Reset();
                        foreach (MixItUp.Base.Model.User.UserDataModel userData in ChannelSession.Settings.UserData.Values)
                        {
                            int intervalsToGive = userData.ViewingMinutes / this.Currency.AcquireInterval;
                            this.Currency.AddAmount(userData, this.Currency.AcquireAmount * intervalsToGive);
                            if (userData.TwitchUserRoles.Contains(UserRoleEnum.Mod) || userData.TwitchUserRoles.Contains(UserRoleEnum.ChannelEditor))
                            {
                                this.Currency.AddAmount(userData, this.Currency.ModeratorBonus * intervalsToGive);
                            }
                            else if (userData.TwitchUserRoles.Contains(UserRoleEnum.Subscriber))
                            {
                                this.Currency.AddAmount(userData, this.Currency.SubscriberBonus * intervalsToGive);
                            }
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                    }
                }
            });

            this.ImportFromFileCommand = this.CreateCommand(async () =>
            {
                this.userImportData.Clear();
                if (await DialogHelper.ShowConfirmation(string.Format(Resources.ImportPointsPrompt1 +
                    Environment.NewLine + Environment.NewLine + Resources.ImportPointsPrompt2, this.CurrencyRankIdentifierString)))
                {
                    try
                    {
                        string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            string fileContents = await ChannelSession.Services.FileService.ReadFile(filePath);
                            string[] lines = fileContents.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Count() > 0)
                            {
                                foreach (string line in lines)
                                {
                                    long id = 0;
                                    string username = null;
                                    int amount = 0;

                                    string[] segments = line.Split(new string[] { " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
                                    if (segments.Count() == 2)
                                    {
                                        if (!int.TryParse(segments[1], out amount))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }

                                        if (!long.TryParse(segments[0], out id))
                                        {
                                            username = segments[0];
                                        }
                                    }
                                    else if (segments.Count() == 3)
                                    {
                                        if (!long.TryParse(segments[0], out id))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }

                                        if (!int.TryParse(segments[2], out amount))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("File is not in the correct format");
                                    }

                                    UserViewModel user = null;
                                    if (amount > 0)
                                    {
                                        if (id > 0)
                                        {
                                            UserDataModel userData = await ChannelSession.Settings.GetUserDataByPlatformID(StreamingPlatformTypeEnum.Twitch, id.ToString());
                                            if (userData != null)
                                            {
                                                user = new UserViewModel(userData);
                                            }
                                            else
                                            {
                                                UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByID(id.ToString());
                                                if (twitchUser != null)
                                                {
                                                    user = await UserViewModel.Create(twitchUser);
                                                }
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(username))
                                        {
                                            UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(username);
                                            if (twitchUser != null)
                                            {
                                                user = await UserViewModel.Create(twitchUser);
                                            }
                                        }
                                    }

                                    if (user != null)
                                    {
                                        if (!this.userImportData.ContainsKey(user.ID))
                                        {
                                            this.userImportData[user.ID] = amount;
                                        }
                                        this.userImportData[user.ID] = Math.Max(this.userImportData[user.ID], amount);
                                        this.ImportFromFileText = string.Format("{0} {1}...", this.userImportData.Count(), MixItUp.Base.Resources.Imported);
                                    }
                                }

                                await ChannelSession.Settings.LoadAllUserData();

                                foreach (var kvp in this.userImportData)
                                {
                                    if (ChannelSession.Settings.UserData.ContainsKey(kvp.Key))
                                    {
                                        MixItUp.Base.Model.User.UserDataModel userData = ChannelSession.Settings.UserData[kvp.Key];
                                        this.Currency.SetAmount(userData, kvp.Value);
                                    }
                                }

                                this.ImportFromFileText = MixItUp.Base.Resources.ImportFromFile;
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    await DialogHelper.ShowMessage(Resources.CurrencyImportFailed +
                        Environment.NewLine + Environment.NewLine + "<USERNAME> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <USERNAME> <AMOUNT>");

                    this.ImportFromFileText = MixItUp.Base.Resources.ImportFromFile;
                }
            });

            this.ExportToFileCommand = this.CreateCommand(async () =>
            {
                await ChannelSession.Settings.LoadAllUserData();

                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(this.Currency.Name + " Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    StringBuilder fileContents = new StringBuilder();
                    foreach (MixItUp.Base.Model.User.UserDataModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        fileContents.AppendLine(string.Format("{0} {1} {2}", userData.TwitchID, userData.Username, this.Currency.GetAmount(userData)));
                    }
                    await ChannelSession.Services.FileService.SaveFile(filePath, fileContents.ToString());
                }
            });

            this.HelpCommand = this.CreateCommand(() =>
            {
                ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency,-Rank,-&-Inventory");
            });
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                await DialogHelper.ShowMessage(string.Format(Resources.CurrencyRankNameRequired, this.CurrencyRankIdentifierString));
                return false;
            }

            if (this.Name.Any(c => char.IsDigit(c)))
            {
                await DialogHelper.ShowMessage(Resources.CurrencyRankNameNoDigits);
                return false;
            }

            CurrencyModel dupeCurrency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeCurrency != null && (this.Currency == null || !this.Currency.ID.Equals(dupeCurrency.ID)))
            {
                await DialogHelper.ShowMessage(Resources.CurrencyRankNameDuplicate);
                return false;
            }

            InventoryModel dupeInventory = ChannelSession.Settings.Inventory.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeInventory != null)
            {
                await DialogHelper.ShowMessage(Resources.InventoryNameDuplicate);
                return false;
            }

            string siName = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name);
            if (siName.Equals("time") || siName.Equals("hours") || siName.Equals("mins") || siName.Equals("sparks") || siName.Equals("embers") || siName.Equals("fanprogression"))
            {
                await DialogHelper.ShowMessage(Resources.CurrencyRankInventoryNameProtected + " time, hours, mins, sparks, embers, fanprogression");
                return false;
            }

            if (string.IsNullOrEmpty(siName))
            {
                await DialogHelper.ShowMessage(Resources.CurrencyRankInventoryNameNotEmpty);
                return false;
            }

            if (this.MaxAmount < 0)
            {
                await DialogHelper.ShowMessage(Resources.MaxAmountMustBeZeroOrMore);
                return false;
            }

            if (this.OnlineRateAmount < 0 || this.OnlineRateInterval < 0)
            {
                await DialogHelper.ShowMessage(Resources.OnlineRateMustBeZeroOrMore);
                return false;
            }

            if (this.OnlineRateAmount > 0 && this.OnlineRateInterval == 0)
            {
                await DialogHelper.ShowMessage(Resources.OnlineRateVsInterval1);
                return false;
            }

            if (this.OfflineRateAmount < 0 || this.OfflineRateInterval < 0)
            {
                await DialogHelper.ShowMessage(Resources.OnlineRateVsInterval2);
                return false;
            }

            if (this.OfflineRateAmount > 0 && this.OfflineRateInterval == 0)
            {
                await DialogHelper.ShowMessage(Resources.OfflineRateVsInterval);
                return false;
            }

            if (this.RegularBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.RegularBonusZeroOrMore);
                return false;
            }

            if (this.SubscriberBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.SubscriberBonusZeroOrMore);
                return false;
            }

            if (this.ModeratorBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.ModeratorBonusZeroOrMore);
                return false;
            }

            if (this.OnFollowBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.OnFollowBonusZeroOrMore);
                return false;
            }

            if (this.OnHostBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.OnHostBonusZeroOrMore);
                return false;
            }

            if (this.OnSubscribeBonus < 0)
            {
                await DialogHelper.ShowMessage(Resources.OnSubscribeBonusZeroOrMore);
                return false;
            }

            if (this.IsRank)
            {
                if (this.Ranks.Count() < 1)
                {
                    await DialogHelper.ShowMessage(Resources.OneRankRequired);
                    return false;
                }
            }

            if (this.MinimumActiveRate < 0)
            {
                await DialogHelper.ShowMessage(Resources.MinimumActivityRateZeroOrMore);
                return false;
            }

            return true;
        }

        public async Task Save()
        {
            if (this.Currency == null)
            {
                this.Currency = new CurrencyModel();
                ChannelSession.Settings.Currency[this.Currency.ID] = this.Currency;
            }

            this.Currency.Name = this.Name;
            this.Currency.IsPrimary = this.IsPrimary;
            this.Currency.MaxAmount = (this.MaxAmount != 0 && this.MaxAmount != int.MaxValue) ? this.MaxAmount : int.MaxValue;

            this.Currency.AcquireAmount = this.OnlineRateAmount;
            this.Currency.AcquireInterval = this.OnlineRateInterval;
            this.Currency.OfflineAcquireAmount = this.OfflineRateAmount;
            this.Currency.OfflineAcquireInterval = this.OfflineRateInterval;

            this.Currency.SpecialTracking = CurrencySpecialTrackingEnum.None;
            if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Bits) { this.Currency.SpecialTracking = CurrencySpecialTrackingEnum.Bits; }

            this.Currency.RegularBonus = this.RegularBonus;
            this.Currency.SubscriberBonus = this.SubscriberBonus;
            this.Currency.ModeratorBonus = this.ModeratorBonus;
            this.Currency.OnFollowBonus = this.OnFollowBonus;
            this.Currency.OnHostBonus = this.OnHostBonus;
            this.Currency.OnSubscribeBonus = this.OnSubscribeBonus;

            this.Currency.MinimumActiveRate = this.MinimumActiveRate;
            this.Currency.ResetInterval = this.AutomaticResetRate;
            this.Currency.ResetStartCadence = this.AutomaticResetStartTime;

            this.Currency.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Currency.Name);

            if (this.IsRank)
            {
                this.Currency.Ranks = this.Ranks.ToList();
                this.Currency.RankChangedCommand = this.RankChangedCommand;
                this.Currency.RankDownCommand = this.RankDownCommand;
            }
            else
            {
                this.Currency.Ranks.Clear();
                this.Currency.RankChangedCommand = null;
                this.Currency.RankDownCommand = null;
            }

            await ChannelSession.SaveSettings();
        }

        public IEnumerable<NewAutoChatCommandModel> GetNewAutoChatCommands()
        {
            List<NewAutoChatCommandModel> commandsToAdd = new List<NewAutoChatCommandModel>();
            if (this.Currency != null)
            {
                ChatCommandModel statusCommand = new ChatCommandModel("User " + this.Currency.Name, new HashSet<string>() { this.Currency.SpecialIdentifier });
                statusCommand.Requirements.AddBasicRequirements();
                statusCommand.Requirements.Role.Role = UserRoleEnum.User;
                statusCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                statusCommand.Requirements.Cooldown.IndividualAmount = 5;

                string statusChatText = string.Empty;
                if (this.Currency.IsRank)
                {
                    statusChatText = string.Format("@$username is a ${0} with ${1} {2}!", this.Currency.UserRankNameSpecialIdentifier, this.Currency.UserAmountSpecialIdentifier, this.Currency.Name);
                }
                else
                {
                    statusChatText = string.Format("@$username has ${0} {1}!", this.Currency.UserAmountSpecialIdentifier, this.Currency.Name);
                }
                statusCommand.Actions.Add(new ChatActionModel(statusChatText));
                commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", statusCommand.Triggers.First(), "Shows User's Amount"), statusCommand));

                if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.None)
                {
                    ChatCommandModel addCommand = new ChatCommandModel("Add " + this.Currency.Name, new HashSet<string>() { "add" + this.Currency.SpecialIdentifier });
                    addCommand.Requirements.AddBasicRequirements();
                    addCommand.Requirements.Role.Role = UserRoleEnum.Mod;
                    addCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                    addCommand.Requirements.Cooldown.IndividualAmount = 5;

                    addCommand.Actions.Add(new ConsumablesActionModel(this.Currency, ConsumablesActionTypeEnum.AddToSpecificUser, usersMustBePresent: true, "$arg2text", username: "$targetusername"));
                    addCommand.Actions.Add(new ChatActionModel(string.Format("@$targetusername received $arg2text {0}!", this.Currency.Name)));
                    commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", addCommand.Triggers.First(), "Adds Amount To Specified User"), addCommand));

                    ChatCommandModel addAllCommand = new ChatCommandModel("Add All " + this.Currency.Name, new HashSet<string>() { "addall" + this.Currency.SpecialIdentifier });
                    addAllCommand.Requirements.AddBasicRequirements();
                    addAllCommand.Requirements.Role.Role = UserRoleEnum.Mod;
                    addAllCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                    addAllCommand.Requirements.Cooldown.IndividualAmount = 5;

                    addAllCommand.Actions.Add(new ConsumablesActionModel(this.Currency, ConsumablesActionTypeEnum.AddToAllChatUsers, usersMustBePresent: true, "$arg1text"));
                    addAllCommand.Actions.Add(new ChatActionModel(string.Format("Everyone got $arg1text {0}!", this.Currency.Name)));
                    commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", addAllCommand.Triggers.First(), "Adds Amount To All Chat Users"), addAllCommand));

                    if (!this.Currency.IsRank)
                    {
                        ChatCommandModel giveCommand = new ChatCommandModel("Give " + this.Currency.Name, new HashSet<string>() { "give" + this.Currency.SpecialIdentifier });
                        giveCommand.Requirements.AddBasicRequirements();
                        giveCommand.Requirements.Role.Role = UserRoleEnum.User;
                        giveCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                        giveCommand.Requirements.Cooldown.IndividualAmount = 5;

                        giveCommand.Actions.Add(new ConsumablesActionModel(this.Currency, ConsumablesActionTypeEnum.AddToSpecificUser, usersMustBePresent: true, "$arg2text", username: "$targetusername", deductFromUser: true));
                        giveCommand.Actions.Add(new ChatActionModel(string.Format("@$username gave @$targetusername $arg2text {0}!", this.Currency.Name)));
                        commandsToAdd.Add(new NewAutoChatCommandModel(string.Format("!{0} - {1}", giveCommand.Triggers.First(), "Gives Amount To Specified User"), giveCommand));
                    }
                }
            }
            return commandsToAdd;
        }
    }
}
