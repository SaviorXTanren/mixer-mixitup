using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
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
    }

    public class StreamingSourceDimensions
    {
        public int X;
        public int Y;
        public int Rotation;
        public float XScale;
        public float YScale;
    }

    [DataContract]
    public class StreamingSoftwareAction : ActionBase
    {
        public const string SourceTextFilesDirectoryName = "SourceTextFiles";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamingSoftwareAction.asyncSemaphore; } }

        public static StreamingSoftwareAction CreateSceneAction(StreamingSoftwareTypeEnum softwareType, string sceneName)
        {
            StreamingSoftwareAction action = new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.Scene);
            action.SceneName = sceneName;
            return action;
        }

        public static StreamingSoftwareAction CreateSourceVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sourceName, bool sourceVisible)
        {
            StreamingSoftwareAction action = new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.SourceVisibility);
            action.SourceName = sourceName;
            action.SourceVisible = sourceVisible;
            return action;
        }

        public static StreamingSoftwareAction CreateTextSourceAction(StreamingSoftwareTypeEnum softwareType, string sourceName, bool sourceVisible, string sourceText, string sourceTextFilePath)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.TextSource;
            action.SourceText = sourceText;
            action.SourceTextFilePath = sourceTextFilePath;
            return action;
        }

        public static StreamingSoftwareAction CreateWebBrowserSourceAction(StreamingSoftwareTypeEnum softwareType, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sourceName, sourceVisible);
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

        public static StreamingSoftwareAction CreateSourceDimensionsAction(StreamingSoftwareTypeEnum softwareType, string sourceName, bool sourceVisible, StreamingSourceDimensions sourceDimensions)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.SourceDimensions;
            action.SourceDimensions = sourceDimensions;
            return action;
        }

        public static StreamingSoftwareAction CreateStartStopStreamAction(StreamingSoftwareTypeEnum softwareType)
        {
            return new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.StartStopStream);
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
        public StreamingSourceDimensions SourceDimensions { get; set; }

        public StreamingSoftwareAction() : base(ActionTypeEnum.StreamingSoftware) { }

        public StreamingSoftwareAction(StreamingSoftwareTypeEnum softwareType, StreamingActionTypeEnum actionType)
            : this()
        {
            this.SoftwareType = softwareType;
            this.ActionType = actionType;
        }

        public StreamingSoftwareTypeEnum SelectedStreamingSoftware { get { return (this.SoftwareType == StreamingSoftwareTypeEnum.DefaultSetting) ? ChannelSession.Settings.DefaultStreamingSoftware : this.SoftwareType; } }

        public void UpdateReferenceTextFile(string textToWrite)
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.ActionType == StreamingActionTypeEnum.TextSource && !string.IsNullOrEmpty(this.SourceText))
            {
                this.UpdateReferenceTextFile(await this.ReplaceStringWithSpecialModifiers(this.SourceText, user, arguments));
            }

            string url = string.Empty;
            if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
            {
                url = await this.ReplaceStringWithSpecialModifiers(this.SourceURL, user, arguments);
            }

            if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.OBSStudio)
            {
                if (ChannelSession.Services.OBSWebsocket == null)
                {
                    await ChannelSession.Services.InitializeOBSWebsocket();
                }

                if (ChannelSession.Services.OBSWebsocket != null)
                {
                    if (this.ActionType == StreamingActionTypeEnum.StartStopStream)
                    {
                        ChannelSession.Services.OBSWebsocket.StartEndStream();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(this.SceneName))
                    {
                        ChannelSession.Services.OBSWebsocket.SetCurrentScene(this.SceneName);
                    }
                    else if (!string.IsNullOrEmpty(this.SourceName))
                    {
                        if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            ChannelSession.Services.OBSWebsocket.SetWebBrowserSource(this.SourceName, url);
                        }
                        else if (this.ActionType == StreamingActionTypeEnum.SourceDimensions && this.SourceDimensions != null)
                        {
                            ChannelSession.Services.OBSWebsocket.SetSourceDimensions(this.SourceName, this.SourceDimensions);
                        }
                        ChannelSession.Services.OBSWebsocket.SetSourceRender(this.SourceName, this.SourceVisible);
                    }
                }
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.XSplit)
            {
                if (ChannelSession.Services.XSplitServer == null)
                {
                    await ChannelSession.Services.InitializeXSplitServer();
                }

                if (ChannelSession.Services.XSplitServer != null)
                {
                    if (this.ActionType == StreamingActionTypeEnum.StartStopStream)
                    {
                        // Do nothing...
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(this.SceneName))
                    {
                        await ChannelSession.Services.XSplitServer.SetCurrentScene(new XSplitScene() { sceneName = this.SceneName });
                    }
                    else if (!string.IsNullOrEmpty(this.SourceName))
                    {
                        if (this.ActionType == StreamingActionTypeEnum.WebBrowserSource && !string.IsNullOrEmpty(this.SourceURL))
                        {
                            await ChannelSession.Services.XSplitServer.SetWebBrowserSource(new XSplitWebBrowserSource() { sourceName = this.SourceName, sourceVisible = this.SourceVisible, webBrowserUrl = url });
                        }
                        await ChannelSession.Services.XSplitServer.SetSourceVisibility(new XSplitSource() { sourceName = this.SourceName, sourceVisible = this.SourceVisible });
                    }
                }
            }
            else if (this.SelectedStreamingSoftware == StreamingSoftwareTypeEnum.StreamlabsOBS)
            {
                if (ChannelSession.Services.StreamlabsOBSService == null)
                {
                    await ChannelSession.Services.InitializeStreamlabsOBSService();
                }

                if (ChannelSession.Services.StreamlabsOBSService != null)
                {
                    if (this.ActionType == StreamingActionTypeEnum.StartStopStream)
                    {
                        await ChannelSession.Services.StreamlabsOBSService.StartStopStream();
                    }
                    else if (this.ActionType == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(this.SceneName))
                    {
                        IEnumerable<StreamlabsOBSScene> scenes = await ChannelSession.Services.StreamlabsOBSService.GetScenes();
                        StreamlabsOBSScene selectedScene = scenes.FirstOrDefault(s => s.Name.Equals(this.SceneName));
                        if (selectedScene != null)
                        {
                            await ChannelSession.Services.StreamlabsOBSService.MakeSceneActive(selectedScene);
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.SourceName))
                    {
                        StreamlabsOBSScene activeScene = await ChannelSession.Services.StreamlabsOBSService.GetActiveScene();
                        if (activeScene != null)
                        {
                            IEnumerable<StreamlabsOBSSceneItem> sceneItems = await ChannelSession.Services.StreamlabsOBSService.GetSceneItems(activeScene);
                            StreamlabsOBSSceneItem selectedSceneItem = sceneItems.FirstOrDefault(s => s.Name.Equals(this.SourceName));
                            if (selectedSceneItem != null)
                            {
                                await ChannelSession.Services.StreamlabsOBSService.SetSceneItemVisibility(selectedSceneItem, this.SourceVisible);
                            }
                        }
                    }
                }
            }
        }
    }
}
