using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserQuoteModel
    {
        public const string QuoteNumberSpecialIdentifier = "quotenumber";
        public const string QuoteTextSpecialIdentifier = "quotetext";
        public const string QuoteGameSpecialIdentifier = "quotegame";
        public const string QuoteDateTimeSpecialIdentifier = "quotedatetime";

        public static event EventHandler<UserQuoteModel> OnQuoteAdded = delegate { };
        public static void QuoteAdded(UserQuoteModel quote) { OnQuoteAdded(null, quote); }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Quote { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public string GameName { get; set; }

        public UserQuoteModel()
        {
            this.DateTime = DateTimeOffset.MinValue;
        }

        public UserQuoteModel(int id, string quote, DateTimeOffset dateTime, string gameName)
        {
            this.ID = id;
            this.Quote = quote;
            this.DateTime = dateTime;
            this.GameName = gameName;
        }

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

                result.Append(MixItUp.Base.Resources.Quote + " #" + this.ID + ": ");
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
