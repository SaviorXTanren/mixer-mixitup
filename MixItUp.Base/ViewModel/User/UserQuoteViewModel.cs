using Mixer.Base.Model.Game;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Linq;
using System.Text;

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

        public UserQuoteViewModel(string quote, DateTimeOffset dateTime, GameTypeModel game) { this.Model = new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), quote, dateTime, game); }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.QuotesFormat))
            {
                SpecialIdentifierStringBuilder str = new SpecialIdentifierStringBuilder(ChannelSession.Settings.QuotesFormat);
                str.ReplaceSpecialIdentifier(UserQuoteModel.QuoteNumberSpecialIdentifier, this.ID.ToString());
                str.ReplaceSpecialIdentifier(UserQuoteModel.QuoteTextSpecialIdentifier, this.Quote);
                str.ReplaceSpecialIdentifier(UserQuoteModel.QuoteGameSpecialIdentifier, this.GameName);
                str.ReplaceSpecialIdentifier(UserQuoteModel.QuoteDateTimeSpecialIdentifier, this.DateTime.ToString("d"));
                return str.ToString();
            }
            else
            {
                StringBuilder result = new StringBuilder();

                result.Append("Quote #" + this.ID + ": ");
                result.Append("\"" + this.Quote + "\"");

                if (!string.IsNullOrEmpty(this.GameName))
                {
                    result.Append(string.Format(" [{0}]", this.GameName));
                }

                if (this.DateTime > DateTimeOffset.MinValue.AddYears(2))
                {
                    result.Append(string.Format(" [{0}]", this.DateTime.ToString("d")));
                }

                return result.ToString();
            }
        }
    }
}
