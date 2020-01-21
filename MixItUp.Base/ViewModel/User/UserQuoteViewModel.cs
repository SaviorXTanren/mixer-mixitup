using Mixer.Base.Model.Game;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserQuoteViewModel
    {
        public const string QuoteNumberSpecialIdentifier = "quotenumber";
        public const string QuoteTextSpecialIdentifier = "quotetext";
        public const string QuoteGameSpecialIdentifier = "quotegame";
        public const string QuoteDateTimeSpecialIdentifier = "quotedatetime";

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Quote { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public string GameName { get; set; }

        public UserQuoteViewModel()
        {
            this.DateTime = DateTimeOffset.MinValue;
        }

        public UserQuoteViewModel(string quote)
            : this()
        {
            this.Quote = quote;
        }

        public UserQuoteViewModel(string quote, DateTimeOffset dateTime, GameTypeModel game)
            : this(quote)
        {
            this.DateTime = dateTime;
            if (game != null)
            {
                this.GameName = game.name;
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.QuotesFormat))
            {
                SpecialIdentifierStringBuilder str = new SpecialIdentifierStringBuilder(ChannelSession.Settings.QuotesFormat);
                str.ReplaceSpecialIdentifier(QuoteNumberSpecialIdentifier, this.ID.ToString());
                str.ReplaceSpecialIdentifier(QuoteTextSpecialIdentifier, this.Quote);
                str.ReplaceSpecialIdentifier(QuoteGameSpecialIdentifier, this.GameName);
                str.ReplaceSpecialIdentifier(QuoteDateTimeSpecialIdentifier, this.DateTime.ToString("d"));
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
