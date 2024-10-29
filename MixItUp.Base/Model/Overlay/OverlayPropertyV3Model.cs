using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayPropertyTypeV3Enum
    {
        Number,
        Text,
        Checkbox,
        DropDown,
        Color,
        Font,
    }

    [DataContract]
    public class OverlayPropertyV3Model
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public OverlayPropertyTypeV3Enum Type { get; set; }

        [DataMember]
        public object Value { get; set; }

        [DataMember]
        public double Minimum { get; set; }
        [DataMember]
        public double Maximum { get; set; }

        [DataMember]
        public List<object> Options { get; set; } = new List<object>();

        [JsonIgnore]
        public double NumberValue
        {
            get
            {
                if (this.Type == OverlayPropertyTypeV3Enum.Number && this.Value is double)
                {
                    return (double)this.Value;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string TextValue
        {
            get
            {
                if (this.Type == OverlayPropertyTypeV3Enum.Text && this.Value is string)
                {
                    return (string)this.Value;
                }
                return null;
            }
        }

        [JsonIgnore]
        public bool CheckboxValue
        {
            get
            {
                if (this.Type == OverlayPropertyTypeV3Enum.Checkbox && this.Value is bool)
                {
                    return (bool)this.Value;
                }
                return false;
            }
        }

        [JsonIgnore]
        public OverlayFontV3Model FontValue
        {
            get
            {
                if (this.Type == OverlayPropertyTypeV3Enum.Font && this.Value is OverlayFontV3Model)
                {
                    return (OverlayFontV3Model)this.Value;
                }
                return null;
            }
        }
    }
}
