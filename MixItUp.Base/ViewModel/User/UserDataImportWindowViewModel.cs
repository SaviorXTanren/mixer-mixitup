using ExcelDataReader;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.User
{
    public class UserDataImportColumnViewModel : UIViewModelBase
    {
        public const string TwitchIDColumn = "Twitch ID";
        public const string TwitchUsernameColumn = "Twitch Username";
        public const string MixerUsernameColumn = "Mixer Username";
        public const string LiveViewingHoursColumn = "Live Viewing Time (Hours)";
        public const string LiveViewingMinutesColumn = "Live Viewing Time (Mins)";
        public const string OfflineViewingHoursColumn = "Offline Viewing Time (Hours)";
        public const string OfflineViewingMinutesColumn = "Offline Viewing Time (Mins)";

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
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.TwitchIDColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.TwitchUsernameColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.MixerUsernameColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.LiveViewingHoursColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.LiveViewingMinutesColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.OfflineViewingHoursColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.OfflineViewingMinutesColumn));

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
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Valid Data File Types|*.txt;*.csv;*.xls;*.xlsx");
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
                    int usersImported = 0;
                    int failedImports = 0;

                    if (string.IsNullOrEmpty(this.UserDataFilePath) || !File.Exists(this.UserDataFilePath))
                    {
                        await DialogHelper.ShowMessage(Resources.InvalidDataFile);
                        return;
                    }

                    if (!this.Columns[0].ColumnNumber.HasValue && !this.Columns[1].ColumnNumber.HasValue)
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

                    string extension = Path.GetExtension(this.UserDataFilePath);
                    if (extension.Equals(".txt") || extension.Equals(".csv"))
                    {
                        string fileContents = await ChannelSession.Services.FileService.ReadFile(this.UserDataFilePath);
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
                    else if (extension.Equals(".xls") || extension.Equals(".xlsx"))
                    {
                        using (var stream = File.Open(this.UserDataFilePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
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
                                long twitchID = 0;
                                if (this.columnDictionary[UserDataImportColumnViewModel.TwitchIDColumn].ArrayNumber >= 0)
                                {
                                    long.TryParse(line[this.columnDictionary[UserDataImportColumnViewModel.TwitchIDColumn].ArrayNumber], out twitchID);
                                }

                                string twitchUsername = null;
                                if (this.columnDictionary[UserDataImportColumnViewModel.TwitchUsernameColumn].ArrayNumber >= 0)
                                {
                                    twitchUsername = line[this.columnDictionary[UserDataImportColumnViewModel.TwitchUsernameColumn].ArrayNumber];
                                }

                                string mixerUsername = null;
                                if (this.columnDictionary[UserDataImportColumnViewModel.MixerUsernameColumn].ArrayNumber >= 0)
                                {
                                    mixerUsername = line[this.columnDictionary[UserDataImportColumnViewModel.MixerUsernameColumn].ArrayNumber];
                                }

                                bool newUser = true;
                                UserDataModel user = null;
                                if (twitchID > 0 && !string.IsNullOrEmpty(twitchUsername))
                                {
                                    user = await ChannelSession.Settings.GetUserDataByPlatformID(StreamingPlatformTypeEnum.Twitch, twitchID.ToString());
                                    if (user != null)
                                    {
                                        newUser = false;
                                    }
                                    else
                                    {
                                        UserViewModel userViewModel = await UserViewModel.Create(new Twitch.Base.Models.NewAPI.Users.UserModel()
                                        {
                                            id = twitchID.ToString(),
                                            login = twitchUsername,
                                            display_name = twitchUsername,
                                        });
                                        user = userViewModel.Data;
                                    }
                                }
                                else if (twitchID > 0)
                                {
                                    user = await ChannelSession.Settings.GetUserDataByPlatformID(StreamingPlatformTypeEnum.Twitch, twitchID.ToString());
                                    if (user != null)
                                    {
                                        newUser = false;
                                    }
                                    else
                                    {
                                        Twitch.Base.Models.NewAPI.Users.UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByID(twitchID.ToString());
                                        if (twitchUser != null)
                                        {
                                            UserViewModel userViewModel = await UserViewModel.Create(twitchUser);
                                            user = userViewModel.Data;
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(twitchUsername))
                                {
                                    user = await ChannelSession.Settings.GetUserDataByPlatformUsername(StreamingPlatformTypeEnum.Twitch, twitchUsername);
                                    if (user != null)
                                    {
                                        newUser = false;
                                    }
                                    else
                                    {
                                        Twitch.Base.Models.NewAPI.Users.UserModel twitchUser = await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(twitchUsername);
                                        if (twitchUser != null)
                                        {
                                            UserViewModel userViewModel = await UserViewModel.Create(twitchUser);
                                            user = userViewModel.Data;
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(mixerUsername))
                                {
                                    // TODO
//#pragma warning disable CS0612 // Type or member is obsolete
//                                    UserDataModel mixerUserData = ChannelSession.Settings.GetUserDataByUsername(StreamingPlatformTypeEnum.Mixer, mixerUsername);
//#pragma warning restore CS0612 // Type or member is obsolete
//                                    if (mixerUserData != null)
//                                    {
//                                        newUser = false;
//                                    }
//                                    else
//                                    {
//                                        user = new UserDataModel()
//                                        {
//                                            MixerID = uint.MaxValue,
//                                            MixerUsername = mixerUsername
//                                        };
//                                    }
                                }

                                if (user != null)
                                {
                                    if (newUser)
                                    {
                                        ChannelSession.Settings.SetUserData(user);
                                    }

                                    int iValue = 0;
                                    if (this.GetIntValueFromLineColumn(UserDataImportColumnViewModel.LiveViewingHoursColumn, line, out iValue))
                                    {
                                        user.ViewingHoursPart = iValue;
                                    }
                                    if (this.GetIntValueFromLineColumn(UserDataImportColumnViewModel.LiveViewingMinutesColumn, line, out iValue))
                                    {
                                        user.ViewingMinutesPart = iValue;
                                    }
                                    if (this.GetIntValueFromLineColumn(UserDataImportColumnViewModel.OfflineViewingHoursColumn, line, out iValue))
                                    {
                                        user.OfflineViewingMinutes = iValue;
                                    }
                                    if (this.GetIntValueFromLineColumn(UserDataImportColumnViewModel.OfflineViewingMinutesColumn, line, out iValue))
                                    {
                                        user.OfflineViewingMinutes = iValue;
                                    }
                                    foreach (var kvp in nameToCurrencies)
                                    {
                                        if (this.GetIntValueFromLineColumn(kvp.Key, line, out iValue))
                                        {
                                            kvp.Value.SetAmount(user, iValue);
                                        }
                                    }

                                    ChannelSession.Settings.UserData.ManualValueChanged(user.ID);

                                    usersImported++;
                                    this.ImportButtonText = string.Format("{0} {1}", usersImported, MixItUp.Base.Resources.Imported);
                                }
                                else
                                {
                                    failedImports++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Error, "User Data Import Failure - " + line);
                                Logger.Log(ex);
                                failedImports++;
                            }
                        }
                    }

                    await ChannelSession.SaveSettings();

                    if (failedImports > 0)
                    {
                        await DialogHelper.ShowMessage(string.Format(Resources.ImportWithFailures, usersImported, failedImports));
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(string.Format(Resources.ImportSuccess, usersImported));
                    }
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
                if (int.TryParse(line[this.columnDictionary[columnName].ArrayNumber], out iValue))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
