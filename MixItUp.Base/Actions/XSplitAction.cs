using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Threading;

namespace MixItUp.Base.Actions
{
    public class XSplitAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return XSplitAction.asyncSemaphore; } }

        public static string XSplitReferenceTextFilesDirectory = Path.Combine(Environment.CurrentDirectory, "XSplit", "SourceTextFiles");

        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public bool SourceVisible { get; set; }

        [DataMember]
        public string SourceText { get; set; }

        [JsonIgnore]
        private string currentTextToWrite { get; set; }

        public XSplitAction() { }

        public XSplitAction(string sceneName) : this(sceneName, null, false, null) { }

        public XSplitAction(string sourceName, bool sourceVisible, string sourceText) : this(null, sourceName, sourceVisible, sourceText) { }

        private XSplitAction(string sceneName, string sourceName, bool sourceVisible, string sourceText)
            : base(ActionTypeEnum.XSplit)
        {
            this.SceneName = sceneName;
            this.SourceName = sourceName;
            this.SourceVisible = sourceVisible;
            this.SourceText = sourceText;
        }

        public string LoadTextFromFilePath { get { return Path.Combine(XSplitReferenceTextFilesDirectory, this.SourceName + ".txt"); } }

        public void UpdateReferenceTextFile()
        {
            if (!string.IsNullOrEmpty(this.SourceName) && !string.IsNullOrEmpty(this.SourceText))
            {
                try
                {
                    Directory.CreateDirectory(XSplitReferenceTextFilesDirectory);

                    string filePath = Path.Combine(XSplitReferenceTextFilesDirectory, this.SourceName + ".txt");
                    using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
                    {
                        writer.Write(this.currentTextToWrite);
                    }
                }
                catch (Exception) { }
            }
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.XSplitServer == null)
            {
                await ChannelSession.Services.InitializeXSplitServer();
            }

            if (ChannelSession.Services.XSplitServer != null)
            {
                if (!string.IsNullOrEmpty(this.SceneName))
                {
                    ChannelSession.Services.XSplitServer.SetCurrentScene(new XSplitScene() { sceneName = this.SceneName });
                }

                if (!string.IsNullOrEmpty(this.SourceName))
                {
                    if (!string.IsNullOrEmpty(this.SourceText))
                    {
                        this.currentTextToWrite = await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, arguments);
                        this.UpdateReferenceTextFile();
                    }
                    ChannelSession.Services.XSplitServer.UpdateSource(new XSplitSource() { sourceName = this.SourceName, sourceVisible = this.SourceVisible });
                }
            }
        }
    }
}
