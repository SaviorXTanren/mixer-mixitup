using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.MainControls
{
    public class MainMenuItem : NotifyPropertyChangedBase
    {
        public string Name { get; private set; }
        public MainControlBase Control { get; private set; }
        public string Link { get; private set; }

        public Visibility LinkIconVisibility { get { return (!string.IsNullOrEmpty(this.Link)) ? Visibility.Visible : Visibility.Collapsed; } }

        public MainMenuItem(string name, MainControlBase control)
        {
            this.Name = name;
            this.Control = control;
        }

        public MainMenuItem(string name, string link)
        {
            this.Name = name;
            this.Link = link;
        }
    }

    /// <summary>
    /// Interaction logic for MainMenuControl.xaml
    /// </summary>
    public partial class MainMenuControl : MainControlBase
    {
        private ObservableCollection<MainMenuItem> menuItems = new ObservableCollection<MainMenuItem>();

        public MainMenuControl()
        {
            InitializeComponent();
        }

        public async Task AddMenuItem(string name, MainControlBase control)
        {
            await control.Initialize(this.Window);
            this.menuItems.Add(new MainMenuItem(name, control));
            if (this.menuItems.Count == 1)
            {
                this.MenuItemSelected(this.menuItems.First());
            }
        }

        public void AddMenuItem(string name, string link)
        {
            this.menuItems.Add(new MainMenuItem(name, link));
        }

        protected override Task InitializeInternal()
        {
            this.MenuItemsListBox.ItemsSource = this.menuItems;

            if (ChannelSession.Settings.DiagnosticLogging)
            {
                this.DisableDiagnosticLogsButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.EnableDiagnosticLogsButton.Visibility = Visibility.Visible;
            }

            return base.InitializeInternal();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar)
                {
                    return;
                }
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            this.MenuToggleButton.IsChecked = false;
        }

        private void MenuItemsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.MenuItemsListBox.SelectedIndex >= 0)
            {
                MainMenuItem item = (MainMenuItem)this.MenuItemsListBox.SelectedItem;
                this.MenuItemSelected(item);
            }
        }

        private void MenuItemSelected(MainMenuItem item)
        {
            if (item.Control != null)
            {
                this.DataContext = item;
                this.ActiveControlContentControl.Content = item.Control;
            }
            else if (!string.IsNullOrEmpty(item.Link))
            {
                Process.Start(item.Link);
            }
            this.MenuToggleButton.IsChecked = false;
        }

        private async void BackupSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(ChannelSession.Settings.Channel.user.username + ".mixitup");
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ChannelSession.Services.Settings.Save(ChannelSession.Settings, filePath);
                }
            });
        }

        private async void RestoreSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will overwrite your current settings and close Mix It Up. Are you sure you wish to do this?"))
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Mix It Up Settings (*.mixitup)|*.mixitup|All files (*.*)|*.*");
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((MainWindow)this.Window).RestoredSettingsFilePath = filePath;
                    this.Window.Close();
                }
            }
        }

        private void InstallationDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        private void SubmitABugButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/issues/new"); }

        private void DocumentationButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki"); }

        private async void EnableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will enable diagnostic logging for assisting with diagnosing bugs. Mix It Up will close if this is done. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = true;
                this.Window.Close();
            }
        }

        private async void DisableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will disable diagnostic logging and close Mix It Up. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = false;
                this.Window.Close();
            }
        }
    }
}
