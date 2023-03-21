using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayOutputV3Model
    {
        public static string ReplaceProperty(string text, string name, string value) { return text.Replace($"{{{name}}}", value ?? string.Empty); }

        public static async Task<string> ReplaceScriptTag(string text, string fileName, string filePath)
        {
            return text.Replace($"<script src=\"{fileName}\"></script>", $"<script>{await ServiceManager.Get<IFileService>().ReadFile(filePath)}</script>");
        }

        public static async Task<string> ReplaceCSSStyleSheetTag(string text, string fileName, string filePath)
        {
            return text.Replace($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{fileName}\">", $"<style>{await ServiceManager.Get<IFileService>().ReadFile(filePath)}</style>");
        }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;

        [DataMember]
        public string Duration { get; set; }

        [DataMember]
        public Dictionary<string, OverlayAnimationV3Model> Animations { get; set; } = new Dictionary<string, OverlayAnimationV3Model>();

        public string GenerateFullHTMLOutput()
        {
            string content = Resources.OverlayBasicIFrameTemplate;
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.HTML), this.HTML);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.CSS), this.CSS);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.Javascript), this.Javascript);
            return content;
        }
    }

    [DataContract]
    public class OverlayBasicOutputV3Model
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public string Duration { get; set; }

        public OverlayBasicOutputV3Model(OverlayOutputV3Model output)
        {
            this.ID = output.ID.ToString();
            this.URL = "data/" + this.ID;
            this.Duration = output.Duration;
        }
    }
}
