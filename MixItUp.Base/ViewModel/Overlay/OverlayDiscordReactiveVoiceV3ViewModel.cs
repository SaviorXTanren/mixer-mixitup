using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayDiscordReactiveVoiceUserV3ViewModel : UIViewModelBase
    {
        public string UserID
        {
            get { return this.userID; }
            set
            {
                this.userID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string userID;

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;
        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                this.displayName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayName;
        public string ServerDisplayName
        {
            get { return this.serverDisplayName; }
            set
            {
                this.serverDisplayName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string serverDisplayName;

        public ObservableCollection<OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel> CustomVisuals { get; set; } = new ObservableCollection<OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel>();

        private OverlayDiscordReactiveVoiceV3ViewModel viewModel;

        public OverlayDiscordReactiveVoiceUserV3ViewModel(OverlayDiscordReactiveVoiceV3ViewModel viewModel, DiscordServerUser user)
        {
            this.viewModel = viewModel;

            this.UserID = user.User.ID;
            this.Username = user.User.UserName;
            this.DisplayName = user.User.GlobalName;
            this.ServerDisplayName = user.Nickname;
        }

        public OverlayDiscordReactiveVoiceUserV3ViewModel(OverlayDiscordReactiveVoiceV3ViewModel viewModel, OverlayDiscordReactiveVoiceUserV3Model user)
        {
            this.viewModel = viewModel;

            this.UserID = user.UserID;
            this.Username = user.Username;
            this.DisplayName = user.DisplayName;
            this.ServerDisplayName = user.ServerDisplayName;
        }

        public Result Validate()
        {
            foreach (OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel customVisual in this.CustomVisuals)
            {
                Result result = customVisual.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result();
        }

        public OverlayDiscordReactiveVoiceUserV3Model GetModel()
        {
            OverlayDiscordReactiveVoiceUserV3Model result = new OverlayDiscordReactiveVoiceUserV3Model()
            {
                UserID = this.UserID,
                Username = this.Username,
                DisplayName = this.DisplayName,
                ServerDisplayName = this.ServerDisplayName,
            };

            foreach (OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel customVisual in this.CustomVisuals)
            {
                result.CustomVisuals.Add(customVisual.GetModel());
            }

            return result;
        }
    }

    public class OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name = Resources.Default;

        public string CustomActiveImageFilePath
        {
            get { return this.customActiveImageFilePath; }
            set
            {
                this.customActiveImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customActiveImageFilePath;
        public string CustomInactiveImageFilePath
        {
            get { return this.customInactiveImageFilePath; }
            set
            {
                this.customInactiveImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customInactiveImageFilePath;
        public string CustomMutedImageFilePath
        {
            get { return this.customMutedImageFilePath; }
            set
            {
                this.customMutedImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customMutedImageFilePath;
        public string CustomDeafenImageFilePath
        {
            get { return this.customDeafenImageFilePath; }
            set
            {
                this.customDeafenImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customDeafenImageFilePath;

        public int CustomWidth
        {
            get { return this.customWidth; }
            set
            {
                this.customWidth = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int customWidth;
        public int CustomHeight
        {
            get { return this.customHeight; }
            set
            {
                this.customHeight = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int customHeight;

        public OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel() { }

        public OverlayDiscordReactiveVoiceUserCustomVisualV3ViewModel(OverlayDiscordReactiveVoiceUserCustomVisualV3Model visual)
        {
            this.CustomActiveImageFilePath = visual.CustomActiveImageFilePath;
            this.CustomInactiveImageFilePath = visual.CustomInactiveImageFilePath;
            this.CustomMutedImageFilePath = visual.CustomMutedImageFilePath;
            this.CustomDeafenImageFilePath = visual.CustomDeafenImageFilePath;
            this.CustomWidth = visual.CustomWidth;
            this.CustomHeight = visual.CustomHeight;
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.NameRequired);
            }

            if (string.IsNullOrEmpty(this.CustomActiveImageFilePath))
            {
                return new Result(Resources.OverlayDiscordReactiveVoiceActiveImageMustBeSet);
            }

            return new Result();
        }

        public OverlayDiscordReactiveVoiceUserCustomVisualV3Model GetModel()
        {
            return new OverlayDiscordReactiveVoiceUserCustomVisualV3Model()
            {
                Name = this.Name,

                CustomActiveImageFilePath = this.CustomActiveImageFilePath,
                CustomInactiveImageFilePath = this.CustomInactiveImageFilePath,
                CustomMutedImageFilePath = this.CustomMutedImageFilePath,
                CustomDeafenImageFilePath = this.CustomDeafenImageFilePath,

                CustomWidth = this.CustomWidth,
                CustomHeight = this.CustomHeight,
            };
        }
    }

    public class OverlayDiscordReactiveVoiceV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayDiscordReactiveVoiceV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayDiscordReactiveVoiceV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayDiscordReactiveVoiceV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public int UserWidth
        {
            get { return this.userWidth; }
            set
            {
                this.userWidth = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int userWidth;
        public int UserHeight
        {
            get { return this.userHeight; }
            set
            {
                this.userHeight = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int userHeight;
        public int UserSpacing
        {
            get { return this.userSpacing; }
            set
            {
                this.userSpacing = value;
                this.NotifyPropertyChanged();
            }
        }
        private int userSpacing;

        public bool DimInactiveUsers
        {
            get { return this.dimInactiveUsers; }
            set
            {
                this.dimInactiveUsers = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool dimInactiveUsers;

        public IEnumerable<OverlayDiscordReactiveVoiceNameDisplayTypeEnum> NameDisplays { get; set; } = EnumHelper.GetEnumList<OverlayDiscordReactiveVoiceNameDisplayTypeEnum>();
        public OverlayDiscordReactiveVoiceNameDisplayTypeEnum SelectedNameDisplay
        {
            get { return this.selectedNameDisplay; }
            set
            {
                this.selectedNameDisplay = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayDiscordReactiveVoiceNameDisplayTypeEnum selectedNameDisplay = OverlayDiscordReactiveVoiceNameDisplayTypeEnum.DisplayName;

        public ObservableCollection<OverlayDiscordReactiveVoiceUserV3ViewModel> Users { get; set; } = new ObservableCollection<OverlayDiscordReactiveVoiceUserV3ViewModel>();

        public OverlayAnimationV3ViewModel ActiveAnimation;
        public OverlayAnimationV3ViewModel InactiveAnimation;
        public OverlayAnimationV3ViewModel MutedAnimation;
        public OverlayAnimationV3ViewModel UnmutedAnimation;
        public OverlayAnimationV3ViewModel DeafenAnimation;
        public OverlayAnimationV3ViewModel UndeafenAnimation;
        public OverlayAnimationV3ViewModel JoinedAnimation;
        public OverlayAnimationV3ViewModel LeftAnimation;

        public ICommand BrowseFilePathCommand { get; set; }

        public ICommand AddOutcomeCommand { get; set; }

        public OverlayDiscordReactiveVoiceV3ViewModel()
            : base(OverlayItemV3Type.DiscordReactiveVoice)
        {
            this.FontSize = 40;
            this.FontColor = "Black";

            this.UserWidth = 100;
            this.UserHeight = 100;
            this.UserSpacing = 50;

            this.DimInactiveUsers = true;
            this.SelectedNameDisplay = OverlayDiscordReactiveVoiceNameDisplayTypeEnum.DisplayName;

            this.ActiveAnimation = new OverlayAnimationV3ViewModel(Resources.Active, new OverlayAnimationV3Model());
            this.InactiveAnimation = new OverlayAnimationV3ViewModel(Resources.Inactive, new OverlayAnimationV3Model());
            this.MutedAnimation = new OverlayAnimationV3ViewModel(Resources.Muted, new OverlayAnimationV3Model());
            this.UnmutedAnimation = new OverlayAnimationV3ViewModel(Resources.Unmuted, new OverlayAnimationV3Model());
            this.DeafenAnimation = new OverlayAnimationV3ViewModel(Resources.Deafen, new OverlayAnimationV3Model());
            this.UndeafenAnimation = new OverlayAnimationV3ViewModel(Resources.Undeafened, new OverlayAnimationV3Model());
            this.JoinedAnimation = new OverlayAnimationV3ViewModel(Resources.Joined, new OverlayAnimationV3Model());
            this.LeftAnimation = new OverlayAnimationV3ViewModel(Resources.Left, new OverlayAnimationV3Model());
        }

        public OverlayDiscordReactiveVoiceV3ViewModel(OverlayDiscordReactiveVoiceV3Model item)
            : base(item)
        {
            this.UserWidth = item.UserWidth;
            this.UserHeight = item.UserHeight;
            this.UserSpacing = item.UserSpacing;

            this.DimInactiveUsers = item.DimInactiveUsers;
            this.SelectedNameDisplay = item.NameDisplay;

            this.ActiveAnimation = new OverlayAnimationV3ViewModel(Resources.Active, item.ActiveAnimation);
            this.InactiveAnimation = new OverlayAnimationV3ViewModel(Resources.Inactive, item.InactiveAnimation);
            this.MutedAnimation = new OverlayAnimationV3ViewModel(Resources.Muted, item.MutedAnimation);
            this.UnmutedAnimation = new OverlayAnimationV3ViewModel(Resources.Unmuted, item.UnmutedAnimation);
            this.DeafenAnimation = new OverlayAnimationV3ViewModel(Resources.Deafen, item.DeafenAnimation);
            this.UndeafenAnimation = new OverlayAnimationV3ViewModel(Resources.Undeafened, item.UndeafenAnimation);
            this.JoinedAnimation = new OverlayAnimationV3ViewModel(Resources.Joined, item.JoinedAnimation);
            this.LeftAnimation = new OverlayAnimationV3ViewModel(Resources.Left, item.LeftAnimation);

            foreach (var kvp in item.Users)
            {
                this.AddUser(new OverlayDiscordReactiveVoiceUserV3ViewModel(this, kvp.Value));
            }

            this.Initialize();
        }

        public override Result Validate()
        {
            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            await base.TestWidget(widget);

            OverlayDiscordReactiveVoiceV3Model discord = (OverlayDiscordReactiveVoiceV3Model)widget.Item;

            //await wheel.Spin(new CommandParametersModel());
        }

        public void DeleteUser(OverlayDiscordReactiveVoiceUserV3ViewModel user)
        {
            this.Users.Remove(user);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayDiscordReactiveVoiceV3Model result = new OverlayDiscordReactiveVoiceV3Model();

            this.AssignProperties(result);

            result.UserWidth = this.UserWidth;
            result.UserHeight = this.UserHeight;
            result.UserSpacing = this.UserSpacing;

            result.DimInactiveUsers = this.DimInactiveUsers;
            result.NameDisplay = this.SelectedNameDisplay;

            result.ActiveAnimation = this.ActiveAnimation.GetAnimation();
            result.InactiveAnimation = this.InactiveAnimation.GetAnimation();
            result.MutedAnimation = this.MutedAnimation.GetAnimation();
            result.UnmutedAnimation = this.UnmutedAnimation.GetAnimation();
            result.DeafenAnimation = this.DeafenAnimation.GetAnimation();
            result.UndeafenAnimation = this.UndeafenAnimation.GetAnimation();
            result.JoinedAnimation = this.JoinedAnimation.GetAnimation();
            result.LeftAnimation = this.LeftAnimation.GetAnimation();

            return result;
        }

        private void Initialize()
        {
            this.Animations.Add(this.ActiveAnimation);
            this.Animations.Add(this.InactiveAnimation);
            this.Animations.Add(this.MutedAnimation);
            this.Animations.Add(this.UnmutedAnimation);
            this.Animations.Add(this.DeafenAnimation);
            this.Animations.Add(this.UndeafenAnimation);
            this.Animations.Add(this.JoinedAnimation);
            this.Animations.Add(this.LeftAnimation);
        }

        private void AddUser(OverlayDiscordReactiveVoiceUserV3ViewModel viewModel)
        {
            this.Users.Add(viewModel);
            viewModel.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };
        }
    }
}
