using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayEndCreditsSectionItemViewModel : UIViewModelBase
    {
        public OverlayEndCreditsSectionTypeEnum SectionType { get; set; }

        public string SectionHTML { get; set; }

        public string UserHTML { get; set; }

        public ICommand DeleteItemCommand { get; set; }

        private OverlayEndCreditsItemViewModel container;

        public OverlayEndCreditsSectionItemViewModel(OverlayEndCreditsItemViewModel container, OverlayEndCreditsSectionTypeEnum sectionType)
            : this(container)
        {
            this.SectionType = sectionType;
            this.SectionHTML = OverlayEndCreditsItemModel.SectionHTMLTemplate;
            if (sectionType == OverlayEndCreditsSectionTypeEnum.Viewers || sectionType == OverlayEndCreditsSectionTypeEnum.NewFollowers ||
                sectionType == OverlayEndCreditsSectionTypeEnum.Hosts || sectionType == OverlayEndCreditsSectionTypeEnum.NewSubscribers ||
                sectionType == OverlayEndCreditsSectionTypeEnum.Subscribers || sectionType == OverlayEndCreditsSectionTypeEnum.Moderators)
            {
                this.UserHTML = OverlayEndCreditsItemModel.UserHTMLTemplate;
            }
            else
            {
                this.UserHTML = OverlayEndCreditsItemModel.UserDetailsHTMLTemplate;
            }
        }

        public OverlayEndCreditsSectionItemViewModel(OverlayEndCreditsItemViewModel container, OverlayEndCreditsSectionModel section)
            : this(container)
        {
            this.SectionType = section.SectionType;
            this.SectionHTML = section.SectionHTML;
            this.UserHTML = section.UserHTML;
        }

        private OverlayEndCreditsSectionItemViewModel(OverlayEndCreditsItemViewModel container)
        {
            this.container = container;
            this.DeleteItemCommand = this.CreateCommand((parameter) =>
            {
                this.container.DeleteItem(this);
                return Task.FromResult(0);
            });
        }

        public string SectionTypeName { get { return EnumHelper.GetEnumName(this.SectionType); } }

        public OverlayEndCreditsSectionModel GetItem()
        {
            return new OverlayEndCreditsSectionModel()
            {
                SectionType = this.SectionType,
                SectionHTML = this.SectionHTML,
                UserHTML = this.UserHTML
            };
        }
    }

    public class OverlayEndCreditsItemViewModel : OverlayHTMLTemplateItemViewModelBase
    {
        public IEnumerable<string> ItemTypeStrings { get; set; } = EnumHelper.GetEnumNames<OverlayEndCreditsSectionTypeEnum>().OrderBy(s => s);
        public string ItemTypeString
        {
            get { return EnumHelper.GetEnumName(this.itemType); }
            set
            {
                this.itemType = EnumHelper.GetEnumValueFromString<OverlayEndCreditsSectionTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndCreditsSectionTypeEnum itemType;

        public IEnumerable<string> SpeedStrings { get; set; } = EnumHelper.GetEnumNames<OverlayEndCreditsSpeedEnum>();
        public string SpeedString
        {
            get { return EnumHelper.GetEnumName(this.speed); }
            set
            {
                this.speed = EnumHelper.GetEnumValueFromString<OverlayEndCreditsSpeedEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndCreditsSpeedEnum speed;

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

        public string SectionTextFont
        {
            get { return this.sectionTextFont; }
            set
            {
                this.sectionTextFont = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sectionTextFont;

        public string SectionTextColor
        {
            get { return this.sectionTextColor; }
            set
            {
                this.sectionTextColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sectionTextColor;

        public string SectionTextSizeString
        {
            get { return this.sectionTextSize.ToString(); }
            set
            {
                this.sectionTextSize = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int sectionTextSize;

        public string ItemTextFont
        {
            get { return this.itemTextFont; }
            set
            {
                this.itemTextFont = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemTextFont;

        public string ItemTextColor
        {
            get { return this.itemTextColor; }
            set
            {
                this.itemTextColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemTextColor;

        public string ItemTextSizeString
        {
            get { return this.itemTextSize.ToString(); }
            set
            {
                this.itemTextSize = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int itemTextSize;

        public ObservableCollection<OverlayEndCreditsSectionItemViewModel> SectionItems { get; set; } = new ObservableCollection<OverlayEndCreditsSectionItemViewModel>();

        public ICommand AddItemCommand { get; set; }

        public OverlayEndCreditsItemViewModel()
        {
            this.speed = OverlayEndCreditsSpeedEnum.Medium;
            this.BackgroundColor = "Black";
            this.SectionTextFont = "Arial";
            this.SectionTextColor = "White";
            this.sectionTextSize = 48;
            this.ItemTextFont = "Arial";
            this.ItemTextColor = "White";
            this.itemTextSize = 24;

            this.HTML = OverlayEndCreditsItemModel.TitleHTMLTemplate;

            this.AddItemCommand = this.CreateCommand((parameter) =>
            {
                this.SectionItems.Add(new OverlayEndCreditsSectionItemViewModel(this, this.itemType));
                this.ItemTypeString = string.Empty;
                return Task.FromResult(0);
            });
        }

        public OverlayEndCreditsItemViewModel(OverlayEndCreditsItemModel item)
            : this()
        {
            this.speed = item.Speed;
            this.BackgroundColor = ColorSchemes.GetColorName(item.BackgroundColor);
            this.SectionTextFont = item.SectionTextFont;
            this.SectionTextColor = ColorSchemes.GetColorName(item.SectionTextColor);
            this.sectionTextSize = item.SectionTextSize;
            this.ItemTextFont = item.ItemTextFont;
            this.ItemTextColor = ColorSchemes.GetColorName(item.ItemTextColor);
            this.itemTextSize = item.ItemTextSize;

            this.HTML = item.TitleTemplate;

            foreach (var kvp in item.SectionTemplates)
            {
                this.SectionItems.Add(new OverlayEndCreditsSectionItemViewModel(this, kvp.Value));
            }
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.SectionTextFont) && !string.IsNullOrEmpty(this.SectionTextColor) && this.sectionTextSize > 0 &&
                !string.IsNullOrEmpty(this.ItemTextFont) && !string.IsNullOrEmpty(this.ItemTextColor) && this.itemTextSize > 0 && !string.IsNullOrEmpty(this.HTML) && this.SectionItems.Count > 0)
            {
                this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);
                this.SectionTextColor = ColorSchemes.GetColorCode(this.SectionTextColor);
                this.ItemTextColor = ColorSchemes.GetColorCode(this.ItemTextColor);

                Dictionary<OverlayEndCreditsSectionTypeEnum, OverlayEndCreditsSectionModel> sections = new Dictionary<OverlayEndCreditsSectionTypeEnum, OverlayEndCreditsSectionModel>();
                foreach (OverlayEndCreditsSectionItemViewModel section in this.SectionItems)
                {
                    sections[section.SectionType] = section.GetItem();
                }

                return new OverlayEndCreditsItemModel(this.HTML, sections, this.BackgroundColor, this.SectionTextColor, this.SectionTextFont, this.sectionTextSize,
                    this.ItemTextColor, this.ItemTextFont, this.itemTextSize, this.speed);
            }
            return null;
        }

        public void DeleteItem(OverlayEndCreditsSectionItemViewModel item)
        {
            this.SectionItems.Remove(item);
        }
    }
}
