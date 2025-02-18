using ExcelDataReader;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Quotes
{
    public class QuotesDataImportColumnViewModel : UIViewModelBase
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

        public QuotesDataImportColumnViewModel(string name) { this.Name = name; }
    }

    public class QuotesDataImportWindowViewModel : UIViewModelBase
    {
        public string QuotesDataFilePath
        {
            get { return this.quotesDataFilePath; }
            set
            {
                this.quotesDataFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string quotesDataFilePath;

        public ICommand QuotesDataFileBrowseCommand { get; private set; }

        public ThreadSafeObservableCollection<QuotesDataImportColumnViewModel> Columns { get; private set; } = new ThreadSafeObservableCollection<QuotesDataImportColumnViewModel>();

        private Dictionary<string, QuotesDataImportColumnViewModel> columnDictionary = new Dictionary<string, QuotesDataImportColumnViewModel>();

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

        public QuotesDataImportWindowViewModel()
        {
            this.Columns.Add(new QuotesDataImportColumnViewModel(MixItUp.Base.Resources.QuoteID));
            this.Columns.Add(new QuotesDataImportColumnViewModel(MixItUp.Base.Resources.Quote));
            this.Columns.Add(new QuotesDataImportColumnViewModel(MixItUp.Base.Resources.DateTime));
            this.Columns.Add(new QuotesDataImportColumnViewModel(MixItUp.Base.Resources.GameName));

            foreach (QuotesDataImportColumnViewModel column in this.Columns)
            {
                this.columnDictionary[column.Name] = column;
            }

            this.QuotesDataFileBrowseCommand = this.CreateCommand(() =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog("Valid Data File Types|*.txt;*.csv;*.xls;*.xlsx");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.QuotesDataFilePath = filePath;
                }
            });

            this.ImportButtonCommand = this.CreateCommand(async () =>
            {
                this.ImportButtonState = false;

                try
                {
                    int successes = 0;
                    int failures = 0;

                    if (string.IsNullOrEmpty(this.QuotesDataFilePath) || !File.Exists(this.QuotesDataFilePath))
                    {
                        await DialogHelper.ShowMessage(Resources.InvalidDataFile);
                        return;
                    }

                    if (!this.Columns.ElementAt(1).ColumnNumber.HasValue)
                    {
                        await DialogHelper.ShowMessage(Resources.QuotesDataFileRequiredColumns);
                        return;
                    }

                    List<QuotesDataImportColumnViewModel> importingColumns = new List<QuotesDataImportColumnViewModel>();
                    foreach (QuotesDataImportColumnViewModel column in this.Columns)
                    {
                        if (column.ColumnNumber.HasValue && column.ColumnNumber <= 0)
                        {
                            await DialogHelper.ShowMessage(Resources.DataFileInvalidColumn);
                            return;
                        }
                        importingColumns.Add(column);
                    }

                    List<List<string>> lines = new List<List<string>>();

                    string extension = Path.GetExtension(this.QuotesDataFilePath);
                    if (extension.Equals(".txt"))
                    {
                        string fileContents = await ServiceManager.Get<IFileService>().ReadFile(this.QuotesDataFilePath);
                        if (!string.IsNullOrEmpty(fileContents))
                        {
                            char separator = ',';
                            if (fileContents.Contains('\t'))
                            {
                                separator = '\t';
                            }    

                            foreach (string line in fileContents.Split(new string[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                List<string> splits = new List<string>();
                                foreach (string split in line.Split(new char[] { separator }))
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
                        using (var stream = File.Open(this.QuotesDataFilePath, FileMode.Open, FileAccess.Read))
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

                    int newQuoteID = 1;
                    if (ChannelSession.Settings.Quotes.Count > 0)
                    {
                        newQuoteID = ChannelSession.Settings.Quotes.Max(q => q.ID) + 1;
                    }

                    if (lines.Count > 0)
                    {
                        foreach (List<string> line in lines)
                        {
                            try
                            {
                                string quoteText = null;
                                if (this.columnDictionary[MixItUp.Base.Resources.Quote].ArrayNumber >= 0)
                                {
                                    quoteText = line[this.columnDictionary[MixItUp.Base.Resources.Quote].ArrayNumber];
                                }

                                int quoteID = -1;
                                if (!this.GetIntValueFromLineColumn(MixItUp.Base.Resources.QuoteID, line, out quoteID))
                                {
                                    if (this.columnDictionary[MixItUp.Base.Resources.QuoteID].ColumnNumber.HasValue)
                                    {
                                        Logger.Log(LogLevel.Error, "User Data Import Failure - " + line);
                                        failures++;
                                        continue;
                                    }
                                }

                                DateTimeOffset dateTime = DateTimeOffset.MinValue;
                                if (this.columnDictionary[MixItUp.Base.Resources.DateTime].ArrayNumber >= 0)
                                {
                                    dateTime = DateTimeOffsetExtensions.FromGeneralString(line[this.columnDictionary[MixItUp.Base.Resources.DateTime].ArrayNumber]);
                                }

                                string gameName = null;
                                if (this.columnDictionary[MixItUp.Base.Resources.GameName].ArrayNumber >= 0)
                                {
                                    gameName = line[this.columnDictionary[MixItUp.Base.Resources.GameName].ArrayNumber];
                                }

                                if (!string.IsNullOrEmpty(quoteText))
                                {
                                    UserQuoteModel quote = new UserQuoteModel(quoteID, quoteText, dateTime, gameName);
                                    if (quoteID <= 0 || ChannelSession.Settings.Quotes.Any(q => q.ID == quoteID))
                                    {
                                        quote.ID = newQuoteID;
                                        newQuoteID++;
                                    }
                                    ChannelSession.Settings.Quotes.Add(quote);

                                    successes++;
                                }
                                else
                                {
                                    failures++;
                                }
                                this.ImportButtonText = string.Format("{0} {1}", successes, MixItUp.Base.Resources.Imported);
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

                    await DialogHelper.ShowMessage(string.Format(Resources.QuotesImportResults, successes, failures));
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