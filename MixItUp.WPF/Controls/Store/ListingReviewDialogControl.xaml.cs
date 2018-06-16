using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for ListingReviewDialogControl.xaml
    /// </summary>
    public partial class ListingReviewDialogControl : UserControl
    {
        public ListingReviewDialogControl()
        {
            InitializeComponent();
        }

        public int Rating
        {
            get
            {
                int rating = 0;
                if (this.Star1Icon.Kind == PackIconKind.Star) { rating++; }
                if (this.Star2Icon.Kind == PackIconKind.Star) { rating++; }
                if (this.Star3Icon.Kind == PackIconKind.Star) { rating++; }
                if (this.Star4Icon.Kind == PackIconKind.Star) { rating++; }
                if (this.Star5Icon.Kind == PackIconKind.Star) { rating++; }
                return rating;
            }
        }

        public string ReviewText { get { return this.ReviewTextBox.Text; } }

        private void Star1Button_Click(object sender, System.Windows.RoutedEventArgs e) { this.SetRatingStars(1); }

        private void Star2Button_Click(object sender, System.Windows.RoutedEventArgs e) { this.SetRatingStars(2); }

        private void Star3Button_Click(object sender, System.Windows.RoutedEventArgs e) { this.SetRatingStars(3); }

        private void Star4Button_Click(object sender, System.Windows.RoutedEventArgs e) { this.SetRatingStars(4); }

        private void Star5Button_Click(object sender, System.Windows.RoutedEventArgs e) { this.SetRatingStars(5); }

        private void ReviewTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.OkButton.IsEnabled = (!string.IsNullOrEmpty(this.ReviewTextBox.Text));
        }

        private void SetRatingStars(int selectedRating)
        {
            if (selectedRating >= 1) { this.Star1Icon.Kind = PackIconKind.Star; } else { this.Star1Icon.Kind = PackIconKind.StarOutline; }
            if (selectedRating >= 2) { this.Star2Icon.Kind = PackIconKind.Star; } else { this.Star2Icon.Kind = PackIconKind.StarOutline; }
            if (selectedRating >= 3) { this.Star3Icon.Kind = PackIconKind.Star; } else { this.Star3Icon.Kind = PackIconKind.StarOutline; }
            if (selectedRating >= 4) { this.Star4Icon.Kind = PackIconKind.Star; } else { this.Star4Icon.Kind = PackIconKind.StarOutline; }
            if (selectedRating >= 5) { this.Star5Icon.Kind = PackIconKind.Star; } else { this.Star5Icon.Kind = PackIconKind.StarOutline; }
        }
    }
}
