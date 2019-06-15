using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayProgressBarItemViewModel : OverlayCustomHTMLItemViewModelBase
    {
        public IEnumerable<string> ProgressBarTypeStrings { get; set; } = EnumHelper.GetEnumNames<ProgressBarTypeEnum>();
        public string ProgressBarTypeString
        {
            get { return EnumHelper.GetEnumName(this.progressBarType); }
            set
            {
                this.progressBarType = EnumHelper.GetEnumValueFromString<ProgressBarTypeEnum>(value);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsNotMilestonesType");
                this.NotifyPropertyChanged("IsFollowersType");
                if (this.progressBarType == ProgressBarTypeEnum.Followers)
                {
                    this.TotalFollowers = true;
                }
            }
        }
        private ProgressBarTypeEnum progressBarType;

        public bool IsNotMilestonesType { get { return this.progressBarType != ProgressBarTypeEnum.Milestones; } }
        public bool IsFollowersType { get { return this.progressBarType == ProgressBarTypeEnum.Followers; } }

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
                this.NotifyPropertyChanged();
                if (this.TotalFollowers)
                {
                    this.StartingAmount = "0";
                }
            }
        }
        protected bool totalFollowers;

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
                this.textColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textColor;

        public string ProgressColor
        {
            get { return this.progressColor; }
            set
            {
                this.progressColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string progressColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public CustomCommand OnGoalReachedCommand
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
        private CustomCommand onGoalReachedCommand;

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
            this.HTML = OverlayProgressBar.HTMLTemplate;
        }

        public OverlayProgressBarItemViewModel(OverlayProgressBar item)
            : this()
        {
            this.progressBarType = item.ProgressBarType;

            if (!string.IsNullOrEmpty(item.CurrentAmountCustom))
            {
                this.StartingAmount = item.CurrentAmountCustom;
            }
            else
            {
                this.StartingAmount = item.CurrentAmountNumber.ToString();
            }

            if (this.progressBarType == ProgressBarTypeEnum.Followers && item.CurrentAmountNumber < 0)
            {
                this.TotalFollowers = true;
                this.StartingAmount = "0";
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
                this.GoalAmount = item.GoalAmountNumber.ToString();
            }

            this.resetAfterDays = item.ResetAfterDays;

            this.width = item.Width;
            this.height = item.Height;
            this.Font = item.TextFont;

            this.TextColor = item.TextFont;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.TextColor))
            {
                this.TextColor = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.TextColor)).Key;
            }

            this.ProgressColor = item.ProgressColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.ProgressColor))
            {
                this.ProgressColor = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.ProgressColor)).Key;
            }

            this.BackgroundColor = item.BackgroundColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.BackgroundColor))
            {
                this.BackgroundColor = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.BackgroundColor)).Key;
            }

            this.HTML = item.HTMLText;

            this.OnGoalReachedCommand = item.GoalReachedCommand;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.HTML) && !string.IsNullOrEmpty(this.StartingAmount) && !string.IsNullOrEmpty(this.GoalAmount) &&
                !string.IsNullOrEmpty(this.Font) && this.width > 0 && this.height > 0)
            {
                string startingAmount = "0";
                string goalAmount = "0";
                int resetAfterDays = 0;

                if (this.progressBarType != ProgressBarTypeEnum.Milestones)
                {
                    if (this.progressBarType == ProgressBarTypeEnum.Followers && this.TotalFollowers)
                    {
                        startingAmount = "-1";
                    }
                    else
                    {
                        startingAmount = this.StartingAmount;
                    }
                    goalAmount = this.GoalAmount;
                }

                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.TextColor))
                {
                    this.TextColor = ColorSchemes.HTMLColorSchemeDictionary[this.TextColor];
                }
                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.ProgressColor))
                {
                    this.ProgressColor = ColorSchemes.HTMLColorSchemeDictionary[this.ProgressColor];
                }
                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.BackgroundColor))
                {
                    this.BackgroundColor = ColorSchemes.HTMLColorSchemeDictionary[this.BackgroundColor];
                }

                if (double.TryParse(startingAmount, out double startingAmountNumber) && double.TryParse(goalAmount, out double goalAmountNumber))
                {
                    return new OverlayProgressBar(this.HTML, this.progressBarType, startingAmountNumber, goalAmountNumber, resetAfterDays, this.ProgressColor, this.BackgroundColor, this.TextColor, this.Font, this.width, this.height, this.OnGoalReachedCommand);
                }
                else
                {
                    return new OverlayProgressBar(this.HTML, this.progressBarType, startingAmount, goalAmount, resetAfterDays, this.ProgressColor, this.BackgroundColor, this.TextColor, this.Font, this.width, this.height, this.OnGoalReachedCommand);
                }
            }
            return null;
        }
    }
}
