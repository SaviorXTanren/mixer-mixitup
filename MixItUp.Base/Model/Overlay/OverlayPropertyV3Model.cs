using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        public async Task<Dictionary<string, object>> GetGenerationProperties(CommandParametersModel parameters)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            if (this.Type == OverlayPropertyTypeV3Enum.Font)
            {
                properties[this.Name + nameof(this.FontValue.FontSize)] = this.FontValue.FontSize;
                properties[this.Name + nameof(this.FontValue.FontFamily)] = this.FontValue.FontFamily;
                properties[this.Name + nameof(this.FontValue.FontColor)] = this.FontValue.FontColor;
                properties[this.Name + nameof(this.FontValue.FontWeight)] = this.FontValue.FontWeight;
                properties[this.Name + nameof(this.FontValue.TextDecoration)] = this.FontValue.TextDecoration;
                properties[this.Name + nameof(this.FontValue.FontStyle)] = this.FontValue.FontStyle;
                properties[this.Name + nameof(this.FontValue.TextAlignment)] = this.FontValue.TextAlignment.ToString().ToLower();
                properties[this.Name + nameof(this.FontValue.ShadowColor)] = (!string.IsNullOrEmpty(this.FontValue.ShadowColor)) ? $"1px 1px {this.FontValue.ShadowColor}" : "none";
            }
            else if (this.Type == OverlayPropertyTypeV3Enum.Number)
            {
                double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextValue, parameters), out double result);
                result = Math.Max(result, this.Minimum);
                result = Math.Min(result, this.Maximum);
                properties[this.Name] = result;
            }
            else if (this.Type == OverlayPropertyTypeV3Enum.Checkbox)
            {
                properties[this.Name] = this.CheckboxValue;
            }
            else
            {
                properties[this.Name] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextValue, parameters);
            }
            return properties;
        }
    }
}
