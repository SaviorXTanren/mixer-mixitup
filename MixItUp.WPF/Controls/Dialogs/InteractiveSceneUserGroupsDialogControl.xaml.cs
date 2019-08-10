using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Dialogs
{
    public class GroupListItem : NotifyPropertyChangedBase
    {
        public InteractiveUserGroupViewModel Group { get; set; }
        public MixPlaySceneModel Scene { get; set; }

        public GroupListItem(InteractiveUserGroupViewModel group, MixPlaySceneModel scene)
        {
            this.Group = group;
            this.Scene = scene;
            this.SetAsDefault = this.Group.DefaultScene.Equals(this.Scene.sceneID);
        }

        public string GroupName { get { return this.Group.GroupName; } set { this.Group.GroupName = value; } }
        public string DefaultScene { get { return this.Group.DefaultScene; } set { this.Group.DefaultScene = value; } }

        public bool IsCustomGroup { get { return this.Group.AssociatedUserRole == MixerRoleEnum.Custom; } }

        public bool CanBeToggled { get { return !(this.Scene.sceneID.Equals(InteractiveUserGroupViewModel.DefaultName) && this.SetAsDefault); } }

        public bool SetAsDefault { get; set; }

        public void NotifyProperties()
        {
            this.NotifyPropertyChanged(nameof(GroupName));
            this.NotifyPropertyChanged(nameof(DefaultScene));
            this.NotifyPropertyChanged(nameof(IsCustomGroup));
            this.NotifyPropertyChanged(nameof(CanBeToggled));
            this.NotifyPropertyChanged(nameof(SetAsDefault));
        }
    }

    /// <summary>
    /// Interaction logic for InteractiveSceneUserGroupsDialogControl.xaml
    /// </summary>
    public partial class InteractiveSceneUserGroupsDialogControl : NotifyPropertyChangedUserControl
    {
        private MixPlayGameModel game;
        private MixPlaySceneModel scene;

        private ObservableCollection<GroupListItem> UserGroups;

        public InteractiveSceneUserGroupsDialogControl(MixPlayGameModel game, MixPlaySceneModel scene)
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
                if (!userGroup.GroupName.Equals(InteractiveUserGroupViewModel.DefaultName))
                {
                    this.UserGroups.Add(new GroupListItem(userGroup, this.scene));
                }
            }

            foreach (GroupListItem groupItem in this.UserGroups)
            {
                groupItem.NotifyProperties();
            }
        }

        private void SetAsDefaultToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton checkbox = (ToggleButton)sender;
            GroupListItem group = (GroupListItem)checkbox.DataContext;
            if (group != null)
            {
                if (checkbox.IsChecked.GetValueOrDefault())
                {
                    group.DefaultScene = this.scene.sceneID;
                    group.SetAsDefault = true;
                }
                else
                {
                    group.DefaultScene = InteractiveUserGroupViewModel.DefaultName;
                    group.SetAsDefault = false;
                }
                group.NotifyProperties();
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

                GroupListItem group = new GroupListItem(new InteractiveUserGroupViewModel(this.GroupNameTextBox.Text), this.scene);

                this.UserGroups.Add(group);
                ChannelSession.Settings.InteractiveUserGroups[this.game.id].Add(group.Group);

                this.GroupNameTextBox.Clear();
            }
        }

        private void SetAsDefaultToggleButton_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleButton checkbox = (ToggleButton)sender;
            GroupListItem group = (GroupListItem)checkbox.DataContext;
            if (group != null)
            {
                checkbox.IsChecked = group.SetAsDefault;
            }
        }
    }
}
