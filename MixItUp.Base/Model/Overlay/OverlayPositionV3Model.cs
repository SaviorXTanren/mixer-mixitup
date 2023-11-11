using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayPositionV3Type
    {
        Simple,
        Percentage,
        Pixel,
        Random
    }

    public class OverlayPositionV3Model
    {
        public static int zIndexCounter = 0;

        public OverlayPositionV3Type Type { get; set; }
        
        public int XPosition { get; set; }
        public int YPosition { get; set; }

        public int Layer { get; set; }

        [JsonIgnore]
        public bool PositionTypeIsPercentage { get { return this.Type == OverlayPositionV3Type.Simple || this.Type == OverlayPositionV3Type.Percentage; } }
        [JsonIgnore]
        public string PositionType { get { return this.PositionTypeIsPercentage ? "%" : "px"; } }

        public virtual void SetPositionProperties(Dictionary<string, string> properties) { this.SetPositionPropertiesInternal(properties, this.XPosition, this.YPosition); }

        protected void SetPositionPropertiesInternal(Dictionary<string, string> properties, int x, int y)
        {
            if (this.Layer == 0)
            {
                zIndexCounter++;
            }

            properties[nameof(this.XPosition)] = x.ToString();
            properties[nameof(this.YPosition)] = y.ToString();
            properties[nameof(this.PositionType)] = this.PositionType;
            properties[nameof(this.Layer)] = (this.Layer == 0) ? zIndexCounter.ToString() : this.Layer.ToString();
        }
    }

    public class OverlayRandomPositionV3Model : OverlayPositionV3Model
    {
        public int XMinimum
        {
            get { return this.XPosition; }
            set { this.XPosition = value; }
        }

        public int YMinimum
        {
            get { return this.YPosition; }
            set { this.YPosition = value; }
        }

        public int XMaximum { get; set; }
        public int YMaximum { get; set; }

        public override void SetPositionProperties(Dictionary<string, string> properties)
        {
            int x = RandomHelper.GenerateRandomNumber(this.XMinimum, this.XMaximum);
            int y = RandomHelper.GenerateRandomNumber(this.YMinimum, this.YMaximum);
            this.SetPositionPropertiesInternal(properties, x, y);
        }
    }
}
