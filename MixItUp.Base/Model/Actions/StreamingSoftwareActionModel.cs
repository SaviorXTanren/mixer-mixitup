using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum StreamingSoftwareTypeEnum
    {
        DefaultSetting,

        OBSStudio,
        XSplit,
        StreamlabsDesktop,
    }

    public enum StreamingSoftwareActionTypeEnum
    {
        Scene,

        SourceVisibility,
        TextSource,
        WebBrowserSource,
        SourceDimensions,

        StartStopStream,

        SaveReplayBuffer,

        SceneCollection,

        SourceFilterVisibility,

        StartStopRecording,

        ImageSource,
        MediaSource,
    }

    [DataContract]
    public class StreamingSoftwareSourceDimensionsModel
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

        public StreamingSoftwareSourceDimensionsModel(int x, int y, int rotation, float xScale, float yScale)
        {
            this.X = x;
            this.Y = y;
            this.Rotation = rotation;
            this.XScale = xScale;
            this.YScale = yScale;
        }

        public StreamingSoftwareSourceDimensionsModel() { }
    }

    [DataContract]
    public class StreamingSoftwareActionModel : ActionModelBase
    {
        public const string SourceTextFilesDirectoryName = "SourceTextFiles";

        public static async Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName)
        {
            StreamingSoftwareSourceDimensionsModel dimensions = null;

            if (softwareType == StreamingSoftwareTypeEnum.DefaultSetting)
            {
                softwareType = ChannelSession.Settings.DefaultStreamingSoftware;
            }

            if (softwareType == StreamingSoftwareTypeEnum.OBSStudio)
            {
                if (ServiceManager.Get<IOBSStudioService>().IsConnected || (await ServiceManager.Get<IOBSStudioService>().Connect()).Success)
                {
                    dimensions = await ServiceManager.Get<IOBSStudioService>().GetSourceDimensions(sceneName, sourceName);
                }
            }
            else if (softwareType == StreamingSoftwareTypeEnum.StreamlabsDesktop)
            {
                if (ServiceManager.Get<StreamlabsDesktopService>().IsConnected || (await ServiceManager.Get<StreamlabsDesktopService>().Connect()).Success)
                {
                    dimensions = await ServiceManager.Get<StreamlabsDesktopService>().GetSourceDimensions(sceneName, sourceName);
                }
            }

            return dimensions;
        }

        public static StreamingSoftwareActionModel CreateSceneAction(StreamingSoftwareTypeEnum softwareType, string sceneName)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingSoftwareActionTypeEnum.Scene);
            action.ItemName = sceneName;
            return action;
        }

        public static StreamingSoftwareActionModel CreateSourceVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingSoftwareActionTypeEnum.SourceVisibility);
            action.ParentName = sceneName;
            action.ItemName = sourceName;
            action.Visible = sourceVisible;
            return action;
        }

        public static StreamingSoftwareActionModel CreateSourceFilterVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sourceName, string filterName, bool sourceVisible)
        {
            StreamingSoftwareActionModel action = new StreamingSoftwareActionModel(softwareType, StreamingSoftwareActionTypeEnum.SourceFilterVisibility);
            action.ParentName = sourceName;
            action.ItemName = filterName;
            action.Visible = sourceVisible;
            return action;
        }

        public static StreamingSoftwareActionModel CreateTextSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceText, string sourceTextFilePath)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingSoftwareActionTypeEnum.TextSource;
            action.SourceText = sourceText;
            action.SourceTextFilePath = sourceTextFilePath;
            return action;
        }

        public static StreamingSoftwareActionModel CreateImageSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingSoftwareActionTypeEnum.ImageSource;
            action.SourceURL = sourceURL;
            return action;
        }

        public static StreamingSoftwareActionModel CreateMediaSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingSoftwareActionTypeEnum.MediaSource;
            action.SourceURL = sourceURL;
            return action;
        }

        public static StreamingSoftwareActionModel CreateWebBrowserSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingSoftwareActionTypeEnum.WebBrowserSource;
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

        public static StreamingSoftwareActionModel CreateSourceDimensionsAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, StreamingSoftwareSourceDimensionsModel sourceDimensions)
        {
            StreamingSoftwareActionModel action = StreamingSoftwareActionModel.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingSoftwareActionTypeEnum.SourceDimensions;
            action.SourceDimensions = sourceDimensions;
            return action;
        }

        public static StreamingSoftwareActionModel CreateSceneCollectionAction(StreamingSoftwareTypeEnum softwareType, string sceneCollectionName)
        {
            return new StreamingSoftwareActionModel(softwareType, StreamingSoftwareActionTypeEnum.SceneCollection)
            {
                ItemName = sceneCollectionName
            };
        }

        [DataMember]
        public StreamingSoftwareTypeEnum StreamingSoftwareType { get; set; }
        [DataMember]
        public StreamingSoftwareActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ItemName { get; set; }
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
        public StreamingSoftwareSourceDimensionsModel SourceDimensions { get; set; }

        public StreamingSoftwareActionModel(StreamingSoftwareTypeEnum softwareType, StreamingSoftwareActionTypeEnum actionType)
            : base(ActionTypeEnum.StreamingSoftware)
        {
            this.StreamingSoftwareType = softwareType;
            this.ActionType = actionType;
        }

        [Obsolete]
        public StreamingSoftwareActionModel() { }

        public StreamingSoftwareTypeEnum SelectedStreamingSoftware { get { return (this.StreamingSoftwareType == StreamingSoftwareTypeEnum.DefaultSetting) ? ChannelSession.Settings.DefaultStreamingSoftware : this.StreamingSoftwareType; } }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            IStreamingSoftwareService ssService = null;
            if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.OBSStudio)
            {
                ssService = ServiceManager.Get<IOBSStudioService>();
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.XSplit)
            {
                ssService = ServiceManager.Get<XSplitService>();
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.StreamlabsDesktop)
            {
                ssService = ServiceManager.Get<StreamlabsDesktopService>();
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
                    if (!string.IsNullOrEmpty(this.ItemName))
                    {
                        name = await ReplaceStringWithSpecialModifiers(this.ItemName, parameters);
                    }

                    string parentName = null;
                    if (!string.IsNullOrEmpty(this.ParentName))
                    {
                        parentName = await ReplaceStringWithSpecialModifiers(this.ParentName, parameters);
                    }

                    if (this.ActionType == StreamingSoftwareActionTypeEnum.StartStopStream)
                    {
                        await ssService.StartStopStream();
                    }
                    else if (this.ActionType == StreamingSoftwareActionTypeEnum.StartStopRecording)
                    {
                        await ssService.StartStopRecording();
                    }
                    else if (this.ActionType == StreamingSoftwareActionTypeEnum.SaveReplayBuffer)
                    {
                        await ssService.SaveReplayBuffer();
                    }
                    else if (this.ActionType == StreamingSoftwareActionTypeEnum.Scene)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            await ssService.ShowScene(name);
                        }
                    }
                    else if (this.ActionType == StreamingSoftwareActionTypeEnum.SourceFilterVisibility)
                    {
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(parentName))
                        {
                            await ssService.SetSourceFilterVisibility(parentName, name, this.Visible);
                        }
                    }
                    else if (this.ActionType == StreamingSoftwareActionTypeEnum.SceneCollection)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            await ssService.SetSceneCollection(name);
                        }
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        if (this.ActionType == StreamingSoftwareActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ssService.SetWebBrowserSourceURL(parentName, name, await ReplaceStringWithSpecialModifiers(this.SourceURL, parameters));
                        }
                        else if (this.ActionType == StreamingSoftwareActionTypeEnum.ImageSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ssService.SetImageSourceFilePath(parentName, name, await ReplaceStringWithSpecialModifiers(this.SourceURL, parameters));
                        }
                        else if (this.ActionType == StreamingSoftwareActionTypeEnum.MediaSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ssService.SetMediaSourceFilePath(parentName, name, await ReplaceStringWithSpecialModifiers(this.SourceURL, parameters));
                        }
                        else if (this.ActionType == StreamingSoftwareActionTypeEnum.TextSource && !string.IsNullOrEmpty(this.SourceText) && !string.IsNullOrEmpty(this.SourceTextFilePath))
                        {
                            try
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(this.SourceTextFilePath)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(this.SourceTextFilePath));
                                }

                                using (StreamWriter writer = new StreamWriter(File.Open(this.SourceTextFilePath, FileMode.Create)))
                                {
                                    writer.Write(await ReplaceStringWithSpecialModifiers(this.SourceText, parameters));
                                    writer.Flush();
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                        else if (this.ActionType == StreamingSoftwareActionTypeEnum.SourceDimensions && this.SourceDimensions != null)
                        {
                            await ssService.SetSourceDimensions(parentName, name, this.SourceDimensions);
                        }
                        await ssService.SetSourceVisibility(parentName, name, this.Visible);
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
