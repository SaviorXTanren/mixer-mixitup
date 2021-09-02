using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum StreamingSoftwareTypeEnum
    {
        DefaultSetting,

        OBSStudio,
        XSplit,
        StreamlabsOBS,
    }

    [Obsolete]
    public enum StreamingActionTypeEnum
    {
        Scene,

        SourceVisibility,
        TextSource,
        WebBrowserSource,
        SourceDimensions,

        StartStopStream,

        SaveReplayBuffer,

        SceneCollection,
    }

    [Obsolete]
    public class StreamingSourceDimensions
    {
        public int X;
        public int Y;
        public int Rotation;
        public float XScale;
        public float YScale;
    }

    [Obsolete]
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

        public static StreamingSoftwareAction CreateSourceVisibilityAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible)
        {
            StreamingSoftwareAction action = new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.SourceVisibility);
            action.SceneName = sceneName;
            action.SourceName = sourceName;
            action.SourceVisible = sourceVisible;
            return action;
        }

        public static StreamingSoftwareAction CreateTextSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceText, string sourceTextFilePath)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.TextSource;
            action.SourceText = sourceText;
            action.SourceTextFilePath = sourceTextFilePath;
            return action;
        }

        public static StreamingSoftwareAction CreateWebBrowserSourceAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, string sourceURL)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
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

        public static StreamingSoftwareAction CreateSourceDimensionsAction(StreamingSoftwareTypeEnum softwareType, string sceneName, string sourceName, bool sourceVisible, StreamingSourceDimensions sourceDimensions)
        {
            StreamingSoftwareAction action = StreamingSoftwareAction.CreateSourceVisibilityAction(softwareType, sceneName, sourceName, sourceVisible);
            action.ActionType = StreamingActionTypeEnum.SourceDimensions;
            action.SourceDimensions = sourceDimensions;
            return action;
        }

        public static StreamingSoftwareAction CreateStartStopStreamAction(StreamingSoftwareTypeEnum softwareType)
        {
            return new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.StartStopStream);
        }

        public static StreamingSoftwareAction CreateSaveReplayBufferAction(StreamingSoftwareTypeEnum softwareType)
        {
            return new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.SaveReplayBuffer);
        }

        public static StreamingSoftwareAction CreateSceneCollectionAction(StreamingSoftwareTypeEnum softwareType, string sceneCollectionName)
        {
            return new StreamingSoftwareAction(softwareType, StreamingActionTypeEnum.SceneCollection)
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
        public StreamingSourceDimensions SourceDimensions { get; set; }

        [DataMember]
        public string SceneCollectionName { get; set; }

        public StreamingSoftwareAction() : base(ActionTypeEnum.StreamingSoftware) { }

        public StreamingSoftwareAction(StreamingSoftwareTypeEnum softwareType, StreamingActionTypeEnum actionType)
            : this()
        {
            this.SoftwareType = softwareType;
            this.ActionType = actionType;
        }

        public StreamingSoftwareTypeEnum SelectedStreamingSoftware { get { return this.SoftwareType; } }

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

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
