using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class QuotesMainControlViewModel : WindowControlViewModelBase
    {
        public bool QuotesEnabled
        {
            get { return ChannelSession.Settings.QuotesEnabled; }
            set
            {
                ChannelSession.Settings.QuotesEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        public string AddQuoteText
        {
            get { return this.addQuoteText; }
            set
            {
                this.addQuoteText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string addQuoteText;

        public ObservableCollection<UserQuoteViewModel> Quotes { get; private set; } = new ObservableCollection<UserQuoteViewModel>();

        public string QuotesFormatText
        {
            get { return ChannelSession.Settings.QuotesFormat; }
            set
            {
                ChannelSession.Settings.QuotesFormat = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand AddQuoteCommand { get; set; }

        public ICommand ExportQuotesCommand { get; set; }

        public QuotesMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.AddQuoteCommand = this.CreateCommand((parameter) =>
            {
                if (!string.IsNullOrEmpty(this.AddQuoteText))
                {
                    ChannelSession.Settings.Quotes.Add(new UserQuoteViewModel(this.AddQuoteText, DateTimeOffset.Now, ChannelSession.MixerChannel.type));
                    this.Refresh();

                    this.AddQuoteText = string.Empty;
                }
                return Task.FromResult(0);
            });

            this.ExportQuotesCommand = this.CreateCommand(async (parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog("Quotes.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    List<List<string>> contents = new List<List<string>>();

                    contents.Add(new List<string>() { "#", MixItUp.Base.Resources.Quote, MixItUp.Base.Resources.Game, MixItUp.Base.Resources.DateTime });

                    foreach (UserQuoteViewModel quote in this.Quotes.ToList())
                    {
                        List<string> data = new List<string>();
                        data.Add(quote.ID.ToString());
                        data.Add(quote.Quote);
                        data.Add(quote.GameName);
                        data.Add(quote.DateTime.ToFriendlyDateTimeString());
                        contents.Add(data);
                    }

                    await SpreadsheetFileHelper.ExportToCSV(filePath, contents);
                }
            });
        }

        public void Refresh()
        {
            this.Quotes.Clear();
            foreach (UserQuoteViewModel quote in ChannelSession.Settings.Quotes.ToList().OrderBy(q => q.ID))
            {
                this.Quotes.Add(quote);
            }
        }

        public async Task RemoveQuote(UserQuoteViewModel quote)
        {
            if (await DialogHelper.ShowConfirmation("Are you sure you want to delete this quote?"))
            {
                ChannelSession.Settings.Quotes.Remove(quote);
                this.Refresh();
            }
        }

        protected override Task OnLoadedInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }
    }
}
