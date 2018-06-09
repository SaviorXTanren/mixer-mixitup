using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Model.Store;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for MainStoreControl.xaml
    /// </summary>
    public partial class MainStoreControl : LoadingControlBase
    {
        private static StoreListingReviewModel review = new StoreListingReviewModel()
        {
            ID = Guid.NewGuid(),
            User = new UserModel() { id = 1, username = "Joe Smoe" },
            Rating = 4,
            Review = "It's not that bad honestly. Could use a bit more work here and there, but was super helpful to get me started. Thanks!"
        };

        private static StoreDetailListingModel testListing = new StoreDetailListingModel()
        {
            ID = Guid.NewGuid(),
            Name = "Really cool command!",
            Description = "This is a really cool comamnd that I designed that you are sure to love! It has all the coolest things in the world you could ever want in a command. So why have you not downloaded it already? Seriously, what is wrong with you? What were you even thinking not doing something like that, come on!",
            AverageRating = 3.8,
            TotalDownloads = 123,
            User = new UserModel() { id = 1, username = "Joe Smoe" },
            DisplayImage = "https://mixitupapp.com/img/bg-img/logo-sm.png",
            Tags = new List<string>() { "Chat", "Overlay", "Host", "Donation", "Sound" },
            Actions = new List<ActionBase>() { new ChatAction("Look at me!") },
            Reviews = new List<StoreListingReviewModel>() { review, review, review, review },
            CreatedDate = DateTimeOffset.Now,
            LastUpdatedDate = DateTimeOffset.Now
        };

        private CommandWindow window;

        private StoreDetailListingModel currentListing = null;

        public MainStoreControl(CommandWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        public Task StoreListingSelected(StoreListingModel storeListing)
        {
            this.LandingGrid.Visibility = Visibility.Collapsed;
            this.DetailsGrid.Visibility = Visibility.Visible;

            this.RateReviewButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
            this.ReportButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
            this.RemoveButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Visible : Visibility.Collapsed;

            this.DetailsGrid.DataContext = this.currentListing = (StoreDetailListingModel)storeListing;

            return Task.FromResult(0);
        }

        protected override Task OnLoaded()
        {
            this.PromotedCommandControl.Content = new LargeCommandLisingControl(this, testListing);

            List<StoreListingModel> storeListings = new List<StoreListingModel>() { testListing, testListing, testListing, testListing, testListing };

            this.CreateAndAddCategory("Chat", storeListings);
            this.CreateAndAddCategory("Donations", storeListings);
            this.CreateAndAddCategory("Overlay", storeListings);

            return base.OnLoaded();
        }

        private void CreateAndAddCategory(string categoryName, IEnumerable<StoreListingModel> storeListings)
        {
            List<StoreListingControl> storeListingControls = new List<StoreListingControl>();
            foreach (StoreListingModel storeListing in storeListings)
            {
                storeListingControls.Add(new SmallCommandListingControl(this, storeListing));
            }
            CategoryCommandListingControl category1 = new CategoryCommandListingControl(this, categoryName, storeListingControls);
            this.CategoriesStackPanel.Children.Add(category1);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            this.window.DownloadCommandFromStore(this.currentListing);
        }

        private void RateReviewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("This will remove your command from the Mix It Up store." + Environment.NewLine + "Are you sure you wish to do this?"))
                {
                    this.window.Close();
                }
            });
        }
    }
}
