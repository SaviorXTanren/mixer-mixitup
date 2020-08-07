using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum FileActionTypeEnum
    {
        SaveToFile,
        AppendToFile,
        ReadFromFile,
        ReadSpecificLineFromFile,
        ReadRandomLineFromFile,
        RemoveSpecificLineFromFile,
        RemoveRandomLineFromFile,
    }

    [DataContract]
    public class FileActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return FileActionModel.asyncSemaphore; } }

        [DataMember]
        public FileActionTypeEnum FileActionType { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string TransferText { get; set; }

        [DataMember]
        public string LineIndexToRead { get; set; }

        public FileActionModel(FileActionTypeEnum fileActionType, string filePath, string transferText)
            : base(ActionTypeEnum.File)
        {
            this.FileActionType = fileActionType;
            this.FilePath = filePath;
            this.TransferText = transferText;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            string filePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, user, platform, arguments, specialIdentifiers);
            if (this.FileActionType == FileActionTypeEnum.SaveToFile || this.FileActionType == FileActionTypeEnum.AppendToFile)
            {
                filePath = filePath.ToFilePathString();

                string textToWrite = (!string.IsNullOrEmpty(this.TransferText)) ? this.TransferText : string.Empty;
                textToWrite = await this.ReplaceStringWithSpecialModifiers(textToWrite, user, platform, arguments, specialIdentifiers);
                if (this.FileActionType == FileActionTypeEnum.SaveToFile)
                {
                    await ChannelSession.Services.FileService.SaveFile(filePath, textToWrite);
                }
                else if (this.FileActionType == FileActionTypeEnum.AppendToFile)
                {
                    string dataToWrite = textToWrite;
                    if (!string.IsNullOrEmpty(await ChannelSession.Services.FileService.ReadFile(filePath)))
                    {
                        dataToWrite = Environment.NewLine + dataToWrite;
                    }
                    await ChannelSession.Services.FileService.AppendFile(filePath, dataToWrite);
                }
            }
            else
            {
                specialIdentifiers.Remove(this.TransferText);

                string data = await ChannelSession.Services.FileService.ReadFile(filePath);
                if (!string.IsNullOrEmpty(data))
                {
                    if (this.FileActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.FileActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                        this.FileActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.FileActionType == FileActionTypeEnum.RemoveRandomLineFromFile)
                    {
                        List<string> lines = new List<string>(data.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        if (lines.Count > 0)
                        {
                            int lineIndex = -1;
                            if (this.FileActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.FileActionType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                            {
                                if (!string.IsNullOrEmpty(this.LineIndexToRead))
                                {
                                    string lineToRead = await this.ReplaceStringWithSpecialModifiers(this.LineIndexToRead, user, platform, arguments, specialIdentifiers);
                                    if (int.TryParse(lineToRead, out lineIndex))
                                    {
                                        lineIndex = lineIndex - 1;
                                        if (lineIndex >= 0 && lineIndex < lines.Count)
                                        {
                                            data = lines[lineIndex];
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                lineIndex = RandomHelper.GenerateRandomNumber(lines.Count);
                                data = lines[lineIndex];
                            }

                            if (this.FileActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.FileActionType == FileActionTypeEnum.RemoveRandomLineFromFile)
                            {
                                if (lineIndex >= 0)
                                {
                                    lines.RemoveAt(lineIndex);
                                    await ChannelSession.Services.FileService.SaveFile(filePath, string.Join(Environment.NewLine, lines));
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    data = await this.ReplaceStringWithSpecialModifiers(data, user, platform, arguments, specialIdentifiers);
                    specialIdentifiers[this.TransferText] = data;
                }
            }
        }
    }
}
