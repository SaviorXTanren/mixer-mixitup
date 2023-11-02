using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
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
        public List<OverlayAnimationV3Model> Animations { get; set; } = new List<OverlayAnimationV3Model>();

        public string TextID { get { return this.ID.ToString(); } }

        public string GenerateWidgetHTMLOutput()
        {
            string content = OverlayResources.OverlayIFrameTemplate;

            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.HTML), this.HTML);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.CSS), this.CSS);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.Javascript), this.Javascript);
            content = OverlayOutputV3Model.ReplaceProperty(content, "BasicAnimations", string.Empty);

            return content;
        }

        public string GenerateBasicHTMLOutput()
        {
            string content = OverlayResources.OverlayIFrameTemplate;

            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.HTML), this.HTML);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.CSS), this.CSS);
            content = OverlayOutputV3Model.ReplaceProperty(content, nameof(this.Javascript), this.Javascript);

            double.TryParse(this.Duration, out double duration);

            StringBuilder animations = new StringBuilder();
            animations.AppendLine(this.Animations[0].GenerateAnimationJavascript());
            animations.AppendLine(this.Animations[1].GenerateTimedAnimationJavascript(duration / 2));
            animations.AppendLine(this.Animations[2].GenerateRemoveAnimationJavascript(this.TextID, duration));
            content = OverlayOutputV3Model.ReplaceProperty(content, "BasicAnimations", animations.ToString());

            return content;
        }
    }
}
