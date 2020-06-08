using ExcelDataReader;
using Mixer.Base.Model.User;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.User
{
    public class UserDataImportColumnViewModel : UIViewModelBase
    {
        public const string MixerIDColumn = "Mixer ID";
        public const string UsernameColumn = "User Name";
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

    public class UserDataImportWindowViewModel : WindowViewModelBase
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

        public ObservableCollection<UserDataImportColumnViewModel> Columns { get; private set; } = new ObservableCollection<UserDataImportColumnViewModel>();

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

        public ICommand ImportButtonCommand { get; private set; }

        public UserDataImportWindowViewModel()
        {
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.MixerIDColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.UsernameColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.LiveViewingHoursColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.LiveViewingMinutesColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.OfflineViewingHoursColumn));
            this.Columns.Add(new UserDataImportColumnViewModel(UserDataImportColumnViewModel.OfflineViewingMinutesColumn));
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                this.Columns.Add(new UserDataImportColumnViewModel(currency.Name));
            }

            this.UserDataFileBrowseCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Valid Data File Types|*.txt;*.csv;*.xls;*.xlsx");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.UserDataFilePath = filePath;
                }
                return Task.FromResult(0);
            });

            this.ImportButtonCommand = this.CreateCommand(async (parameter) =>
            {
                try
                {
                    int usersImported = 0;
                    int failedImports = 0;

                    if (string.IsNullOrEmpty(this.UserDataFilePath) || !File.Exists(this.UserDataFilePath))
                    {
                        await DialogHelper.ShowMessage("A valid data file must be specified");
                        return;
                    }

                    if (!this.Columns[0].ColumnNumber.HasValue && !this.Columns[1].ColumnNumber.HasValue)
                    {
                        await DialogHelper.ShowMessage("Your data file must include at least either" + Environment.NewLine + "the Mixer ID or Username columns.");
                        return;
                    }

                    List<UserDataImportColumnViewModel> importingColumns = new List<UserDataImportColumnViewModel>();
                    foreach (UserDataImportColumnViewModel column in this.Columns.Skip(2))
                    {
                        if (column.ColumnNumber.HasValue && column.ColumnNumber <= 0)
                        {
                            await DialogHelper.ShowMessage("A number 0 or greater must be specified for" + Environment.NewLine + "each column that you want to include.");
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
                                foreach (string split in line.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    splits.Add(split);
                                }
                                lines.Add(splits);
                            }
                        }
                        else
                        {
                            await DialogHelper.ShowMessage("We were unable to read data from the file. Please make sure it is not already opened in another program.");
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

                                uint mixerID = 0;
                                string mixerUsername = null;
                                if (line.Count >= 2)
                                {
                                    if (this.Columns[0].ArrayNumber >= 0)
                                    {
                                        if (uint.TryParse(line[this.Columns[0].ArrayNumber], out uint uValue))
                                        {
                                            mixerID = uValue;
                                        }
                                    }

                                    if (this.Columns[1].ArrayNumber >= 0)
                                    {
                                        mixerUsername = line[this.Columns[1].ArrayNumber];
                                    }
                                }

                                bool newUser = true;
                                UserDataModel user = null;
                                if (mixerID > 0)
                                {
                                    user = ChannelSession.Settings.GetUserDataByMixerID(mixerID);
                                    if (user != null)
                                    {
                                        newUser = false;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(mixerUsername))
                                        {
                                            user = new UserDataModel(mixerID, mixerUsername);
                                        }
                                        else
                                        {
                                            UserModel mixerUser = await ChannelSession.MixerUserConnection.GetUser(mixerID);
                                            if (mixerUser != null)
                                            {
                                                user = new UserDataModel(mixerUser);
                                            }
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(mixerUsername))
                                {
                                    UserModel mixerUser = await ChannelSession.MixerUserConnection.GetUser(mixerUsername);
                                    if (mixerUser != null)
                                    {
                                        user = ChannelSession.Settings.GetUserDataByMixerID(mixerUser.id);
                                        if (user == null)
                                        {
                                            user = new UserDataModel(mixerUser);
                                        }
                                        else
                                        {
                                            newUser = false;
                                        }
                                    }
                                }

                                if (user != null)
                                {
                                    if (newUser)
                                    {
                                        ChannelSession.Settings.AddUserData(user);
                                    }

                                    foreach (UserDataImportColumnViewModel column in importingColumns)
                                    {
                                        if (column.ArrayNumber >= 0 && line.Count >= column.ColumnNumber)
                                        {
                                            string value = line[column.ArrayNumber];
                                            if (int.TryParse(value, out int iValue))
                                            {
                                                if (column.Name == UserDataImportColumnViewModel.LiveViewingHoursColumn)
                                                {
                                                    user.ViewingHoursPart = iValue;
                                                }
                                                else if (column.Name == UserDataImportColumnViewModel.LiveViewingMinutesColumn)
                                                {
                                                    user.ViewingMinutesPart = iValue;
                                                }
                                                else if (column.Name == UserDataImportColumnViewModel.OfflineViewingHoursColumn)
                                                {
                                                    user.OfflineViewingMinutes = iValue / 60;
                                                }
                                                else if (column.Name == UserDataImportColumnViewModel.OfflineViewingMinutesColumn)
                                                {
                                                    user.OfflineViewingMinutes = iValue;
                                                }
                                                else
                                                {
                                                    if (nameToCurrencies.ContainsKey(column.Name))
                                                    {
                                                        nameToCurrencies[column.Name].SetAmount(user, iValue);
                                                    }
                                                }
                                            }
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
                        await DialogHelper.ShowMessage($"{usersImported} users were imported successfully, but {failedImports} users were not able to be imported. This could be due to invalid data or failure to find their information on the platform. Please contact support for further help with this if needed");
                    }
                    else
                    {
                        await DialogHelper.ShowMessage($"{usersImported} users were imported successfully");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await DialogHelper.ShowMessage("We were unable to read data from the file. Please make sure it is not already opened in another program. If this continues, please reach out to support.");
                }
                this.ImportButtonText = MixItUp.Base.Resources.ImportData;
            });
        }
    }
}
