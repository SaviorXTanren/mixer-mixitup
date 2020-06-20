using Mixer.Base.Model.Game;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserQuoteModel
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

        public UserQuoteModel()
        {
            this.DateTime = DateTimeOffset.MinValue;
        }

        public UserQuoteModel(int id, string quote, DateTimeOffset dateTime, GameTypeModel game)
        {
            this.ID = id;
            this.Quote = quote;
            this.DateTime = dateTime;
            if (game != null)
            {
                this.GameName = game.name;
            }
        }
    }
}
