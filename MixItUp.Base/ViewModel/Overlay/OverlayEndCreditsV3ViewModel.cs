using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayEndCreditsSpeedV3TypeEnum
    {
        Slow,
        Medium,
        Fast
    }

    public class OverlayEndCreditsSectionV3ViewModel : UIViewModelBase
    {
        public Guid ID { get; set; }

        public IEnumerable<OverlayEndCreditsSectionV3Type> Types { get; set; } = EnumHelper.GetEnumList<OverlayEndCreditsSectionV3Type>();

        public OverlayEndCreditsSectionV3Type SelectedType
        {
            get { return this.selectedType; }
            set
            {
                this.selectedType = value;
                this.NotifyPropertyChanged();

                switch (this.SelectedType)
                {
                    case OverlayEndCreditsSectionV3Type.CustomSection:
                        this.ItemTemplate = OverlayEndCreditsSectionV3Model.TextItemTemplate;
                        break;
                    case OverlayEndCreditsSectionV3Type.Raids:
                    case OverlayEndCreditsSectionV3Type.Resubscribers:
                    case OverlayEndCreditsSectionV3Type.GiftedSubscriptions:
                    case OverlayEndCreditsSectionV3Type.TwitchBits:
                    case OverlayEndCreditsSectionV3Type.TrovoSpells:
                    case OverlayEndCreditsSectionV3Type.YouTubeSuperChats:
                    case OverlayEndCreditsSectionV3Type.Donations:
                        this.ItemTemplate = OverlayEndCreditsSectionV3Model.UsernameAmountItemTemplate;
                        break;
                    case OverlayEndCreditsSectionV3Type.HTML:
                        this.ItemTemplate = string.Empty;
                        break;
                    default:
                        this.ItemTemplate = OverlayEndCreditsSectionV3Model.UsernameItemTemplate;
                        break;
                }

                if (this.SelectedType == OverlayEndCreditsSectionV3Type.HTML)
                {
                    this.Columns = 1;
                }
                this.NotifyPropertyChanged(nameof(this.IsColumnsEditable));
            }
        }
        private OverlayEndCreditsSectionV3Type selectedType;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public string ItemTemplate
        {
            get { return this.itemTemplate; }
            set
            {
                this.itemTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemTemplate;

        public int Columns
        {
            get { return this.columns; }
            set
            {
                this.columns = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int columns;

        public bool IsColumnsEditable { get { return this.SelectedType != OverlayEndCreditsSectionV3Type.HTML; } }

        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
            }
        }
        private string html;

        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private OverlayEndCreditsV3ViewModel endCredits;

        public OverlayEndCreditsSectionV3ViewModel(OverlayEndCreditsV3ViewModel endCredits)
        {
            this.endCredits = endCredits;

            this.ID = Guid.NewGuid();
            this.SelectedType = OverlayEndCreditsSectionV3Type.Chatters;
            this.Columns = 1;
            this.HTML = OverlayEndCreditsSectionV3Model.DefaultHTML;

            this.Initialize();
        }

        public OverlayEndCreditsSectionV3ViewModel(OverlayEndCreditsV3ViewModel endCredits, OverlayEndCreditsSectionV3Model section)
        {
            this.endCredits = endCredits;

            this.ID = section.ID;
            this.SelectedType = section.Type;
            this.Name = section.Name;
            this.ItemTemplate = section.ItemTemplate;
            this.Columns = section.Columns;
            this.HTML = section.HTML;

            this.Initialize();
        }

        public OverlayEndCreditsSectionV3Model GetModel()
        {
            return new OverlayEndCreditsSectionV3Model()
            {
                ID = this.ID,
                Type = this.SelectedType,
                Name = this.Name,
                ItemTemplate = this.ItemTemplate,
                Columns = this.Columns,
                HTML = this.HTML
            };
        }

        private void Initialize()
        {
            this.MoveUpCommand = this.CreateCommand(() =>
            {
                this.endCredits.MoveSectionUp(this);
            });

            this.MoveDownCommand = this.CreateCommand(() =>
            {
                this.endCredits.MoveSectionDown(this);
            });

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.endCredits.DeleteSection(this);
            });
        }
    }

    public class OverlayEndCreditsHeaderV3ViewModel : OverlayHeaderV3ViewModelBase
    {
        public OverlayEndCreditsHeaderV3ViewModel() { }

        public OverlayEndCreditsHeaderV3ViewModel(OverlayEndCreditsHeaderV3Model model) : base(model) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEndCreditsHeaderV3Model result = new OverlayEndCreditsHeaderV3Model();
            this.AssignProperties(result);
            return result;
        }
    }

    public class OverlayEndCreditsV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public const int FastScrollSpeed = 10;
        public const int MediumScrollSpeed = 25;
        public const int SlowScrollSpeed = 50;

        public const double FastScrollRate = 3;
        public const double MediumScrollRate = 8;
        public const double SlowScrollRate = 13;

        public static async Task LoadTestData(OverlayEndCreditsV3Model endCredits)
        {
            List<UserV2ViewModel> users = new List<UserV2ViewModel>();
            foreach (UserV2Model user in await ServiceManager.Get<UserService>().LoadQuantityOfUserData(20))
            {
                users.Add(new UserV2ViewModel(user));
            }

            foreach (OverlayEndCreditsSectionV3Model section in endCredits.Sections)
            {
                section.ClearTracking();
                foreach (UserV2ViewModel user in users)
                {
                    switch (section.Type)
                    {
                        case OverlayEndCreditsSectionV3Type.CustomSection:
                            section.Track(user, Resources.Text);
                            break;
                        case OverlayEndCreditsSectionV3Type.Raids:
                        case OverlayEndCreditsSectionV3Type.Resubscribers:
                        case OverlayEndCreditsSectionV3Type.GiftedSubscriptions:
                        case OverlayEndCreditsSectionV3Type.TwitchBits:
                        case OverlayEndCreditsSectionV3Type.TrovoSpells:
                        case OverlayEndCreditsSectionV3Type.YouTubeSuperChats:
                        case OverlayEndCreditsSectionV3Type.Donations:
                            section.Track(user, RandomHelper.GenerateRandomNumber(1, 100));
                            break;
                        default:
                            section.Track(user);
                            break;
                    }
                }
            }
        }

        public override string DefaultHTML { get { return OverlayEndCreditsV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayEndCreditsV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayEndCreditsV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public OverlayEndCreditsHeaderV3ViewModel Header
        {
            get { return this.header; }
            set
            {
                this.header = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndCreditsHeaderV3ViewModel header;

        public IEnumerable<OverlayEndCreditsSpeedV3TypeEnum> ScrollSpeeds { get; set; } = EnumHelper.GetEnumList<OverlayEndCreditsSpeedV3TypeEnum>();

        public OverlayEndCreditsSpeedV3TypeEnum SelectedScrollSpeed
        {
            get { return this.selectedScrollSpeed; }
            set
            {
                this.selectedScrollSpeed = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndCreditsSpeedV3TypeEnum selectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Medium;

        public bool RunCreditsWhenVisible
        {
            get { return this.runCreditsWhenVisible; }
            set
            {
                this.runCreditsWhenVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool runCreditsWhenVisible;

        public bool RunEndlessly
        {
            get { return this.runEndlessly; }
            set
            {
                this.runEndlessly = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool runEndlessly;

        public bool DontShowNoDataError
        {
            get { return this.dontShowNoDataError; }
            set
            {
                this.dontShowNoDataError = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool dontShowNoDataError;

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

        public ObservableCollection<OverlayEndCreditsSectionV3ViewModel> Sections { get; set; } = new ObservableCollection<OverlayEndCreditsSectionV3ViewModel>();

        public CustomCommandModel StartedCommand
        {
            get { return this.startedCommand; }
            set
            {
                this.startedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel startedCommand;

        public CustomCommandModel EndedCommand
        {
            get { return this.endedCommand; }
            set
            {
                this.endedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel endedCommand;

        public ICommand AddSectionCommand { get; set; }

        public OverlayEndCreditsV3ViewModel()
            : base(OverlayItemV3Type.EndCredits)
        {
            this.Header = new OverlayEndCreditsHeaderV3ViewModel();

            this.Header.FontSize = 42;
            this.Header.FontColor = "White";
            this.Header.Bold = true;
            this.Header.Underline = true;
            this.Header.LeftAlignment = false;
            this.Header.CenterAlignment = true;

            this.FontColor = "White";
            this.LeftAlignment = false;
            this.CenterAlignment = true;

            this.BackgroundColor = "Black";

            this.StartedCommand = this.CreateEmbeddedCommand(Resources.Started);
            this.EndedCommand = this.CreateEmbeddedCommand(Resources.Ended);

            this.Initialize();
        }

        public OverlayEndCreditsV3ViewModel(OverlayEndCreditsV3Model item)
            : base(item)
        {
            this.Header = new OverlayEndCreditsHeaderV3ViewModel(item.Header);

            switch (item.ScrollRate)
            {
                case FastScrollRate: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Fast; break;
                case MediumScrollRate: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Medium; break;
                case SlowScrollRate: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Slow; break;
                case 0.0:
                    switch (item.ScrollSpeed)
                    {
                        case FastScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Fast; break;
                        case MediumScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Medium; break;
                        case SlowScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Slow; break;
                    }
                    break;
            }
            this.BackgroundColor = item.BackgroundColor;
            this.RunCreditsWhenVisible = item.RunCreditsWhenVisible;
            this.RunEndlessly = item.RunEndlessly;
            this.DontShowNoDataError = item.DontShowNoDataError;

            this.StartedCommand = this.GetEmbeddedCommand(item.StartedCommandID, Resources.Started);
            this.EndedCommand = this.GetEmbeddedCommand(item.EndedCommandID, Resources.Ended);

            foreach (OverlayEndCreditsSectionV3Model section in item.Sections)
            {
                this.Sections.Add(new OverlayEndCreditsSectionV3ViewModel(this, section));
                this.Sections.Last().PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            }

            this.Initialize();
        }

        public override Result Validate()
        {
            Result result = this.Header.Validate();
            if (!result.Success)
            {
                return result;
            }

            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayEndCreditsV3Model endCredits = (OverlayEndCreditsV3Model)widget.Item;

            await OverlayEndCreditsV3ViewModel.LoadTestData(endCredits);

            await endCredits.PlayCredits();

            foreach (OverlayEndCreditsSectionV3Model section in endCredits.Sections)
            {
                section.ClearTracking();
            }

            await base.TestWidget(widget);
        }

        public void MoveSectionUp(OverlayEndCreditsSectionV3ViewModel section)
        {
            int index = this.Sections.IndexOf(section);
            if (index > 0)
            {
                this.Sections.Remove(section);
                this.Sections.Insert(index - 1, section);
            }
        }

        public void MoveSectionDown(OverlayEndCreditsSectionV3ViewModel section)
        {
            int index = this.Sections.IndexOf(section);
            if (index < this.Sections.Count - 1)
            {
                this.Sections.Remove(section);
                this.Sections.Insert(index + 1, section);
            }
        }

        public void DeleteSection(OverlayEndCreditsSectionV3ViewModel section)
        {
            this.Sections.Remove(section);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEndCreditsV3Model result = new OverlayEndCreditsV3Model();

            this.AssignProperties(result);

            result.Header = (OverlayEndCreditsHeaderV3Model)this.Header.GetItem();

            switch (this.SelectedScrollSpeed)
            {
                case OverlayEndCreditsSpeedV3TypeEnum.Slow: result.ScrollSpeed = SlowScrollSpeed; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Medium: result.ScrollSpeed = MediumScrollSpeed; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Fast: result.ScrollSpeed = FastScrollSpeed; break;
                default: result.ScrollSpeed = MediumScrollSpeed; break;
            }

            switch (this.SelectedScrollSpeed)
            {
                case OverlayEndCreditsSpeedV3TypeEnum.Slow: result.ScrollRate = SlowScrollRate; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Medium: result.ScrollRate = MediumScrollRate; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Fast: result.ScrollRate = FastScrollRate; break;
                default: result.ScrollRate = MediumScrollRate; break;
            }

            result.BackgroundColor = this.BackgroundColor;
            result.RunCreditsWhenVisible = this.RunCreditsWhenVisible;
            result.RunEndlessly = this.RunEndlessly;
            result.DontShowNoDataError = this.DontShowNoDataError;

            foreach (OverlayEndCreditsSectionV3ViewModel section in this.Sections)
            {
                result.Sections.Add(section.GetModel());
            }

            result.StartedCommandID = this.StartedCommand.ID;
            ChannelSession.Settings.SetCommand(this.StartedCommand);

            result.EndedCommandID = this.EndedCommand.ID;
            ChannelSession.Settings.SetCommand(this.EndedCommand);

            return result;
        }

        private void Initialize()
        {
            this.Header.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };

            this.AddSectionCommand = this.CreateCommand(() =>
            {
                this.Sections.Add(new OverlayEndCreditsSectionV3ViewModel(this));
                this.Sections.Last().PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            });
        }
    }
}
