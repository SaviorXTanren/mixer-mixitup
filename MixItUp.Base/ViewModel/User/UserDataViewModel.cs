using MixItUp.Base.ViewModel.Import;
using Newtonsoft.Json;
using System;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserDataViewModel : IEquatable<UserDataViewModel>
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public int RankPoints { get; set; }

        [DataMember]
        public int CurrencyAmount { get; set; }

        public UserDataViewModel() { }

        public UserDataViewModel(uint id, string username)
        {
            this.ID = id;
            this.UserName = username;
        }

        public UserDataViewModel(UserViewModel user) : this(user.ID, user.UserName) { }

        public UserDataViewModel(ScorpBotViewer viewer)
            : this(viewer.ID, viewer.UserName)
        {
            this.ViewingMinutes = (int)(viewer.Hours * 60.0);
            this.RankPoints = (int)viewer.RankPoints;
            this.CurrencyAmount = (int)viewer.Currency;
        }

        public UserDataViewModel(DbDataReader dataReader)
            : this(uint.Parse(dataReader["ID"].ToString()), dataReader["UserName"].ToString())
        {
            this.ViewingMinutes = int.Parse(dataReader["ViewingMinutes"].ToString());
            this.RankPoints = int.Parse(dataReader["RankPoints"].ToString());
            this.CurrencyAmount = int.Parse(dataReader["CurrencyAmount"].ToString());
        }

        [JsonIgnore]
        public string ViewingTimeString
        {
            get
            {
                int hours = this.ViewingMinutes / 60;
                int minutes = this.ViewingMinutes % 60;
                return string.Format("{0} H & {1} M", hours, minutes);
            }
        }

        [JsonIgnore]
        public string RankNameAndPoints
        {
            get
            {
                UserRankViewModel rank = null;
                if (ChannelSession.Settings.Ranks.Count > 0)
                {
                    rank = ChannelSession.Settings.Ranks.Where(r => r.MinimumPoints <= this.RankPoints).OrderByDescending(r => r.MinimumPoints).FirstOrDefault();
                }
                return string.Format("{0} - {1}", (rank != null) ? rank.Name : "No Rank", this.RankPoints);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is UserDataViewModel)
            {
                return this.Equals((UserDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserDataViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
