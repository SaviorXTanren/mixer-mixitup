using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    public class GroupListItem : NotifyPropertyChangedBase
    {
        public InteractiveUserGroupViewModel Group { get; set; }

        public GroupListItem(InteractiveUserGroupViewModel group) { this.Group = group; }

        public string GroupName { get { return this.Group.GroupName; } set { this.Group.GroupName = value; } }
        public string DefaultScene { get { return this.Group.DefaultScene; } set { this.Group.DefaultScene = value; } }
        public bool IsEnabled
        {
            get { return this.Group.IsEnabled; }
            set
            {
                this.Group.IsEnabled = value;
                this.NotifyPropertyChanged("DefaultScene");
                this.NotifyPropertyChanged("MatchesCurrentScene");
                this.NotifyPropertyChanged("IsEnabled");
                this.NotifyPropertyChanged("CanBeClicked");
            }
        }

        public bool IsCustomGroup { get { return this.Group.AssociatedUserRole == UserRole.Custom; } }

        public Visibility ShowToggleButton { get { return this.IsCustomGroup ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility ShowDeleteButton { get { return this.IsCustomGroup ? Visibility.Visible : Visibility.Collapsed; } }

        public bool MatchesCurrentScene { get; set; }
        public bool CanBeClicked { get { return this.IsEnabled && (!this.MatchesCurrentScene || !this.Group.DefaultScene.Equals(InteractiveUserGroupViewModel.DefaultName)); } }
    }

    /// <summary>
    /// Interaction logic for InteractiveSceneUserGroupsDialogControl.xaml
    /// </summary>
    public partial class InteractiveSceneUserGroupsDialogControl : NotifyPropertyChangedUserControl
    {
        private InteractiveGameModel game;
        private InteractiveSceneModel scene;

        private ObservableCollection<GroupListItem> UserGroups;

        public InteractiveSceneUserGroupsDialogControl(InteractiveGameModel game, InteractiveSceneModel scene)
        {
            this.game = game;
            this.scene = scene;

            InitializeComponent();

            this.UserGroups = new ObservableCollection<GroupListItem>();

            this.Loaded += InteractiveSceneUserGroupsDialogControl_Loaded;
        }

        private void InteractiveSceneUserGroupsDialogControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.InteractiveGroupsListView.ItemsSource = this.UserGroups;
            foreach (InteractiveUserGroupViewModel userGroup in ChannelSession.Settings.InteractiveUserGroups[this.game.id])
            {
                GroupListItem group = new GroupListItem(userGroup);
                group.MatchesCurrentScene = userGroup.DefaultScene.Equals(this.scene.sceneID);
                this.UserGroups.Add(group);
            }
        }

        private void SetDefaultForGroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            GroupListItem group = (GroupListItem)checkbox.DataContext;
            if (group != null)
            {
                if (checkbox.IsChecked.GetValueOrDefault())
                {
                    group.DefaultScene = this.scene.sceneID;
                    group.MatchesCurrentScene = true;
                    if (this.scene.sceneID.Equals(InteractiveUserGroupViewModel.DefaultName))
                    {
                        checkbox.IsEnabled = false;
                    }
                }
                else
                {
                    group.DefaultScene = InteractiveUserGroupViewModel.DefaultName;
                    group.MatchesCurrentScene = false;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            GroupListItem group = (GroupListItem)button.DataContext;
            if (group != null)
            {
                this.UserGroups.Remove(group);
                ChannelSession.Settings.InteractiveUserGroups[this.game.id].Remove(group.Group);
            }
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.GroupNameTextBox.Text))
            {
                if (this.UserGroups.Count(ug => this.GroupNameTextBox.Text.Equals(ug.GroupName)) > 0)
                {
                    return;
                }

                InteractiveUserGroupViewModel userGroup = new InteractiveUserGroupViewModel(this.GroupNameTextBox.Text);
                GroupListItem group = new GroupListItem(userGroup);

                if (group.DefaultScene.Equals(this.scene.sceneID))
                {
                    group.MatchesCurrentScene = true;
                }

                this.UserGroups.Add(group);
                ChannelSession.Settings.InteractiveUserGroups[this.game.id].Add(group.Group);

                this.GroupNameTextBox.Clear();
            }
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
