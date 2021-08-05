using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public string ScreenshotURL { get { return this.model.ScreenshotURL; } }

        public HashSet<CommunityCommandTagEnum> Tags { get { return this.model.Tags; } }

        public Guid UserID { get { return this.model.UserId; } }

        public string Username { get { return this.model.Username; } }

        public string UserAvatarURL { get { return this.model.UserAvatarURL; } }

        public double AverageRating { get { return Math.Round(this.model.AverageRating, 2); } }

        public int Downloads { get { return this.model.Downloads; } }

        public DateTimeOffset LastUpdated { get { return this.model.LastUpdated; } }

        public string LastUpdatedString { get { return this.LastUpdated.ToCorrectLocalTime().ToFriendlyDateTimeString(); } }

        public string TagsDisplayString { get { return MixItUp.Base.Resources.TagsHeader + " " + string.Join(", ", this.Tags.Select(t => EnumLocalizationHelper.GetLocalizedName(t))); } }

        public string DownloadsString
        {
            get
            {
                if (this.Downloads > 1000000)
                {
                    return string.Format(MixItUp.Base.Resources.CommunityCommandsDownloadsMillionFormat, (int)(this.Downloads / 1000000));
                }
                else if (this.Downloads > 1000)
                {
                    return string.Format(MixItUp.Base.Resources.CommunityCommandsDownloadsThousandsFormat, (int)(this.Downloads / 1000));
                }
                else
                {
                    return this.Downloads.ToString();
                }
            }
        }

        public string WebsiteURL { get { return $"https://mixitupapp.com/community/command/{this.ID}"; } }
        public string DownloadURL { get { return $"mixitup://community/command/{this.ID}"; } }
    }

    public class CommunityCommandDetailsViewModel : CommunityCommandViewModel
    {
        private CommunityCommandDetailsModel model;

        public CommunityCommandDetailsViewModel(CommunityCommandDetailsModel model)
            : base(model)
        {
            this.model = model;

            this.Commands = this.model.GetCommands();

            foreach (CommunityCommandReviewModel review in this.model.Reviews.OrderByDescending(r => r.DateTime))
            {
                if (!string.IsNullOrWhiteSpace(review.Review))
                {
                    this.Reviews.Add(new CommunityCommandReviewViewModel(review));
                }
            }
            this.NotifyPropertyChanged("HasNoReviews");
        }

        public virtual bool IsMyCommand { get { return false; } }

        public bool IsPublicCommand { get { return !this.IsMyCommand; } }

        public List<CommandModelBase> Commands { get; set; } = new List<CommandModelBase>();

        public List<CommunityCommandReviewViewModel> Reviews { get; } = new List<CommunityCommandReviewViewModel>();

        public CommandModelBase PrimaryCommand { get { return this.Commands.FirstOrDefault(); } }

        public bool HasNoReviews { get { return this.Reviews.Count == 0; } }
    }

    public class MyCommunityCommandDetailsViewModel : CommunityCommandDetailsViewModel
    {
        private CommunityCommandDetailsModel model;

        public MyCommunityCommandDetailsViewModel(CommunityCommandDetailsModel model)
            : base(model)
        {
            this.model = model;
        }

        public override bool IsMyCommand { get { return true; } }
    }
}
