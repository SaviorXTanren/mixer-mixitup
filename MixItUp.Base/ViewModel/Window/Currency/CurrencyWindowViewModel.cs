using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Currency
{
    public enum CurrencyAcquireRateTypeEnum
    {
        [Name("1PerMinute")]
        Minutes,
        [Name("1PerHour")]
        Hours,
        [Name("1PerSpark")]
        Sparks,
        [Name("1PerEmber")]
        Embers,
        [Name("FanProgression")]
        FanProgression,
        Custom,
        Disabled,
    }

    public class CurrencyWindowViewModel : WindowViewModelBase
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
                else if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Sparks || this.OnlineRate == CurrencyAcquireRateTypeEnum.Embers || this.OnlineRate == CurrencyAcquireRateTypeEnum.FanProgression)
                {
                    this.OnlineRateAmount = 1;
                    this.OnlineRateInterval = 1;

                    this.OfflineRate = CurrencyAcquireRateTypeEnum.Disabled;

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
        public bool IsOnlineRateTimeBased { get { return this.OnlineRate != CurrencyAcquireRateTypeEnum.Sparks && this.OnlineRate != CurrencyAcquireRateTypeEnum.Embers && this.OnlineRate != CurrencyAcquireRateTypeEnum.FanProgression; } }
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

        public CustomCommand RankChangedCommand { get; set; }
        public ObservableCollection<RankModel> Ranks { get; set; } = new ObservableCollection<RankModel>();

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

            if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.Sparks)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Sparks;
            }
            else if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.Embers)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.Embers;
            }
            else if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.FanProgression)
            {
                this.OnlineRate = CurrencyAcquireRateTypeEnum.FanProgression;
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
                foreach (RankModel rank in this.Currency.Ranks.OrderBy(r => r.Amount))
                {
                    this.Ranks.Add(rank);
                }
            }
        }

        public CurrencyWindowViewModel()
        {
            this.OnlineRate = CurrencyAcquireRateTypeEnum.Minutes;
            this.OfflineRate = CurrencyAcquireRateTypeEnum.Disabled;

            this.AutomaticResetRate = CurrencyResetRateEnum.Never;

            this.AddRankCommand = this.CreateCommand(async (parameter) =>
            {
                if (string.IsNullOrEmpty(this.NewRankName))
                {
                    await DialogHelper.ShowMessage("A rank name must be specified");
                    return;
                }

                if (this.NewRankAmount < 0)
                {
                    await DialogHelper.ShowMessage("A minimum amount must be specified");
                    return;
                }

                if (this.Ranks.Any(r => r.Name.Equals(this.NewRankName) || r.Amount == this.NewRankAmount))
                {
                    await DialogHelper.ShowMessage("Every rank must have a unique name and minimum amount");
                    return;
                }

                RankModel newRank = new RankModel(this.NewRankName, this.NewRankAmount);
                this.Ranks.Add(newRank);

                var tempRanks = this.Ranks.ToList();

                this.Ranks.Clear();
                foreach (RankModel rank in tempRanks.OrderBy(r => r.Amount))
                {
                    this.Ranks.Add(rank);
                }

                this.NewRankName = string.Empty;
                this.NewRankAmount = 0;
            });

            this.ManualResetCommand = this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format("Do you want to reset all {0} points?", this.CurrencyRankIdentifierString)))
                {
                    if (this.Currency != null)
                    {
                        await this.Currency.Reset();
                    }
                }
            });

            this.RetroactivelyGivePointsCommand = this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format("This option will reset all {0} points for this {0} & assign an amount to each user that directly equals the SAVED online rate, not the currently edited online rate. Before using this option, please save all edits to this {0}, re-edit it, then select this option." +
                    Environment.NewLine + Environment.NewLine + "EX: If the Online Rate is \"1 Per Hour\" and a user has 16 viewing hours, then that user's {0} points will be set to 16." +
                    Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?", this.CurrencyRankIdentifierString)))
                {
                    if (this.Currency != null && this.Currency.AcquireInterval > 0)
                    {
                        if (this.Currency.SpecialTracking != CurrencySpecialTrackingEnum.None)
                        {
                            await DialogHelper.ShowMessage("The rate type for this currency does not support retroactively giving points.");
                            return;
                        }

                        await this.Currency.Reset();

                        HashSet<uint> subscriberIDs = new HashSet<uint>();
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Subscriber))
                        {
                            subscriberIDs.Add(user.id);
                        }

                        HashSet<uint> modIDs = new HashSet<uint>();
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Mod))
                        {
                            modIDs.Add(user.id);
                        }
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.ChannelEditor))
                        {
                            modIDs.Add(user.id);
                        }

                        foreach (MixItUp.Base.Model.User.UserDataModel userData in ChannelSession.Settings.UserData.Values)
                        {
                            int intervalsToGive = userData.ViewingMinutes / this.Currency.AcquireInterval;
                            this.Currency.AddAmount(userData, this.Currency.AcquireAmount * intervalsToGive);
                            if (modIDs.Contains(userData.MixerID))
                            {
                                this.Currency.AddAmount(userData, this.Currency.ModeratorBonus * intervalsToGive);
                            }
                            else if (subscriberIDs.Contains(userData.MixerID))
                            {
                                this.Currency.AddAmount(userData, this.Currency.SubscriberBonus * intervalsToGive);
                            }
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                    }
                }
            });

            this.ImportFromFileCommand = this.CreateCommand(async (parameter) =>
            {
                this.userImportData.Clear();

                if (await DialogHelper.ShowConfirmation(string.Format("This will allow you to import the total amounts that each user had, assign them to this {0}, and will overwrite any amounts that each user has." +
                    Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?", this.CurrencyRankIdentifierString)))
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
                                    UserModel mixerUser = null;
                                    uint id = 0;
                                    string username = null;
                                    int amount = 0;

                                    string[] segments = line.Split(new string[] { " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
                                    if (segments.Count() == 2)
                                    {
                                        if (!int.TryParse(segments[1], out amount))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }

                                        if (!uint.TryParse(segments[0], out id))
                                        {
                                            username = segments[0];
                                        }
                                    }
                                    else if (segments.Count() == 3)
                                    {
                                        if (!uint.TryParse(segments[0], out id))
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

                                    if (amount > 0)
                                    {
                                        if (id > 0)
                                        {
                                            mixerUser = await ChannelSession.MixerUserConnection.GetUser(id);
                                        }
                                        else if (!string.IsNullOrEmpty(username))
                                        {
                                            mixerUser = await ChannelSession.MixerUserConnection.GetUser(username);
                                        }
                                    }

                                    if (mixerUser != null)
                                    {
                                        UserViewModel user = new UserViewModel(mixerUser);
                                        if (!this.userImportData.ContainsKey(user.ID))
                                        {
                                            this.userImportData[user.ID] = amount;
                                        }
                                        this.userImportData[user.ID] = Math.Max(this.userImportData[user.ID], amount);
                                        this.ImportFromFileText = string.Format("{0} {1}...", this.userImportData.Count(), MixItUp.Base.Resources.Imported);
                                    }
                                }

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

                    await DialogHelper.ShowMessage("We were unable to import the data. Please ensure your file is in one of the following formats:" +
                        Environment.NewLine + Environment.NewLine + "<USERNAME> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <USERNAME> <AMOUNT>");

                    this.ImportFromFileText = MixItUp.Base.Resources.ImportFromFile;
                }
            });

            this.ExportToFileCommand = this.CreateCommand(async (parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(this.Currency.Name + " Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    StringBuilder fileContents = new StringBuilder();
                    foreach (MixItUp.Base.Model.User.UserDataModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        fileContents.AppendLine(string.Format("{0} {1} {2}", userData.MixerID, userData.Username, this.Currency.GetAmount(userData)));
                    }
                    await ChannelSession.Services.FileService.SaveFile(filePath, fileContents.ToString());
                }
            });

            this.HelpCommand = this.CreateCommand((parameter) =>
            {
                ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency,-Rank,-&-Inventory");
                return Task.FromResult(0);
            });
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                await DialogHelper.ShowMessage(string.Format("A {0} name must be specified", this.CurrencyRankIdentifierString));
                return false;
            }

            if (this.Name.Any(c => char.IsDigit(c)))
            {
                await DialogHelper.ShowMessage("The name can not contain any number digits in it");
                return false;
            }

            CurrencyModel dupeCurrency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeCurrency != null && (this.Currency == null || !this.Currency.ID.Equals(dupeCurrency.ID)))
            {
                await DialogHelper.ShowMessage("There already exists a currency or rank system with this name");
                return false;
            }

            MixItUp.Base.Model.User.UserInventoryModel dupeInventory = ChannelSession.Settings.Inventories.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeInventory != null)
            {
                await DialogHelper.ShowMessage("There already exists an inventory with this name");
                return false;
            }

            string siName = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name);
            if (siName.Equals("time") || siName.Equals("hours") || siName.Equals("mins") || siName.Equals("sparks") || siName.Equals("embers") || siName.Equals("fanprogression"))
            {
                await DialogHelper.ShowMessage("The following names are reserved and can not be used: time, hours, mins, sparks, embers, fanprogression");
                return false;
            }

            if (string.IsNullOrEmpty(siName))
            {
                await DialogHelper.ShowMessage("The name must have at least 1 letter in it");
                return false;
            }

            if (this.MaxAmount < 0)
            {
                await DialogHelper.ShowMessage("The max amount must be greater than 0 or can be left as 0 for no max amount");
                return false;
            }

            if (this.OnlineRateAmount < 0 || this.OnlineRateInterval < 0)
            {
                await DialogHelper.ShowMessage("The online amount & minutes must be 0 or greater");
                return false;
            }

            if (this.OnlineRateAmount > 0 && this.OnlineRateInterval == 0)
            {
                await DialogHelper.ShowMessage("The online minutes can not be 0 if the online amount is greater than 0");
                return false;
            }

            if (this.OfflineRateAmount < 0 || this.OfflineRateInterval < 0)
            {
                await DialogHelper.ShowMessage("The offline amount & minutes must be 0 or greater");
                return false;
            }

            if (this.OfflineRateAmount > 0 && this.OfflineRateInterval == 0)
            {
                await DialogHelper.ShowMessage("The offline minutes can not be 0 if the offline amount is greater than 0");
                return false;
            }

            if (this.SubscriberBonus < 0)
            {
                await DialogHelper.ShowMessage("The Subscriber bonus must be 0 or greater");
                return false;
            }

            if (this.ModeratorBonus < 0)
            {
                await DialogHelper.ShowMessage("The Moderator bonus must be 0 or greater");
                return false;
            }

            if (this.OnFollowBonus < 0)
            {
                await DialogHelper.ShowMessage("The On Follow bonus must be 0 or greater");
                return false;
            }

            if (this.OnHostBonus < 0)
            {
                await DialogHelper.ShowMessage("The On Host bonus must be 0 or greater");
                return false;
            }

            if (this.OnSubscribeBonus < 0)
            {
                await DialogHelper.ShowMessage("The On Subscribe bonus must be 0 or greater");
                return false;
            }

            if (this.IsRank)
            {
                if (this.Ranks.Count() < 1)
                {
                    await DialogHelper.ShowMessage("At least one rank must be created");
                    return false;
                }
            }

            if (this.MinimumActiveRate < 0)
            {
                await DialogHelper.ShowMessage("The Minimum Activity Rate must be 0 or greater");
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
            if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Sparks) { this.Currency.SpecialTracking = CurrencySpecialTrackingEnum.Sparks; }
            else if (this.OnlineRate == CurrencyAcquireRateTypeEnum.Embers) { this.Currency.SpecialTracking = CurrencySpecialTrackingEnum.Embers; }
            else if (this.OnlineRate == CurrencyAcquireRateTypeEnum.FanProgression) { this.Currency.SpecialTracking = CurrencySpecialTrackingEnum.FanProgression; }

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
            }
            else
            {
                this.Currency.Ranks.Clear();
                this.Currency.RankChangedCommand = null;
            }

            await ChannelSession.SaveSettings();
        }

        public IEnumerable<NewAutoChatCommand> GetNewAutoChatCommands()
        {
            List<NewAutoChatCommand> commandsToAdd = new List<NewAutoChatCommand>();
            if (this.Currency != null)
            {
                ChatCommand statusCommand = new ChatCommand("User " + this.Currency.Name, this.Currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
                string statusChatText = string.Empty;
                if (this.Currency.IsRank)
                {
                    statusChatText = string.Format("@$username is a ${0} with ${1} {2}!", this.Currency.UserRankNameSpecialIdentifier, this.Currency.UserAmountSpecialIdentifier, this.Currency.Name);
                }
                else
                {
                    statusChatText = string.Format("@$username has ${0} {1}!", this.Currency.UserAmountSpecialIdentifier, this.Currency.Name);
                }
                statusCommand.Actions.Add(new ChatAction(statusChatText));
                commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", statusCommand.Commands.First(), "Shows User's Amount"), statusCommand));

                if (this.Currency.SpecialTracking == CurrencySpecialTrackingEnum.None)
                {
                    ChatCommand addCommand = new ChatCommand("Add " + this.Currency.Name, "add" + this.Currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                    addCommand.Actions.Add(new CurrencyAction(this.Currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername"));
                    addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text {0}!", this.Currency.Name)));
                    commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", addCommand.Commands.First(), "Adds Amount To Specified User"), addCommand));

                    ChatCommand addAllCommand = new ChatCommand("Add All " + this.Currency.Name, "addall" + this.Currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                    addAllCommand.Actions.Add(new CurrencyAction(this.Currency, CurrencyActionTypeEnum.AddToAllChatUsers, "$arg1text"));
                    addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text {0}!", this.Currency.Name)));
                    commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", addAllCommand.Commands.First(), "Adds Amount To All Chat Users"), addAllCommand));

                    if (!this.Currency.IsRank)
                    {
                        ChatCommand giveCommand = new ChatCommand("Give " + this.Currency.Name, "give" + this.Currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
                        giveCommand.Actions.Add(new CurrencyAction(this.Currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername", deductFromUser: true));
                        giveCommand.Actions.Add(new ChatAction(string.Format("@$username gave @$targetusername $arg2text {0}!", this.Currency.Name)));
                        commandsToAdd.Add(new NewAutoChatCommand(string.Format("!{0} - {1}", giveCommand.Commands.First(), "Gives Amount To Specified User"), giveCommand));
                    }
                }
            }
            return commandsToAdd;
        }
    }
}
