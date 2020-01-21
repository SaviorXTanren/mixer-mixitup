using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.ScorpBot
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

        public ScorpBotTimer(Dictionary<string, object> data)
        {
            this.Name = (string)data["Name2"];

            this.Text = (string)data["Response"];
            this.Text = SpecialIdentifierStringBuilder.ConvertScorpBotText(this.Text);

            this.Enabled = (((int)data["Enabled"]) == 1);
        }
    }
}
