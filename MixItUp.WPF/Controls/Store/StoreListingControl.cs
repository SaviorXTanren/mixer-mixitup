using MixItUp.Base.Model.Store;
using System;
using System.Text;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Store
{
    public class StoreListingControl : LoadingControlBase
    {
        public static BitmapImage DefaultDisplayImage { get; private set; }

        protected MainStoreControl mainStoreControl { get; private set; }

        protected StoreListingModel listing { get; private set; }

        public StoreListingControl(MainStoreControl mainStoreControl, StoreListingModel listing)
        {
            this.mainStoreControl = mainStoreControl;
            this.DataContext = this.listing = listing;

            if (StoreListingControl.DefaultDisplayImage == null)
            {
                StoreListingControl.DefaultDisplayImage = this.CreateImage(StoreListingModel.DefaultDisplayImage);
            }
        }

        public BitmapImage DisplayImage { get { return (!string.IsNullOrEmpty(this.listing.DisplayImage)) ? this.CreateImage(this.listing.DisplayImage) : StoreListingControl.DefaultDisplayImage; } }

        public string TooltipDescription
        {
            get
            {
                StringBuilder tooltipDescription = new StringBuilder();

                string currentLine = string.Empty;
                foreach (string split in this.listing.Description.Split(new char[] { ' ' }))
                {
                    currentLine += split;
                    if (currentLine.Length > 70)
                    {
                        tooltipDescription.AppendLine(currentLine);
                        currentLine = string.Empty;
                    }
                    else
                    {
                        currentLine += " ";
                    }
                }
                
                if (!string.IsNullOrEmpty(currentLine))
                {
                    tooltipDescription.Append(currentLine);
                }
                else
                {
                    tooltipDescription.Remove(tooltipDescription.Length - 2, 2);
                }

                return tooltipDescription.ToString();
            }
        }

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
