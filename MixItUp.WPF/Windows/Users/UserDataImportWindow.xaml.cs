using ExcelDataReader;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Users
{
    public class ImportDataColumns
    {
        public string DataName { get; set; }
        public string ColumnNumber { get; set; }

        public int GetColumnNumber()
        {
            int number = -1;
            if (!string.IsNullOrEmpty(this.ColumnNumber))
            {
                int.TryParse(this.ColumnNumber, out number);
            }
            return number;
        }
    }

    /// <summary>
    /// Interaction logic for UserDataImportWindow.xaml
    /// </summary>
    public partial class UserDataImportWindow : LoadingWindowBase
    {
        private ObservableCollection<ImportDataColumns> dataColumns = new ObservableCollection<ImportDataColumns>();

        private int usersImported = 0;

        public UserDataImportWindow()
        {
            InitializeComponent();

            this.DataColumnsItemsControls.ItemsSource = dataColumns;

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.dataColumns.Add(new ImportDataColumns() { DataName = "User ID" });
            this.dataColumns.Add(new ImportDataColumns() { DataName = "User Name" });
            this.dataColumns.Add(new ImportDataColumns() { DataName = "Live Viewing Time (Hours)" });
            this.dataColumns.Add(new ImportDataColumns() { DataName = "Live Viewing Time (Mins)" });
            this.dataColumns.Add(new ImportDataColumns() { DataName = "Offline Viewing Time (Hours)" });
            this.dataColumns.Add(new ImportDataColumns() { DataName = "Offline Viewing Time (Mins)" });

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                this.dataColumns.Add(new ImportDataColumns() { DataName = currency.Name });
            }

            return Task.FromResult(0);
        }

        private void UserDataFileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Valid Data File Types|*.txt;*.csv;*.xls;*.xlsx");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.UserDataFilePathTextBox.Text = filePath;
            }
        }

        private async void ImportDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                usersImported = 0;

                if (string.IsNullOrEmpty(this.UserDataFilePathTextBox.Text) || !File.Exists(this.UserDataFilePathTextBox.Text))
                {
                    await DialogHelper.ShowMessage("A valid data file must be specified");
                    return;
                }

                if ((string.IsNullOrEmpty(this.dataColumns[0].ColumnNumber) || this.dataColumns[0].GetColumnNumber() <= 0) &&
                    (string.IsNullOrEmpty(this.dataColumns[1].ColumnNumber) || this.dataColumns[1].GetColumnNumber() <= 0))
                {
                    await DialogHelper.ShowMessage("Your data file must include at least either" + Environment.NewLine + "the User ID or User Name columns.");
                    return;
                }

                foreach (ImportDataColumns dataColumn in this.dataColumns)
                {
                    if (!string.IsNullOrEmpty(dataColumn.ColumnNumber) && dataColumn.GetColumnNumber() <= 0)
                    {
                        await DialogHelper.ShowMessage("A number 0 or greater must be specified for" + Environment.NewLine + "each column that you want to include.");
                        return;
                    }
                }

                string filepath = this.UserDataFilePathTextBox.Text;
                string extension = Path.GetExtension(filepath);
                if (extension.Equals(".txt") || extension.Equals(".csv"))
                {
                    string fileContents = await ChannelSession.Services.FileService.ReadFile(filepath);
                    if (!string.IsNullOrEmpty(fileContents))
                    {
                        await Task.Run(async () =>
                        {
                            foreach (string line in fileContents.Split(new string[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                await this.AddUserData(line.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries));
                            }
                        });
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("We were unable to read data from the file. Please make sure it is not already opened in another program.");
                    }
                }
                else if (extension.Equals(".xls") || extension.Equals(".xlsx"))
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read))
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
                                            await this.AddUserData(values);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            await this.Dispatcher.InvokeAsync(async () => { await DialogHelper.ShowMessage("We were unable to read data from the file. Please make sure it is not already opened in another program."); });
                        }
                    });
                }
            });

            this.ImportDataButton.Content = "Import Data";
        }

        private async Task AddUserData(IEnumerable<string> dataValues)
        {
            int currentColumn = 1;
            UserDataModel importedUserData = new UserDataModel();
            foreach (string dataValue in dataValues)
            {
                bool columnMatched = false;
                foreach (ImportDataColumns dataColumn in this.dataColumns)
                {
                    if (dataColumn.GetColumnNumber() == currentColumn)
                    {
                        switch (dataColumn.DataName)
                        {
                            case "User ID":
                                if (uint.TryParse(dataValue, out uint id))
                                {
                                    importedUserData.MixerID = id;
                                }
                                columnMatched = true;
                                break;
                            case "User Name":
                                importedUserData.MixerUsername = dataValue;
                                columnMatched = true;
                                break;
                            case "Live Viewing Time (Hours)":
                                if (int.TryParse(dataValue, out int liveHours))
                                {
                                    importedUserData.ViewingMinutes = liveHours * 60;
                                }
                                columnMatched = true;
                                break;
                            case "Live Viewing Time (Mins)":
                                if (int.TryParse(dataValue, out int liveMins))
                                {
                                    importedUserData.ViewingMinutes = liveMins;
                                }
                                columnMatched = true;
                                break;
                            case "Offline Viewing Time (Hours)":
                                if (int.TryParse(dataValue, out int offlineHours))
                                {
                                    importedUserData.OfflineViewingMinutes = offlineHours * 60;
                                }
                                columnMatched = true;
                                break;
                            case "Offline Viewing Time (Mins)":
                                if (int.TryParse(dataValue, out int offlineMins))
                                {
                                    importedUserData.OfflineViewingMinutes = offlineMins;
                                }
                                columnMatched = true;
                                break;
                            default:
                                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                                {
                                    if (currency.Name.Equals(dataColumn.DataName))
                                    {
                                        if (int.TryParse(dataValue, out int currencyAmount))
                                        {
                                            currency.SetAmount(importedUserData, currencyAmount);
                                        }
                                        columnMatched = true;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    if (columnMatched)
                    {
                        break;
                    }
                }
                currentColumn++;
            }

            if (importedUserData.MixerID == 0)
            {
                UserModel user = await ChannelSession.MixerUserConnection.GetUser(importedUserData.Username);
                if (user != null)
                {
                    importedUserData.MixerID = user.id;
                }
            }
            else if (string.IsNullOrEmpty(importedUserData.Username))
            {
                UserModel user = await ChannelSession.MixerUserConnection.GetUser(importedUserData.MixerID);
                if (user != null)
                {
                    importedUserData.MixerUsername = user.username;
                }
            }

            if (importedUserData.MixerID > 0 && !string.IsNullOrEmpty(importedUserData.Username))
            {
                UserViewModel user = new UserViewModel(new UserModel() { id = importedUserData.MixerID, username = importedUserData.Username });
                UserDataModel userData = user.Data;

                usersImported++;
                this.Dispatcher.Invoke(() => { this.ImportDataButton.Content = string.Format("Imported {0} Users", usersImported); });
            }
        }
    }
}
