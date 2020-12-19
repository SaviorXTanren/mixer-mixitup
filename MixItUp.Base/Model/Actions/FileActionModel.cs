using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        [DataMember]
        public FileActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string TransferText { get; set; }

        [DataMember]
        public string LineIndex { get; set; }

        public FileActionModel(FileActionTypeEnum actionType, string filePath, string transferText, string lineIndex = null)
            : base(ActionTypeEnum.File)
        {
            this.ActionType = actionType;
            this.FilePath = filePath;
            this.TransferText = transferText;
            this.LineIndex = lineIndex;
        }

        internal FileActionModel(MixItUp.Base.Actions.FileAction action)
            : base(ActionTypeEnum.File)
        {
            this.ActionType = (FileActionTypeEnum)(int)action.FileActionType;
            this.FilePath = action.FilePath;
            this.TransferText = action.TransferText;
            this.LineIndex = action.LineIndexToRead;
        }

        private FileActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string filePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            if (this.ActionType == FileActionTypeEnum.SaveToFile || this.ActionType == FileActionTypeEnum.AppendToFile)
            {
                filePath = filePath.ToFilePathString();

                string textToWrite = (!string.IsNullOrEmpty(this.TransferText)) ? this.TransferText : string.Empty;
                textToWrite = await this.ReplaceStringWithSpecialModifiers(textToWrite, parameters);
                if (this.ActionType == FileActionTypeEnum.SaveToFile)
                {
                    await ChannelSession.Services.FileService.SaveFile(filePath, textToWrite);
                }
                else if (this.ActionType == FileActionTypeEnum.AppendToFile)
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
                parameters.SpecialIdentifiers.Remove(this.TransferText);

                string data = await ChannelSession.Services.FileService.ReadFile(filePath);
                if (!string.IsNullOrEmpty(data))
                {
                    if (this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                        this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile)
                    {
                        List<string> lines = new List<string>(data.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        if (lines.Count > 0)
                        {
                            int lineIndex = -1;
                            if (this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                            {
                                if (!string.IsNullOrEmpty(this.LineIndex))
                                {
                                    string lineToRead = await this.ReplaceStringWithSpecialModifiers(this.LineIndex, parameters);
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

                            if (this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile)
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

                    data = await this.ReplaceStringWithSpecialModifiers(data, parameters);
                    parameters.SpecialIdentifiers[this.TransferText] = data;
                }
            }
        }
    }
}
