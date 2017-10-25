using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class OBSStudioAction : ActionBase
    {
        public static string OBSStudioReferenceTextFilesDirectory = Path.Combine(Environment.CurrentDirectory, "OBS", "SourceTextFiles");

        [DataMember]
        public string SceneCollection { get; set; }

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

        public OBSStudioAction() { }

        public OBSStudioAction(string sceneCollection, string sceneName) : this(sceneCollection, sceneName, null, false, null) { }

        public OBSStudioAction(string sourceName, bool sourceVisible, string sourceText) : this(null, null, sourceName, sourceVisible, sourceText) { }

        private OBSStudioAction(string sceneCollection, string sceneName, string sourceName, bool sourceVisible, string sourceText)
            : base(ActionTypeEnum.OBSStudio)
        {
            this.SceneCollection = sceneCollection;
            this.SceneName = sceneName;
            this.SourceName = sourceName;
            this.SourceVisible = sourceVisible;
            this.SourceText = sourceText;
        }

        public string LoadTextFromFilePath { get { return Path.Combine(OBSStudioReferenceTextFilesDirectory, this.SourceName + ".txt"); } }

        public void UpdateReferenceTextFile()
        {
            if (!string.IsNullOrEmpty(this.SourceName) && !string.IsNullOrEmpty(this.SourceText))
            {
                try
                {
                    Directory.CreateDirectory(OBSStudioReferenceTextFilesDirectory);

                    string filePath = Path.Combine(OBSStudioReferenceTextFilesDirectory, this.SourceName + ".txt");
                    using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
                    {
                        writer.Write(this.currentTextToWrite);
                    }
                }
                catch (Exception) { }
            }
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.OBSWebsocket == null)
            {
                await ChannelSession.Services.InitializeOBSWebsocket();
            }

            if (ChannelSession.Services.OBSWebsocket != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.SceneCollection))
                    {
                        ChannelSession.Services.OBSWebsocket.SetCurrentSceneCollection(this.SceneCollection);
                    }

                    if (!string.IsNullOrEmpty(this.SceneName))
                    {
                        ChannelSession.Services.OBSWebsocket.SetCurrentScene(this.SceneName);
                    }

                    if (!string.IsNullOrEmpty(this.SourceName))
                    {
                        if (!string.IsNullOrEmpty(this.SourceText))
                        {
                            this.currentTextToWrite = await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, arguments);
                            this.UpdateReferenceTextFile();
                        }
                        ChannelSession.Services.OBSWebsocket.SetSourceRender(this.SourceName, this.SourceVisible);
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
