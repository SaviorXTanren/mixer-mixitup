using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class StreamlabsOBSAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamlabsOBSAction.asyncSemaphore; } }

        public static string StreamlabsOBSStudioReferenceTextFilesDirectory = Path.Combine("StreamlabsOBS", "SourceTextFiles");

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

        public StreamlabsOBSAction() : base(ActionTypeEnum.StreamlabsOBS) { }

        public StreamlabsOBSAction(string sceneName)
            : this()
        {
            this.SceneName = sceneName;
        }

        public StreamlabsOBSAction(string sourceName, bool sourceVisible, string sourceText = null)
            : this()
        {
            this.SourceName = sourceName;
            this.SourceVisible = sourceVisible;
            this.SourceText = sourceText;
        }

        public string LoadTextFromDirectoryPath { get { return Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), StreamlabsOBSStudioReferenceTextFilesDirectory); } }
        public string LoadTextFromFilePath { get { return Path.Combine(LoadTextFromDirectoryPath, this.SourceName + ".txt"); } }

        public void UpdateReferenceTextFile()
        {
            if (!string.IsNullOrEmpty(this.SourceName) && !string.IsNullOrEmpty(this.SourceText))
            {
                try
                {
                    Directory.CreateDirectory(this.LoadTextFromDirectoryPath);
                    using (StreamWriter writer = new StreamWriter(File.Open(this.LoadTextFromFilePath, FileMode.Create)))
                    {
                        writer.Write(this.currentTextToWrite);
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.StreamlabsOBSService != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.SceneName))
                    {
                        IEnumerable<StreamlabsOBSScene> scenes = await ChannelSession.Services.StreamlabsOBSService.GetScenes();
                        StreamlabsOBSScene selectedScene = scenes.FirstOrDefault(s => s.Name.Equals(this.SceneName));
                        if (selectedScene != null)
                        {
                            await ChannelSession.Services.StreamlabsOBSService.MakeSceneActive(selectedScene);
                        }
                    }

                    if (!string.IsNullOrEmpty(this.SourceName))
                    {
                        StreamlabsOBSScene activeScene = await ChannelSession.Services.StreamlabsOBSService.GetActiveScene();
                        if (activeScene != null)
                        {
                            IEnumerable<StreamlabsOBSSceneItem> sceneItems = await ChannelSession.Services.StreamlabsOBSService.GetSceneItems(activeScene);
                            StreamlabsOBSSceneItem selectedSceneItem = sceneItems.FirstOrDefault(s => s.Name.Equals(this.SourceName));
                            if (selectedSceneItem != null)
                            {
                                if (!string.IsNullOrEmpty(this.SourceText))
                                {
                                    this.currentTextToWrite = await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, arguments);
                                    this.UpdateReferenceTextFile();
                                }
                                await ChannelSession.Services.StreamlabsOBSService.SetSceneItemVisibility(selectedSceneItem, this.SourceVisible);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }
    }
}
