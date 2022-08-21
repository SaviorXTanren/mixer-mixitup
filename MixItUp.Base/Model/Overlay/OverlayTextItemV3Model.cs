﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTextItemV3AlignmentTypeEnum
    {
        Left,
        Center,
        Right,
        Justify,
    }

    [DataContract]
    public class OverlayTextItemV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = "<p id=\"text-{ID}\">" + Environment.NewLine +
            "    {Text}" + Environment.NewLine +
            "</p>";

        public static readonly string DefaultCSS = "#text-{ID} {" + Environment.NewLine +
            "    font-size: {FontSize}px;" + Environment.NewLine +
            "    color: {FontColor};" + Environment.NewLine +
            "    font-family: '{FontFamily}';" + Environment.NewLine +
            "    font-weight: {FontWeight};" + Environment.NewLine +
            "    text-decoration: {TextDecoration};" + Environment.NewLine +
            "    font-style: {FontStyle};" + Environment.NewLine +
            "    text-align: {TextAlignment};" + Environment.NewLine +
            "    text-shadow: {Shadow};" + Environment.NewLine +
            "}";

        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public int FontSize { get; set; }
        [DataMember]
        public string FontName { get; set; }
        [DataMember]
        public string FontColor { get; set; }
        [DataMember]
        public bool Bold { get; set; }
        [DataMember]
        public bool Italics { get; set; }
        [DataMember]
        public bool Underline { get; set; }
        [DataMember]
        public OverlayTextItemV3AlignmentTypeEnum TextAlignment { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }

        public OverlayTextItemV3Model() : base(OverlayItemV3Type.Text) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, "Text", this.Text);
            item.CSS = ReplaceProperty(item.CSS, "Text", this.Text);
            item.Javascript = ReplaceProperty(item.Javascript, "Text", this.Text);

            item.HTML = ReplaceProperty(item.HTML, "FontSize", this.FontSize.ToString());
            item.CSS = ReplaceProperty(item.CSS, "FontSize", this.FontSize.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, "FontSize", this.FontSize.ToString());

            item.HTML = ReplaceProperty(item.HTML, "FontFamily", this.FontName);
            item.CSS = ReplaceProperty(item.CSS, "FontFamily", this.FontName);
            item.Javascript = ReplaceProperty(item.Javascript, "FontFamily", this.FontName);

            item.HTML = ReplaceProperty(item.HTML, "FontColor", this.FontColor);
            item.CSS = ReplaceProperty(item.CSS, "FontColor", this.FontColor);
            item.Javascript = ReplaceProperty(item.Javascript, "FontColor", this.FontColor);

            item.HTML = ReplaceProperty(item.HTML, "FontWeight", this.Bold ? "bold" : "normal");
            item.CSS = ReplaceProperty(item.CSS, "FontWeight", this.Bold ? "bold" : "normal");
            item.Javascript = ReplaceProperty(item.Javascript, "FontWeight", this.Bold ? "bold" : "normal");

            item.HTML = ReplaceProperty(item.HTML, "TextDecoration", this.Underline ? "underline" : "none");
            item.CSS = ReplaceProperty(item.CSS, "TextDecoration", this.Underline ? "underline" : "none");
            item.Javascript = ReplaceProperty(item.Javascript, "TextDecoration", this.Underline ? "underline" : "none");

            item.HTML = ReplaceProperty(item.HTML, "FontStyle", this.Italics ? "italic" : "normal");
            item.CSS = ReplaceProperty(item.CSS, "FontStyle", this.Italics ? "italic" : "normal");
            item.Javascript = ReplaceProperty(item.Javascript, "FontStyle", this.Italics ? "italic" : "normal");

            item.HTML = ReplaceProperty(item.HTML, "TextAlignment", this.TextAlignment.ToString().ToLower());
            item.CSS = ReplaceProperty(item.CSS, "TextAlignment", this.TextAlignment.ToString().ToLower());
            item.Javascript = ReplaceProperty(item.Javascript, "TextAlignment", this.TextAlignment.ToString().ToLower());

            if (!string.IsNullOrEmpty(this.ShadowColor))
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", $"2px 2px {this.ShadowColor}");
                item.CSS = ReplaceProperty(item.CSS, "Shadow", $"2px 2px {this.ShadowColor}");
                item.Javascript = ReplaceProperty(item.Javascript, "Shadow", $"2px 2px {this.ShadowColor}");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", "none");
                item.CSS = ReplaceProperty(item.CSS, "Shadow", "none");
                item.Javascript = ReplaceProperty(item.Javascript, "Shadow", "none");
            }

            return item;
        }
    }
}