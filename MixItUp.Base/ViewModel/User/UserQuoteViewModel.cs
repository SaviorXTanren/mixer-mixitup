using Mixer.Base.Model.Game;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserQuoteViewModel
    {
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
            this.GameName = game.name;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

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
