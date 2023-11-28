using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
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

        public ThreadSafeObservableCollection<UserQuoteViewModel> Quotes { get; private set; } = new ThreadSafeObservableCollection<UserQuoteViewModel>();

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
            this.AddQuoteCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.AddQuoteText))
                {
                    ChannelSession.Settings.Quotes.Add(new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), this.AddQuoteText, DateTimeOffset.Now, await GamePreMadeChatCommandModel.GetCurrentGameName(ChannelSession.Settings.DefaultStreamingPlatform)));
                    this.Refresh();

                    this.AddQuoteText = string.Empty;
                }
            });

            this.ExportQuotesCommand = this.CreateCommand(async () =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowSaveFileDialog("Quotes.txt", MixItUp.Base.Resources.TextFileFormatFilter);
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
            this.Quotes.ClearAndAddRange(ChannelSession.Settings.Quotes.ToList().OrderBy(q => q.ID).Select(q => new UserQuoteViewModel(q)));
        }

        public async Task RemoveQuote(UserQuoteViewModel quote)
        {
            if (await DialogHelper.ShowConfirmation(Resources.DeleteQuotePrompt))
            {
                ChannelSession.Settings.Quotes.Remove(quote.Model);
                this.Refresh();
            }
        }

        protected override Task OnOpenInternal()
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
