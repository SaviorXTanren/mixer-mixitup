using MixItUp.Base;
using MixItUp.Base.Model.Store;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for MainStoreControl.xaml
    /// </summary>
    public partial class MainStoreControl : LoadingControlBase
    {
        private CommandWindow window;

        private StoreDetailListingModel currentListing = null;

        public MainStoreControl(CommandWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        public async Task StoreListingSelected(StoreListingModel storeListing)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                this.LandingSearchGrid.Visibility = Visibility.Collapsed;
                this.DetailsGrid.Visibility = Visibility.Visible;
                this.BackButton.Visibility = Visibility.Visible;

                this.RateReviewButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
                this.ReportButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
                this.RemoveButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Visible : Visibility.Collapsed;

                this.DetailsGrid.DataContext = this.currentListing = await ChannelSession.Services.MixItUpService.GetStoreListing(storeListing.ID);
            });
        }

        protected override async Task OnLoaded()
        {
            await this.window.RunAsyncOperation(async () =>
            {
                await this.LoadLandingPage();

                await base.OnLoaded();
            });
        }

        private void CreateAndAddCategory(string categoryName, IEnumerable<StoreListingModel> storeListings)
        {
            if (storeListings != null)
            {
                List<StoreListingControl> storeListingControls = new List<StoreListingControl>();
                foreach (StoreListingModel storeListing in storeListings)
                {
                    storeListingControls.Add(new SmallCommandListingControl(this, storeListing));
                }
                CategoryCommandListingControl category = new CategoryCommandListingControl(this, categoryName, storeListingControls);
                this.CategoriesStackPanel.Children.Add(category);
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                StoreDetailListingModel listingDetails = await ChannelSession.Services.MixItUpService.GetStoreListing(this.currentListing.ID);
                await ChannelSession.Services.MixItUpService.AddStoreListingDownload(this.currentListing);
                this.window.DownloadCommandFromStore(listingDetails);
            });
        }

        private async void RateReviewButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                ListingReviewDialogControl reviewControl = new ListingReviewDialogControl();
                string result = await MessageBoxHelper.ShowCustomDialog(reviewControl);
                if (!string.IsNullOrEmpty(result) && result.Equals("True") && reviewControl.Rating > 0 && !string.IsNullOrEmpty(reviewControl.ReviewText))
                {
                    StoreListingReviewModel review = new StoreListingReviewModel(this.currentListing, reviewControl.Rating, reviewControl.ReviewText);

                    StoreListingReviewModel existingReview = this.currentListing.Reviews.FirstOrDefault(r => r.UserID.Equals(ChannelSession.User.id));
                    if (existingReview != null)
                    {
                        review.ID = existingReview.ID;
                        await ChannelSession.Services.MixItUpService.UpdateStoreReview(review);
                    }
                    else
                    {
                        await ChannelSession.Services.MixItUpService.AddStoreReview(review);
                    }

                    await this.StoreListingSelected(this.currentListing);
                }
            });
        }

        private async void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                string report = await MessageBoxHelper.ShowTextEntryDialog("Report Reason");
                if (!string.IsNullOrEmpty(report))
                {
                    await ChannelSession.Services.MixItUpService.AddStoreListingReport(new StoreListingReportModel(this.currentListing, report));

                    await MessageBoxHelper.ShowMessageDialog("Thank you for submitting this report." + Environment.NewLine + "We will review it shortly.");
                }
            });
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("This will remove your command from the Mix It Up store." + Environment.NewLine + "Are you sure you wish to do this?"))
                {
                    await ChannelSession.Services.MixItUpService.DeleteStoreListing(this.currentListing.ID);
                    this.window.Close();
                };
            });
        }

        private void LandingSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.LandingSearchButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void LandingSearchButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (!string.IsNullOrEmpty(this.LandingSearchTextBox.Text))
                {
                    await this.PerformSearch(this.LandingSearchTextBox.Text);
                }
            });
        }

        private async Task PerformSearch(string search)
        {
            if (search.Length > 50)
            {
                await MessageBoxHelper.ShowMessageDialog("Searches must be 50 characters or less");
                return;
            }

            search = search.Substring(0, Math.Min(50, search.Length));

            this.LandingGrid.Visibility = Visibility.Collapsed;
            this.SearchGrid.Visibility = Visibility.Visible;
            this.BackButton.Visibility = Visibility.Visible;

            this.SearchStackPanel.Children.Clear();
            this.NoResultsFoundTextBlock.Visibility = Visibility.Collapsed;

            IEnumerable<StoreListingModel> listings = await ChannelSession.Services.MixItUpService.SearchStoreListings(search);
            if (listings != null && listings.Count() > 0)
            {
                foreach (StoreListingModel listing in listings)
                {
                    this.SearchStackPanel.Children.Add(new SearchCommandListingControl(this, listing));
                }
            }
            else
            {
                this.NoResultsFoundTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                await this.LoadLandingPage();
            });
        }

        private async Task LoadLandingPage()
        {
            this.LandingSearchGrid.Visibility = Visibility.Visible;
            this.LandingGrid.Visibility = Visibility.Visible;
            this.DetailsGrid.Visibility = Visibility.Collapsed;
            this.SearchGrid.Visibility = Visibility.Collapsed;
            this.BackButton.Visibility = Visibility.Collapsed;

            this.PromotedCommandControl.Content = new LargeCommandListingControl(this, await ChannelSession.Services.MixItUpService.GetTopRandomStoreListings());

            this.CreateAndAddCategory("Chat", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Chat"));
            this.CreateAndAddCategory("Donations", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Donations"));
            this.CreateAndAddCategory("Overlay", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Overlay"));
        }
    }
}
