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
            action.Name = sceneName;
            return action;
        }

        public static StreamingSoftwareActionModel CreateSourceVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingActionTypeEnum.SourceVisibility);
            action.ParentName = sceneName;
            action.Name = sourceName;
            action.Visible = sourceVisible;
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
                Name = sceneCollectionName
            };
        }

        [DataMember]
        public StreamingSoftwareTypeEnum SoftwareType { get; set; }
        [DataMember]
        public StreamingActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ParentName { get; set; }

        [DataMember]
        public bool Visible { get; set; }

        [DataMember]
        public string SourceText { get; set; }
        [DataMember]
        public string SourceTextFilePath { get; set; }

        [DataMember]
        public string SourceURL { get; set; }

        [DataMember]
        public StreamingSourceDimensionsModel SourceDimensions { get; set; }

        public StreamingSoftwareActionModel(StreamingSoftwareTypeEnum softwareType, StreamingActionTypeEnum actionType)
            : base(ActionTypeEnum.StreamingSoftware)
        {
            this.SoftwareType = softwareType;
            this.ActionType = actionType;
        }

        internal StreamingSoftwareActionModel(MixItUp.Base.Actions.StreamingSoftwareAction action)
            : base(ActionTypeEnum.StreamingSoftware)
        {
            this.SoftwareType = (StreamingSoftwareTypeEnum)(int)action.SoftwareType;
            this.ActionType = (StreamingActionTypeEnum)(int)action.ActionType;
            if (this.ActionType == StreamingActionTypeEnum.SceneCollection)
            {
                this.Name = action.SceneCollectionName;
            }
            else if (this.ActionType == StreamingActionTypeEnum.Scene)
            {
                this.Name = action.SceneName;
            }
            else if (this.ActionType == StreamingActionTypeEnum.SourceDimensions || this.ActionType == StreamingActionTypeEnum.SourceVisibility ||
                this.ActionType == StreamingActionTypeEnum.TextSource || this.ActionType == StreamingActionTypeEnum.WebBrowserSource)
            {
                this.Name = action.SourceName;
                this.ParentName = action.SceneName;
            }
            this.Visible = action.SourceVisible;
            this.SourceText = action.SourceText;
            this.SourceTextFilePath = action.SourceTextFilePath;
            this.SourceURL = action.SourceURL;
            if (action.SourceDimensions != null)
            {
                this.SourceDimensions = new StreamingSourceDimensionsModel()
                {
                    X = action.SourceDimensions.X, Y = action.SourceDimensions.Y, XScale = action.SourceDimensions.XScale, YScale = action.SourceDimensions.YScale, Rotation = action.SourceDimensions.Rotation
                };
            }
        }

        // TODO
        public StreamingSoftwareTypeEnum SelectedStreamingSoftware { get { return StreamingSoftwareTypeEnum.DefaultSetting; } }// (this.SoftwareType == StreamingSoftwareTypeEnum.DefaultSetting) ? ChannelSession.Settings.DefaultStreamingSoftware : this.SoftwareType; } }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
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
                    string name = null;
                    if (!string.IsNullOrEmpty(this.Name))
                    {
                        name = await this.ReplaceStringWithSpecialModifiers(this.Name, user, platform, arguments, specialIdentifiers);
                    }

                    string parentName = null;
                    if (!string.IsNullOrEmpty(this.ParentName))
                    {
                        parentName = await this.ReplaceStringWithSpecialModifiers(this.ParentName, user, platform, arguments, specialIdentifiers);
                    }

                    if (this.ActionType == StreamingActionTypeEnum.StartStopStream)
                    {
                        await ssService.StartStopStream();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.SaveReplayBuffer)
                    {
                        await ssService.SaveReplayBuffer();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(name))
                    {
                        await ssService.ShowScene(name);
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ssService.SetWebBrowserSourceURL(parentName, name, await this.ReplaceStringWithSpecialModifiers(this.SourceURL, user, platform, arguments, specialIdentifiers));
                        }
                        else if (this.ActionType == StreamingActionTypeEnum.TextSource && !string.IsNullOrEmpty(this.SourceText) && !string.IsNullOrEmpty(this.SourceTextFilePath))
                        {
                            try
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(this.SourceTextFilePath)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(this.SourceTextFilePath));
                                }

                                using (StreamWriter writer = new StreamWriter(File.Open(this.SourceTextFilePath, FileMode.Create)))
                                {
                                    writer.Write(await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, platform, arguments, specialIdentifiers));
                                    writer.Flush();
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                        else if (this.ActionType == StreamingActionTypeEnum.SourceDimensions && this.SourceDimensions != null)
                        {
                            // TODO
                            //await ssService.SetSourceDimensions(parentName, name, this.SourceDimensions);
                        }
                        await ssService.SetSourceVisibility(parentName, name, this.Visible);
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.SceneCollection && !string.IsNullOrEmpty(name))
                    {
                        await ssService.SetSceneCollection(name);
                    }
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "The Streaming Software selected is not enabled: " + this.SelectedStreamingSoftware);
            }
        }
    }
}
