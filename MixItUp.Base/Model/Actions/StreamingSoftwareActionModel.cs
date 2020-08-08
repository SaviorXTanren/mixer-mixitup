using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum StreamingSoftwareTypeEnum
    {
        [Name("Default Setting")]
        DefaultSetting,

        [Name("OBS Studio")]
        OBSStudio,
        [Name("XSplit")]
        XSplit,
        [Name("Streamlabs OBS")]
        StreamlabsOBS,
    }

    public enum StreamingActionTypeEnum
    {
        Scene,

        [Name("Source Visibility")]
        SourceVisibility,
        [Name("Text Source")]
        TextSource,
        [Name("Web Browser Source")]
        WebBrowserSource,
        [Name("Source Dimensions")]
        SourceDimensions,

        [Name("Start/Stop Stream")]
        StartStopStream,

        [Name("Save Replay Buffer")]
        SaveReplayBuffer,

        [Name("Set Scene Collection")]
        SceneCollection,
    }

    [DataContract]
    public class StreamingSourceDimensionsModel
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public int Rotation { get; set; }
        [DataMember]
        public float XScale { get; set; }
        [DataMember]
        public float YScale { get; set; }
    }

    [DataContract]
    public class StreamingSoftwareActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamingSoftwareActionModel.asyncSemaphore; } }

        public const string SourceTextFilesDirectoryName = "SourceTextFiles";

        public static StreamingSoftwareActionModel CreateSceneAction(StreamingSoftwareTypeEnum softwareType, string sceneName)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.Scene);
            action.SceneName = sceneName;
            return action;
        }

        public static StreamingSoftwareActionModel CreateSourceVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.SourceVisibility);
            action.SceneName = sceneName;
            action.SourceName = sourceName;
            action.SourceVisible = sourceVisible;
            return action;
        }

        public static StreamingSoftwareActionModel CreateTextSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceText, string sourceTextFilePath)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.TextSource;
            action.SourceText = sourceText;
            action.SourceTextFilePath = sourceTextFilePath;
            return action;
        }

        public static StreamingSoftwareActionModel CreateWebBrowserSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.WebBrowserSource;
            action.SourceURL = sourceURL;
            if (softwareType == StreamingSoftwareTypeEnum.XSplit)
            {
                if (!File.Exists(action.SourceURL) && !action.SourceURL.Contains("://"))
                {
                    action.SourceURL = "http://" + action.SourceURL;
                }
            }
            return action;
        }

        public static StreamingSoftwareActionModel CreateSourceDimensionsAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, StreamingSourceDimensionsModel sourceDimensions)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.SourceDimensions;
            action.SourceDimensions = sourceDimensions;
            return action;
        }

        public static StreamingSoftwareActionModel CreateStartStopStreamAction(StreamingSoftwareTypeEnum softwareType)
        {
            return new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.StartStopStream);
        }

        public static StreamingSoftwareActionModel CreateSaveReplayBufferAction(StreamingSoftwareTypeEnum softwareType)
        {
            return new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.SaveReplayBuffer);
        }

        public static StreamingSoftwareActionModel CreateSceneCollectionAction(StreamingSoftwareTypeEnum softwareType, string sceneCollectionName)
        {
            return new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.SceneCollection)
            {
                SceneCollectionName = sceneCollectionName
            };
        }

        [DataMember]
        public StreamingSoftwareTypeEnum SoftwareType { get; set; }
        [DataMember]
        public StreamingActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public bool SourceVisible { get; set; }

        [DataMember]
        public string SourceText { get; set; }
        [DataMember]
        public string SourceTextFilePath { get; set; }

        [DataMember]
        public string SourceURL { get; set; }

        [DataMember]
        public StreamingSourceDimensionsModel SourceDimensions { get; set; }

        [DataMember]
        public string SceneCollectionName { get; set; }

        public StreamingSoftwareActionModel(StreamingSoftwareTypeEnum softwareType, StreamingActionTypeEnum actionType)
            : base(ActionTypeEnum.StreamingSoftware)
        {
            this.SoftwareType = softwareType;
            this.ActionType = actionType;
        }

        // TODO
        public StreamingSoftwareTypeEnum SelectedStreamingSoftware { get { return StreamingSoftwareTypeEnum.DefaultSetting; } }// (this.SoftwareType == StreamingSoftwareTypeEnum.DefaultSetting) ? ChannelSession.Settings.DefaultStreamingSoftware : this.SoftwareType; } }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.ActionType == StreamingActionTypeEnum.TextSource && !string.IsNullOrEmpty(this.SourceText))
            {
                this.UpdateReferenceTextFile(await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, platform, arguments, specialIdentifiers));
            }

            string url = string.Empty;
            if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
            {
                url = await this.ReplaceStringWithSpecialModifiers(this.SourceURL, user, platform, arguments, specialIdentifiers);
            }

            string sceneName = null;
            if (!string.IsNullOrEmpty(this.SceneName))
            {
                sceneName = await this.ReplaceStringWithSpecialModifiers(this.SceneName, user, platform, arguments, specialIdentifiers);
            }

            string sourceName = null;
            if (!string.IsNullOrEmpty(this.SourceName))
            {
                sourceName = await this.ReplaceStringWithSpecialModifiers(this.SourceName, user, platform, arguments, specialIdentifiers);
            }

            IStreamingSoftwareService ssService = null;
            if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.OBSStudio)
            {
                ssService = ChannelSession.Services.OBSStudio;
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.XSplit)
            {
                ssService = ChannelSession.Services.XSplit;
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.StreamlabsOBS)
            {
                ssService = ChannelSession.Services.StreamlabsOBS;
            }

            if (ssService != null && ssService.IsEnabled)
            {
                Logger.Log(LogLevel.Debug, "Checking for Streaming Software connection");

                if (!ssService.IsConnected)
                {
                    Result result = await ssService.Connect();
                    if (!result.Success)
                    {
                        Logger.Log(LogLevel.Error, result.Message);
                        return;
                    }
                }

                Logger.Log(LogLevel.Debug, "Performing for Streaming Software connection");

                if (ssService.IsConnected)
                {
                    if (this.ActionType == StreamingActionTypeEnum.StartStopStream)
                    {
                        await ssService.StartStopStream();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.SaveReplayBuffer)
                    {
                        await ssService.SaveReplayBuffer();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(sceneName))
                    {
                        await ssService.ShowScene(sceneName);
                    }
                    else if (!string.IsNullOrEmpty(sourceName))
                    {
                        if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ssService.SetWebBrowserSourceURL(sceneName, sourceName, url);
                        }
                        else if (this.ActionType == StreamingActionTypeEnum.SourceDimensions && this.SourceDimensions != null)
                        {
                            // TODO
                            //await ssService.SetSourceDimensions(sceneName, sourceName, this.SourceDimensions);
                        }
                        await ssService.SetSourceVisibility(sceneName, sourceName, this.SourceVisible);
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.SceneCollection && !string.IsNullOrEmpty(this.SceneCollectionName))
                    {
                        await ssService.SetSceneCollection(this.SceneCollectionName);
                    }
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "The Streaming Software selected is not enabled: " + this.SelectedStreamingSoftware);
            }
        }

        private void UpdateReferenceTextFile(string textToWrite)
        {
            if (!string.IsNullOrEmpty(this.SourceText) && !string.IsNullOrEmpty(this.SourceTextFilePath))
            {
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(this.SourceTextFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(this.SourceTextFilePath));
                    }

                    using (StreamWriter writer = new StreamWriter(File.Open(this.SourceTextFilePath, FileMode.Create)))
                    {
                        writer.Write(textToWrite);
                        writer.Flush();
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }
    }
}
