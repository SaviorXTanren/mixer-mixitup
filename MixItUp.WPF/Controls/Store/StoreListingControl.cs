using MixItUp.Base.Model.Store;
using System.Text;

namespace MixItUp.WPF.Controls.Store
{
    public class StoreListingControl : LoadingControlBase
    {
        protected MainStoreControl mainStoreControl { get; private set; }

        protected StoreListingModel listing { get; private set; }

        public StoreListingControl(MainStoreControl mainStoreControl, StoreListingModel listing)
        {
            this.mainStoreControl = mainStoreControl;
            if (listing != null)
            {
                this.DataContext = this.listing = listing;
            }
        }

        public string TooltipDescription
        {
            get
            {
                StringBuilder tooltipDescription = new StringBuilder();
                if (this.listing != null && this.listing.Description != null)
                {
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
                }
                return tooltipDescription.ToString();
            }
        }
    }
}
