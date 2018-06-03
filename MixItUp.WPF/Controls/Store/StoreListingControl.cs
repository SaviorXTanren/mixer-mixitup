using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
using MixItUp.Base.Model.Store;
using System;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Store
{
    public class StoreListingControl : LoadingControlBase
    {
        public static BitmapImage DefaultDisplayImage { get; private set; }

        private StoreListingModel listing;

        public StoreListingControl(StoreListingModel listing)
        {
            this.DataContext = this.listing = listing;

            if (StoreListingControl.DefaultDisplayImage == null)
            {
                StoreListingControl.DefaultDisplayImage = this.CreateImage(StoreListingModel.DefaultDisplayImage);
            }
        }

        public BitmapImage DisplayImage { get { return (!string.IsNullOrEmpty(this.listing.DisplayImage)) ? this.CreateImage(this.listing.DisplayImage) : StoreListingControl.DefaultDisplayImage; } }

        private BitmapImage CreateImage(string url)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();
            return bitmap;
        }
    }
}
