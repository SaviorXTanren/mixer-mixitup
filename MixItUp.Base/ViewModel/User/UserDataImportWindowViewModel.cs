using ExcelDataReader;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.User
{
    public class UserDataImportColumnViewModel : UIViewModelBase
    {
        public string Name { get; private set; }

        public int? ColumnNumber
        {
            get { return this.columnNumber; }
            set
            {
                if (value.HasValue && value > 0)
                {
                    this.columnNumber = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int? columnNumber;

        public int ArrayNumber { get { return (this.ColumnNumber.HasValue) ? this.ColumnNumber.GetValueOrDefault() - 1 : -1; } }

        public UserDataImportColumnViewModel(string name) { this.Name = name; }
    }

    public class UserDataImportWindowViewModel : UIViewModelBase
    {
        public string UserDataFilePath
        {
            get { return this.userDataFilePath; }
            set
            {
                this.userDataFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string userDataFilePath;

        public IEnumerable<StreamingPlatformTypeEnum> Platforms { get { return StreamingPlatforms.SupportedPlatforms; } }

        public StreamingPlatformTypeEnum SelectedPlatform
        {
            get { return this.selectedPlatform; }
            set
            {
                this.selectedPlatform = value;
                this.NotifyPropertyChanged();
            }
        }
        private StreamingPlatformTypeEnum selectedPlatform = StreamingPlatformTypeEnum.Twitch;

        public ICommand UserDataFileBrowseCommand { get; private set; }

        public ThreadSafeObservableCollection<UserDataImportColumnViewModel> Columns { get; private set; } = new ThreadSafeObservableCollection<UserDataImportColumnViewModel>();

        private Dictionary<string, UserDataImportColumnViewModel> columnDictionary = new Dictionary<string, UserDataImportColumnViewModel>();

        public string ImportButtonText
        {
            get { return this.importButtonText; }
            set
            {
                this.importButtonText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string importButtonText = MixItUp.Base.Resources.ImportData;

        public bool ImportButtonState
        {
            get { return this.importButtonState; }
            set
            {
                this.importButtonState = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool importButtonState = true;

        public ICommand ImportButtonCommand { get; private set; }

        public UserDataImportWindowViewModel()
        {
            this.Columns.Add(new UserDataImportColumnViewModel(MixItUp.Base.Resources.PlatformID));
            this.Columns.Add(new UserDataImportColumnViewModel(MixItUp.Base.Resources.PlatformUsername));
            this.Columns.Add(new UserDataImportColumnViewModel(MixItUp.Base.Resources.ViewingHours));
            this.Columns.Add(new UserDataImportColumnViewModel(MixItUp.Base.Resources.ViewingMinutes));

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                this.Columns.Add(new UserDataImportColumnViewModel(currency.Name));
            }

            foreach (UserDataImportColumnViewModel column in this.Columns)
            {
                this.columnDictionary[column.Name] = column;
            }

            this.UserDataFileBrowseCommand = this.CreateCommand(() =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog("Valid Data File Types|*.txt;*.csv;*.xls;*.xlsx");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.UserDataFilePath = filePath;
                }
            });

            this.ImportButtonCommand = this.CreateCommand(async () =>
            {
                this.ImportButtonState = false;

                try
                {
                    int successes = 0;
                    int pending = 0;
                    int failures = 0;

                    if (string.IsNullOrEmpty(this.UserDataFilePath) || !File.Exists(this.UserDataFilePath))
                    {
                        await DialogHelper.ShowMessage(Resources.InvalidDataFile);
                        return;
                    }

                    if (this.Columns.Take(2).All(c => !c.ColumnNumber.HasValue))
                    {
                        await DialogHelper.ShowMessage(Resources.DataFileRequiredColumns);
                        return;
                    }

                    List<UserDataImportColumnViewModel> importingColumns = new List<UserDataImportColumnViewModel>();
                    foreach (UserDataImportColumnViewModel column in this.Columns.Skip(2))
                    {
                        if (column.ColumnNumber.HasValue && column.ColumnNumber <= 0)
                        {
                            await DialogHelper.ShowMessage(Resources.DataFileInvalidColumn);
                            return;
                        }
                        importingColumns.Add(column);
                    }

                    Dictionary<string, CurrencyModel> nameToCurrencies = new Dictionary<string, CurrencyModel>();
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        nameToCurrencies[currency.Name] = currency;
                    }

                    List<List<string>> lines = new List<List<string>>();

                    await ServiceManager.Get<UserService>().LoadAllUserData();

                    string extension = Path.GetExtension(this.UserDataFilePath);
                    if (extension.Equals(".txt"))
                    {
                        string fileContents = await ServiceManager.Get<IFileService>().ReadFile(this.UserDataFilePath);
                        if (!string.IsNullOrEmpty(fileContents))
                        {
                            foreach (string line in fileContents.Split(new string[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                List<string> splits = new List<string>();
                                foreach (string split in line.Split(new char[] { ' ', '\t', ',', ';' }))
                                {
                                    splits.Add(split);
                                }
                                lines.Add(splits);
                            }
                        }
                        else
                        {
                            await DialogHelper.ShowMessage(Resources.DataFileImportFailed);
                        }
                    }
                    else if (extension.Equals(".xls") || extension.Equals(".xlsx") || extension.Equals(".csv"))
                    {
                        bool isCSV = extension.Equals(".csv");
                        using (var stream = File.Open(this.UserDataFilePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = (isCSV) ? ExcelReaderFactory.CreateCsvReader(stream) : ExcelReaderFactory.CreateReader(stream))
                            {
                                var result = reader.AsDataSet();
                                if (result.Tables.Count > 0)
                                {
                                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                                    {
                                        List<string> values = new List<string>();
                                        for (int j = 0; j < result.Tables[0].Rows[i].ItemArray.Length; j++)
                                        {
                                            values.Add(result.Tables[0].Rows[i].ItemArray[j].ToString());
                                        }
                                        lines.Add(values);
                                    }
                                }
                            }
                        }
                    }

                    if (lines.Count > 0)
                    {
                        foreach (List<string> line in lines)
                        {
                            try
                            {
                                string platformID = null;
                                if (this.columnDictionary[MixItUp.Base.Resources.PlatformID].ArrayNumber >= 0)
                                {
                                    platformID = line[this.columnDictionary[MixItUp.Base.Resources.PlatformID].ArrayNumber];
                                }

                                string platformUsername = null;
                                if (this.columnDictionary[MixItUp.Base.Resources.PlatformUsername].ArrayNumber >= 0)
                                {
                                    platformUsername = line[this.columnDictionary[MixItUp.Base.Resources.PlatformUsername].ArrayNumber];
                                    if (platformUsername != null)
                                    {
                                        platformUsername = platformUsername.ToLower();
                                    }
                                }

                                UserV2ViewModel user = null;
                                UserImportModel userImport = null;

                                if (user == null && (!string.IsNullOrEmpty(platformID) || !string.IsNullOrEmpty(platformUsername)))
                                {
                                    user = await ServiceManager.Get<UserService>().GetUserByPlatform(this.SelectedPlatform, platformID: platformID, platformUsername: platformUsername);
                                }

                                int hours = 0;
                                this.GetIntValueFromLineColumn(MixItUp.Base.Resources.ViewingHours, line, out hours);

                                int minutes = 0;
                                this.GetIntValueFromLineColumn(MixItUp.Base.Resources.ViewingMinutes, line, out minutes);

                                Dictionary<Guid, int> currencies = new Dictionary<Guid, int>();
                                foreach (var kvp in nameToCurrencies)
                                {
                                    if (this.GetIntValueFromLineColumn(kvp.Key, line, out int iValue))
                                    {
                                        currencies[kvp.Value.ID] = iValue;
                                    }
                                }

                                if (user != null)
                                {
                                    if (hours > 0)
                                    {
                                        user.OnlineViewingHoursOnly = hours;
                                    }
                                    if (minutes > 0)
                                    {
                                        user.OnlineViewingMinutesOnly = minutes;
                                    }

                                    foreach (var kvp in currencies)
                                    {
                                        if (ChannelSession.Settings.Currency.ContainsKey(kvp.Key))
                                        {
                                            ChannelSession.Settings.Currency[kvp.Key].SetAmount(user, kvp.Value);
                                        }
                                    }

                                    ChannelSession.Settings.Users.ManualValueChanged(user.ID);
                                }
                                else
                                {
                                    userImport = new UserImportModel()
                                    {
                                        Platform = this.SelectedPlatform,
                                        PlatformID = platformID,
                                        PlatformUsername = platformUsername,
                                        OnlineViewingMinutes = minutes + (hours * 60),
                                        CurrencyAmounts = currencies
                                    };

                                    ChannelSession.Settings.ImportedUsers[userImport.ID] = userImport;
                                }

                                if (user != null)
                                {
                                    successes++;
                                }
                                else if (userImport != null)
                                {
                                    pending++;
                                }
                                else
                                {
                                    failures++;
                                }
                                this.ImportButtonText = string.Format("{0} {1}", successes + pending, MixItUp.Base.Resources.Imported);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Error, "User Data Import Failure - " + line);
                                Logger.Log(ex);
                                failures++;
                            }
                        }
                    }

                    await ChannelSession.SaveSettings();

                    await DialogHelper.ShowMessage(string.Format(Resources.UserImportResults, successes, pending, failures));
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await DialogHelper.ShowMessage(Resources.ImportFailed);
                }

                this.ImportButtonText = MixItUp.Base.Resources.ImportData;
                this.ImportButtonState = true;
            });
        }

        private bool GetIntValueFromLineColumn(string columnName, List<string> line, out int iValue)
        {
            iValue = 0;
            if (this.columnDictionary[columnName].ArrayNumber >= 0 && line.Count >= (this.columnDictionary[columnName].ArrayNumber + 1))
            {
                if (double.TryParse(line[this.columnDictionary[columnName].ArrayNumber], out double dValue))
                {
                    iValue = (int)dValue;
                    return true;
                }
            }
            return false;
        }
    }
}
