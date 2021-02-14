using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModels;
using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.User
{
    public class UserQuoteViewModel : UIViewModelBase
    {
        public static int GetNextQuoteNumber()
        {
            if (ChannelSession.Settings.Quotes.Count > 0)
            {
                return ChannelSession.Settings.Quotes.Max(q => q.ID) + 1;
            }
            return 1;
        }

        public int ID
        {
            get { return this.Model.ID; }
            set
            {
                this.Model.ID = value;
                this.NotifyPropertyChanged();
            }
        }

        public string Quote
        {
            get { return this.Model.Quote; }
            set
            {
                this.Model.Quote = value;
                this.NotifyPropertyChanged();
            }
        }

        public DateTimeOffset DateTime
        {
            get { return this.Model.DateTime; }
            set
            {
                this.Model.DateTime = value;
                this.NotifyPropertyChanged();
            }
        }

        public string GameName
        {
            get { return this.Model.GameName; }
            set
            {
                this.Model.GameName = value;
                this.NotifyPropertyChanged();
            }
        }

        public UserQuoteModel Model { get; private set; }

        public UserQuoteViewModel(UserQuoteModel model) { this.Model = model; }

        public UserQuoteViewModel(string quote, DateTimeOffset dateTime, string gameName = null) { this.Model = new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), quote, dateTime, gameName); }

        public override string ToString() { return this.Model.ToString(); }
    }
}
