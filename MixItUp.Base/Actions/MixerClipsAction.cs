﻿using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Clips;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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

        private const string VideoFileContentLocatorType = "HlsStreaming";

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
                string cleanClipName = await this.ReplaceStringWithSpecialModifiers(this.ClipName, user, arguments, isFilePath: true);
                if (!string.IsNullOrEmpty(clipName) && MixerClipsAction.MinimumLength <= this.ClipLength && this.ClipLength <= MixerClipsAction.MaximumLength)
                {
                    bool clipCreated = false;
                    DateTimeOffset clipCreationTime = DateTimeOffset.Now;

                    BroadcastModel broadcast = await ChannelSession.Connection.GetCurrentBroadcast();
                    if (broadcast != null)
                    {
                        if (await ChannelSession.Connection.CanClipBeMade(broadcast))
                        {
                            clipCreated = await ChannelSession.Connection.CreateClip(new ClipRequestModel()
                            {
                                broadcastId = broadcast.id.ToString(),
                                highlightTitle = clipName,
                                clipDurationInSeconds = this.ClipLength
                            });
                        }
                    }

                    if (clipCreated)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await Task.Delay(2000);

                            IEnumerable<ClipModel> clips = await ChannelSession.Connection.GetChannelClips(ChannelSession.Channel);
                            ClipModel clip = clips.OrderByDescending(c => c.uploadDate).FirstOrDefault();
                            if (clip != null && clip.uploadDate.ToLocalTime() >= clipCreationTime && clip.title.Equals(clipName))
                            {
                                string clipUrl = string.Format("https://mixer.com/{0}?clip={1}", ChannelSession.User.username, clip.shareableId);

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
                                        string destinationFile = Path.Combine(this.DownloadDirectory, cleanClipName + ".mp4");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                        Task.Run(async () =>
                                        {
                                            Process process = new Process();
                                            process.StartInfo.FileName = MixerClipsAction.GetFFMPEGExecutablePath();
                                            process.StartInfo.Arguments = string.Format("-i {0} -c copy -bsf:a aac_adtstoasc \"{1}\"", clipLocator.uri, destinationFile);
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
                                return;
                            }
                        }
                        await ChannelSession.Chat.SendMessage("Clip was created, but could not be retrieved at this time");
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Unable to create clip, please verify that clips can be created by ensuring the Clips button on your stream is not grayed out.");
                    }
                }
            }
        }
    }
}
