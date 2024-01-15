using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

    [DataContract]
    public class OverlayPositionV3Model
    {
        public static int zIndexCounter = 0;

        [DataMember]
        public OverlayPositionV3Type Type { get; set; }

        [DataMember]
        public int XPosition { get; set; }
        [DataMember]
        public int YPosition { get; set; }

        [DataMember]
        public int Layer { get; set; }

        [JsonIgnore]
        public bool PositionTypeIsPercentage { get { return this.Type == OverlayPositionV3Type.Simple || this.Type == OverlayPositionV3Type.Percentage; } }
        [JsonIgnore]
        public string PositionType { get { return this.PositionTypeIsPercentage ? "%" : "px"; } }

        [JsonIgnore]
        public int XTranslation { get { return this.PositionTypeIsPercentage ? -50 : 0; } }
        [JsonIgnore]
        public int YTranslation { get { return this.PositionTypeIsPercentage ? -50 : 0; } }

        public virtual void SetPositionProperties(Dictionary<string, object> properties) { this.SetPositionPropertiesInternal(properties, this.XPosition, this.YPosition); }

        protected void SetPositionPropertiesInternal(Dictionary<string, object> properties, int x, int y)
        {
            if (this.Layer == 0)
            {
                zIndexCounter++;
            }

            properties[nameof(this.XPosition)] = x;
            properties[nameof(this.YPosition)] = y;
            properties[nameof(this.PositionType)] = this.PositionType;
            properties[nameof(this.XTranslation)] = this.XTranslation;
            properties[nameof(this.YTranslation)] = this.YTranslation;
            properties[nameof(this.Layer)] = (this.Layer == 0) ? zIndexCounter : this.Layer;
        }
    }

    [DataContract]
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

        [DataMember]
        public int XMaximum { get; set; }
        [DataMember]
        public int YMaximum { get; set; }

        public override void SetPositionProperties(Dictionary<string, object> properties)
        {
            int x = RandomHelper.GenerateRandomNumber(this.XMinimum, this.XMaximum);
            int y = RandomHelper.GenerateRandomNumber(this.YMinimum, this.YMaximum);
            this.SetPositionPropertiesInternal(properties, x, y);
        }
    }
}
