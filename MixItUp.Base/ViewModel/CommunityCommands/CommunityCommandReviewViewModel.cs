using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.CommunityCommands
{
    public class CommunityCommandReviewViewModel : UIViewModelBase
    {
        private CommunityCommandReviewModel model;

        public CommunityCommandReviewViewModel(CommunityCommandReviewModel model)
        {
            this.model = model;
            
            for (int i = 0; i < this.Rating; i++)
            {
                this.RatingItems.Add(true);
            }
        }

        public Guid ID { get { return this.model.ID; } }

        public Guid CommandID { get { return this.model.CommandID; } }

        public string Username { get { return this.model.Username; } }

        public string UserAvatarURL { get { return this.model.UserAvatarURL; } }

        public int Rating { get { return this.model.Rating; } }

        public string Review { get { return this.model.Review; } }

        public DateTimeOffset DateTime { get { return this.model.DateTime; } }

        public string DateTimeString { get { return this.DateTime.ToCorrectLocalTime().ToFriendlyDateTimeString(); } }

        public List<bool> RatingItems { get; set; } = new List<bool>();
    }
}
