using MixItUp.Base.Util;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class ScorpBotTimer
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public ScorpBotTimer() { }

        public ScorpBotTimer(DbDataReader reader)
        {
            this.Name = (string)reader["Name2"];

            this.Text = (string)reader["Response"];
            this.Text = SpecialIdentifierStringBuilder.ConvertScorpBotText(this.Text);

            this.Enabled = (((int)reader["Enabled"]) == 1);
        }
    }
}
