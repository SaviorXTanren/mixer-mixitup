using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Clips;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class MixerClipsAction : ActionBase
    {
        public const string MixerClipURLSpecialIdentifier = "mixerclipurl";

        public const int MinimumLength = 15;
        public const int MaximumLength = 300;

        public const string VideoFileContentLocatorType = "HlsStreaming";

        private const string FFMPEGExecutablePath = "ffmpeg-4.0-win32-static\\bin\\ffmpeg.exe";

        public static string GetFFMPEGExecutablePath() { return Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), MixerClipsAction.FFMPEGExecutablePath); }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return MixerClipsAction.asyncSemaphore; } }

        [DataMember]
        public string ClipName { get; set; }

        [DataMember]
        public int ClipLength { get; set; }

        [DataMember]
        public bool ShowClipInfoInChat { get; set; }

        [DataMember]
        public bool DownloadClip { get; set; }
        [DataMember]
        public string DownloadDirectory { get; set; }

        public MixerClipsAction()
            : base(ActionTypeEnum.MixerClips)
        {
            this.ShowClipInfoInChat = true;
        }

        public MixerClipsAction(string clipName, int clipLength, bool showClipInfoInChat = true, bool downloadClip = false, string downloadDirectory = null)
            : this()
        {
            this.ClipName = clipName;
            this.ClipLength = clipLength;
            this.ShowClipInfoInChat = showClipInfoInChat;
            this.DownloadClip = downloadClip;
            this.DownloadDirectory = downloadDirectory;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                if (this.ShowClipInfoInChat)
                {
                    await ChannelSession.Chat.SendMessage("Sending clip creation request to Mixer...");
                }

                string clipName = await this.ReplaceStringWithSpecialModifiers(this.ClipName, user, arguments);
                DateTimeOffset clipCreationTime = DateTimeOffset.Now;

                if (!string.IsNullOrEmpty(clipName) && MixerClipsAction.MinimumLength <= this.ClipLength && this.ClipLength <= MixerClipsAction.MaximumLength)
                {
                    Logger.LogDiagnostic("Getting current broadcast...");
                    BroadcastModel broadcast = await ChannelSession.Connection.GetCurrentBroadcast();
                    if (broadcast == null)
                    {
                        Logger.LogDiagnostic("Could not find current broadcast...");
                        await ChannelSession.Chat.SendMessage("ERROR: Unable to get information about current broadcast for clip creation. Please verify that clips can be created by ensuring the Clips button on your stream is not grayed out and try restarting your stream to see if this resolves the issue.");
                        return;
                    }

                    Logger.LogDiagnostic("Checking if clips can be made...");
                    if (await ChannelSession.Connection.CanClipBeMade(broadcast))
                    {
                        Logger.LogDiagnostic("Clips can not be made...");
                        await ChannelSession.Chat.SendMessage("ERROR: Clips can not be made for current broadcast. Please verify that clips can be created by ensuring the Clips button on your stream is not grayed out and try restarting your stream to see if this resolves the issue.");
                        return;
                    }

                    Logger.LogDiagnostic("Creating clip...");
                    ClipModel clip = await ChannelSession.Connection.CreateClip(new ClipRequestModel()
                    {
                        broadcastId = broadcast.id.ToString(),
                        highlightTitle = clipName,
                        clipDurationInSeconds = this.ClipLength
                    });

                    if (clip == null)
                    {
                        Logger.LogDiagnostic("Clip data not returned, attempting to query for it...");
                        for (int i = 0; i < 10 && clip == null; i++)
                        {
                            await Task.Delay(2000);

                            IEnumerable<ClipModel> clips = await ChannelSession.Connection.GetChannelClips(ChannelSession.Channel);
                            clips = clips.OrderByDescending(c => c.uploadDate);
                            Logger.LogDiagnostic("Found Clips: " + string.Join(", ", clips.Select(c => c.title)));

                            clip = clips.FirstOrDefault();
                            if (clip != null && clip.uploadDate.ToLocalTime() >= clipCreationTime && clip.title.Equals(clipName))
                            {
                                break;
                            }
                            else
                            {
                                clip = null;
                            }
                        }
                    }

                    if (clip != null)
                    {
                        Logger.LogDiagnostic("Clip found: " + clip.contentId);
                        await this.ProcessClip(clip, clipName);
                    }
                    else
                    {
                        Logger.LogDiagnostic("Could not find clip with matching clip name");
                        await ChannelSession.Chat.SendMessage("ERROR: Unable to create clip or could not find clip. Please verify that clips can be created by ensuring the Clips button on your stream is not grayed out and try restarting your stream to see if this resolves the issue.");
                    }
                }
            }
        }

        private async Task ProcessClip(ClipModel clip, string clipName)
        {
            GlobalEvents.MixerClipCreated(clip);

            string clipUrl = string.Format("https://mixer.com/{0}?clip={1}", ChannelSession.Channel.token, clip.shareableId);

            if (this.ShowClipInfoInChat)
            {
                await ChannelSession.Chat.SendMessage("Clip Created: " + clipUrl);
            }

            this.extraSpecialIdentifiers[MixerClipURLSpecialIdentifier] = clipUrl;

            if (this.DownloadClip)
            {
                if (!Directory.Exists(this.DownloadDirectory))
                {
                    string error = "ERROR: The download folder specified for Mixer Clips does not exist";
                    Logger.Log(error);
                    await ChannelSession.Chat.Whisper(ChannelSession.User.username, error);
                    return;
                }

                if (!ChannelSession.Services.FileService.FileExists(MixerClipsAction.GetFFMPEGExecutablePath()))
                {
                    string error = "ERROR: FFMPEG could not be found and the Mixer Clip can not be converted without it";
                    Logger.Log(error);
                    await ChannelSession.Chat.Whisper(ChannelSession.User.username, error);
                    return;
                }

                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(VideoFileContentLocatorType));
                if (clipLocator != null)
                {
                    string destinationFile = Path.Combine(this.DownloadDirectory, clipName + ".mp4");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = MixerClipsAction.GetFFMPEGExecutablePath();
                        process.StartInfo.Arguments = string.Format("-i {0} -c copy -bsf:a aac_adtstoasc \"{1}\"", clipLocator.uri, destinationFile.ToFilePathString());
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();
                        while (!process.HasExited)
                        {
                            await Task.Delay(500);
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }
    }
}
