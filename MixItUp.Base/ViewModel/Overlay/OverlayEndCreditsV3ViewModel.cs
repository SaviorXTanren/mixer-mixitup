using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public OverlayEndCreditsSectionV3Type Type { get; set; }

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
                this.columns = value;
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

        public OverlayEndCreditsSectionV3ViewModel(OverlayEndCreditsSectionV3Type type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;
            this.Name = EnumLocalizationHelper.GetLocalizedName(type);
            this.Columns = 1;
            this.HTML = OverlayEndCreditsSectionV3Model.DefaultHTML;

            switch (type)
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

        public OverlayEndCreditsSectionV3ViewModel(OverlayEndCreditsSectionV3Model section)
        {
            this.ID = section.ID;
            this.Type = section.Type;
            this.Name = section.Name;
            this.ItemTemplate = section.ItemTemplate;
            this.Columns = section.Columns;
            this.HTML = section.HTML;
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
            this.RunCreditsWhenVisible = item.RunCreditsWhenVisible;

            this.StartedCommand = this.GetEmbeddedCommand(item.StartedCommandID, Resources.Started);
            this.EndedCommand = this.GetEmbeddedCommand(item.EndedCommandID, Resources.Ended);

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

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEndCreditsV3Model result = new OverlayEndCreditsV3Model();

            this.AssignProperties(result);

            result.Header = (OverlayEndCreditsHeaderV3Model)this.Header.GetItem();
            result.Header.Position = new OverlayPositionV3Model();

            switch (this.SelectedScrollSpeed)
            {
                case OverlayEndCreditsSpeedV3TypeEnum.Slow: result.ScrollSpeed = SlowScrollSpeed; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Medium: result.ScrollSpeed = MediumScrollSpeed; break;
                case OverlayEndCreditsSpeedV3TypeEnum.Fast: result.ScrollSpeed = FastScrollSpeed; break;
                default: result.ScrollSpeed = MediumScrollSpeed; break;
            }
            result.RunCreditsWhenVisible = this.RunCreditsWhenVisible;

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
        }
    }
}
