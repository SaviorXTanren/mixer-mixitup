using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
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
                    case OverlayEndCreditsSectionV3Type.Custom:
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
                    default:
                        this.ItemTemplate = OverlayEndCreditsSectionV3Model.UsernameItemTemplate;
                        break;
                }
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

        public OverlayEndCreditsSectionV3ViewModel()
        {
            this.ID = Guid.NewGuid();
            this.SelectedType = OverlayEndCreditsSectionV3Type.Chatters;
            this.Columns = 1;
            this.HTML = OverlayEndCreditsSectionV3Model.DefaultHTML;
        }

        public OverlayEndCreditsSectionV3ViewModel(OverlayEndCreditsSectionV3Model section)
        {
            this.ID = section.ID;
            this.SelectedType = section.Type;
            this.Name = section.Name;
            this.ItemTemplate = section.ItemTemplate;
            this.Columns = section.Columns;
            this.HTML = section.HTML;
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
    }

    public class OverlayEndCreditsHeaderV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return string.Empty; } }

        public override string DefaultCSS { get { return string.Empty; } }

        public override string DefaultJavascript { get { return string.Empty; } }

        public OverlayEndCreditsHeaderV3ViewModel() : base(OverlayItemV3Type.Text) { }

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

            this.StartedCommand = this.CreateEmbeddedCommand(Resources.Started);
            this.EndedCommand = this.CreateEmbeddedCommand(Resources.Ended);

            this.Initialize();
        }

        public OverlayEndCreditsV3ViewModel(OverlayEndCreditsV3Model item)
            : base(item)
        {
            this.Header = new OverlayEndCreditsHeaderV3ViewModel(item.Header);

            switch (item.ScrollSpeed)
            {
                case FastScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Fast; break;
                case MediumScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Medium; break;
                case SlowScrollSpeed: this.SelectedScrollSpeed = OverlayEndCreditsSpeedV3TypeEnum.Slow; break;
            }
            this.BackgroundColor = item.BackgroundColor;
            this.RunCreditsWhenVisible = item.RunCreditsWhenVisible;

            this.StartedCommand = this.GetEmbeddedCommand(item.StartedCommandID, Resources.Started);
            this.EndedCommand = this.GetEmbeddedCommand(item.EndedCommandID, Resources.Ended);

            foreach (OverlayEndCreditsSectionV3Model section in item.Sections)
            {
                this.Sections.Add(new OverlayEndCreditsSectionV3ViewModel(section));
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
                        case OverlayEndCreditsSectionV3Type.Custom:
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

            await endCredits.PlayCredits();

            await base.TestWidget(widget);
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
            result.BackgroundColor = this.BackgroundColor;
            result.RunCreditsWhenVisible = this.RunCreditsWhenVisible;

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
                this.Sections.Add(new OverlayEndCreditsSectionV3ViewModel());
                this.Sections.Last().PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            });
        }
    }
}
