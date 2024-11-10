using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayProgressBarItemViewModel : OverlayHTMLTemplateItemViewModelBase
    {
        public IEnumerable<OverlayProgressBarItemTypeEnum> ProgressBarTypes { get; set; } = EnumHelper.GetEnumList<OverlayProgressBarItemTypeEnum>();
        public OverlayProgressBarItemTypeEnum ProgressBarType
        {
            get { return this.progressBarType; }
            set
            {
                this.progressBarType = value;
                if (this.progressBarType == OverlayProgressBarItemTypeEnum.Followers)
                {
                    this.TotalFollowers = true;
                }

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsNotMilestonesType");
                this.NotifyPropertyChanged("IsFollowersType");
                this.NotifyPropertyChanged("CanSetStartingAmount");
                this.NotifyPropertyChanged("SupportsRefreshUpdating");
            }
        }
        private OverlayProgressBarItemTypeEnum progressBarType;

        public bool IsFollowersType { get { return this.progressBarType == OverlayProgressBarItemTypeEnum.Followers; } }

        public bool CanSetStartingAmount { get { return !(this.IsFollowersType && this.TotalFollowers); } }

        public string StartingAmount
        {
            get { return this.startingAmount; }
            set
            {
                this.startingAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        protected string startingAmount;

        public string GoalAmount
        {
            get { return this.goalAmount; }
            set
            {
                this.goalAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        protected string goalAmount;

        public bool TotalFollowers
        {
            get { return this.totalFollowers; }
            set
            {
                this.totalFollowers = value;
                if (this.TotalFollowers)
                {
                    this.StartingAmount = "0";
                }
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CanSetStartingAmount");
            }
        }
        protected bool totalFollowers = true;

        public string ResetAfterDaysString
        {
            get { return this.resetAfterDays.ToString(); }
            set
            {
                this.resetAfterDays = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int resetAfterDays;

        public string WidthString
        {
            get { return this.width.ToString(); }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int width;

        public string HeightString
        {
            get { return this.height.ToString(); }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public string Font
        {
            get { return this.font; }
            set
            {
                this.font = value;
                this.NotifyPropertyChanged();
            }
        }
        private string font;

        public string TextColor
        {
            get { return this.textColor; }
            set
            {
                this.textColor = MixItUp.Base.Resources.ResourceManager.GetSafeString(value);
                this.NotifyPropertyChanged();
            }
        }
        private string textColor;

        public string ProgressColor
        {
            get { return this.progressColor; }
            set
            {
                this.progressColor = MixItUp.Base.Resources.ResourceManager.GetSafeString(value);
                this.NotifyPropertyChanged();
            }
        }
        private string progressColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = MixItUp.Base.Resources.ResourceManager.GetSafeString(value);
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public override bool SupportsRefreshUpdating { get { return true; } }

        public CustomCommandModel OnGoalReachedCommand
        {
            get { return this.onGoalReachedCommand; }
            set
            {
                this.onGoalReachedCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsOnGoalReachedCommandSet");
                this.NotifyPropertyChanged("IsOnGoalReachedCommandNotSet");
            }
        }
        private CustomCommandModel onGoalReachedCommand;

        public bool IsOnGoalReachedCommandSet { get { return this.OnGoalReachedCommand != null; } }
        public bool IsOnGoalReachedCommandNotSet { get { return !this.IsOnGoalReachedCommandSet; } }

        public OverlayProgressBarItemViewModel()
        {
            this.width = 400;
            this.height = 100;
            this.Font = "Arial";
            this.StartingAmount = "0";
            this.GoalAmount = "0";
            this.resetAfterDays = 0;
            this.HTML = OverlayProgressBarItemModel.HTMLTemplate;
        }

        public OverlayProgressBarItemViewModel(OverlayProgressBarItemModel item)
            : this()
        {
            this.progressBarType = item.ProgressBarType;

            if (!string.IsNullOrEmpty(item.CurrentAmountCustom))
            {
                this.StartingAmount = item.CurrentAmountCustom;
            }
            else
            {
                this.StartingAmount = item.StartAmount.ToString();
            }

            if (this.progressBarType == OverlayProgressBarItemTypeEnum.Followers && item.StartAmount == 0)
            {
                this.TotalFollowers = true;
            }
            else
            {
                this.TotalFollowers = false;
            }

            if (!string.IsNullOrEmpty(item.GoalAmountCustom))
            {
                this.GoalAmount = item.GoalAmountCustom;
            }
            else
            {
                this.GoalAmount = item.GoalAmount.ToString();
            }

            this.resetAfterDays = item.ResetAfterDays;

            this.width = item.Width;
            this.height = item.Height;
            this.Font = item.TextFont;

            //this.TextColor = ColorSchemes.GetColorName(item.TextColor);
            //this.ProgressColor = ColorSchemes.GetColorName(item.ProgressColor);
            //this.BackgroundColor = ColorSchemes.GetColorName(item.BackgroundColor);

            this.OnGoalReachedCommand = item.ProgressGoalReachedCommand;

            this.HTML = item.HTML;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.HTML) && !string.IsNullOrEmpty(this.StartingAmount) && !string.IsNullOrEmpty(this.GoalAmount) &&
                !string.IsNullOrEmpty(this.Font) && this.width > 0 && this.height > 0)
            {
                //this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                //this.ProgressColor = ColorSchemes.GetColorCode(this.ProgressColor);
                //this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                if (this.progressBarType == OverlayProgressBarItemTypeEnum.Custom)
                {
                    return new OverlayProgressBarItemModel(this.HTML, this.progressBarType, this.StartingAmount, this.GoalAmount, this.resetAfterDays, this.ProgressColor, this.BackgroundColor, this.TextColor, this.Font, this.width, this.height, this.OnGoalReachedCommand);
                }
                else
                {
                    if (double.TryParse(this.StartingAmount, out double start) && double.TryParse(this.GoalAmount, out double goal))
                    {
                        return new OverlayProgressBarItemModel(this.HTML, this.progressBarType, start, goal, this.resetAfterDays, this.ProgressColor, this.BackgroundColor, this.TextColor, this.Font, this.width, this.height, this.OnGoalReachedCommand);
                    }
                }
            }
            return null;
        }
    }
}
