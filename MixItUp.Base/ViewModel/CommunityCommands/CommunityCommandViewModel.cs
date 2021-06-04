using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.CommunityCommands
{
    public class CommunityCommandViewModel : UIViewModelBase
    {
        private CommunityCommandModel model;

        public CommunityCommandViewModel(CommunityCommandModel model)
        {
            this.model = model;
        }

        public Guid ID { get { return this.model.ID; } }

        public string Name { get { return this.model.Name; } }

        public string Description { get { return this.model.Description; } }

        public string ImageURL { get { return this.model.ImageURL; } }

        public HashSet<string> Tags { get { return this.model.Tags; } }

        public string Username { get { return this.model.Username; } }

        public string UserAvatarURL { get { return this.model.UserAvatarURL; } }

        public double AverageRating { get { return Math.Round(this.model.AverageRating, 2); } }

        public int Downloads { get { return this.model.Downloads; } }

        public DateTimeOffset LastUpdated { get { return this.model.LastUpdated; } }

        public string LastUpdatedString { get { return this.LastUpdated.ToFriendlyDateTimeString(); } }
    }

    public class CommunityCommandDetailsViewModel : CommunityCommandViewModel
    {
        private CommunityCommandDetailsModel model;

        public CommunityCommandDetailsViewModel(CommunityCommandDetailsModel model)
            : base(model)
        {
            this.model = model;

            this.Command = this.model.GetCommand();

            foreach (CommunityCommandReviewModel review in this.model.Reviews)
            {
                this.Reviews.Add(new CommunityCommandReviewViewModel(review));
            }
        }

        public CommandModelBase Command { get; set; }

        public List<CommunityCommandReviewViewModel> Reviews { get; } = new List<CommunityCommandReviewViewModel>();
    }
}
