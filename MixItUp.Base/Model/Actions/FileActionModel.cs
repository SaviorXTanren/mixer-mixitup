using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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
        InsertInFileAtSpecificLine,
        InsertInFileAtRandomLine,
        RemoveLineWithSpecificTextFromFile,
        CountLinesInFile,
        ReadEachLineFromFile,
    }

    [DataContract]
    public class FileActionModel : GroupActionModel
    {
        [DataMember]
        public FileActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string TransferText { get; set; }

        [DataMember]
        public string LineIndex { get; set; }

        [DataMember]
        public bool CaseSensitive { get; set; }

        public FileActionModel(FileActionTypeEnum actionType, string filePath, string transferText, string lineIndex = null, bool caseSensitive = false)
            : base(ActionTypeEnum.File)
        {
            this.ActionType = actionType;
            this.FilePath = filePath;
            this.TransferText = transferText;
            this.LineIndex = lineIndex;
            this.CaseSensitive = caseSensitive;
        }

        public FileActionModel(FileActionTypeEnum actionType, string filePath, string transferText, IEnumerable<ActionModelBase> actions, string lineIndex = null, bool caseSensitive = false)
            : base(ActionTypeEnum.File, actions)
        {
            this.ActionType = actionType;
            this.FilePath = filePath;
            this.TransferText = transferText;
            this.LineIndex = lineIndex;
            this.CaseSensitive = caseSensitive;
        }

        [Obsolete]
        public FileActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string filePath = await ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            filePath = filePath.ToFilePathString();

            if (this.ActionType == FileActionTypeEnum.ReadFromFile ||
                this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile ||
                this.ActionType == FileActionTypeEnum.CountLinesInFile ||
                this.ActionType == FileActionTypeEnum.ReadEachLineFromFile)
            {
                if (!ServiceManager.Get<IFileService>().IsURLPath(filePath) && !ServiceManager.Get<IFileService>().FileExists(filePath))
                {
                    Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - File Action - File does not exist: {filePath}");
                }
            }

            string textToWrite = string.Empty;
            string textToRead = string.Empty;
            List<string> lines = new List<string>();

            if (this.ActionType == FileActionTypeEnum.ReadFromFile ||
                this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.CountLinesInFile ||
                this.ActionType == FileActionTypeEnum.ReadEachLineFromFile)
            {
                parameters.SpecialIdentifiers.Remove(this.TransferText);
            }

            if (this.ActionType == FileActionTypeEnum.SaveToFile || this.ActionType == FileActionTypeEnum.AppendToFile ||
                this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile)
            {
                textToWrite = await this.GetTextToSave(parameters);
            }

            if (this.ActionType == FileActionTypeEnum.ReadFromFile || this.ActionType == FileActionTypeEnum.AppendToFile ||
                this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile ||
                this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine ||
                this.ActionType == FileActionTypeEnum.CountLinesInFile ||
                this.ActionType == FileActionTypeEnum.ReadEachLineFromFile)
            {
                textToRead = await ServiceManager.Get<IFileService>().ReadFile(filePath);
            }

            if (this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile ||
                this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine ||
                this.ActionType == FileActionTypeEnum.CountLinesInFile ||
                this.ActionType == FileActionTypeEnum.ReadEachLineFromFile)
            {
                if (!string.IsNullOrEmpty(textToRead))
                {
                    lines = new List<string>(textToRead.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None));
                    if (lines.Count > 0 && string.IsNullOrEmpty(lines.Last()))
                    {
                        lines.RemoveAt(lines.Count - 1);
                    }
                }

                if (this.ActionType == FileActionTypeEnum.CountLinesInFile)
                {
                    textToRead = lines.Count().ToString();
                }
                else if (this.ActionType == FileActionTypeEnum.ReadEachLineFromFile)
                {
                    foreach (string line in lines)
                    {
                        parameters.SpecialIdentifiers[this.TransferText] = line;
                        await this.RunSubActions(parameters);
                    }
                }
                else
                {
                    int lineIndex = 0;
                    if (this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile ||
                        this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine)
                    {
                        string lineToRead = await ReplaceStringWithSpecialModifiers(this.LineIndex, parameters);
                        if (!int.TryParse(lineToRead, out lineIndex))
                        {
                            return;
                        }
                        lineIndex = lineIndex - 1;
                    }
                    else if (this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                        this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine)
                    {
                        lineIndex = RandomHelper.GenerateRandomNumber(lines.Count);
                    }
                    else if (this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile)
                    {
                        lineIndex = lines.FindIndex(l => l.Equals(textToWrite, this.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
                    }

                    if (lineIndex >= 0)
                    {
                        if (this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine)
                        {
                            if (this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine)
                            {
                                lineIndex = Math.Min(lines.Count, lineIndex);
                            }
                            lines.Insert(lineIndex, textToWrite);
                        }
                        else
                        {
                            if (lines.Count > lineIndex)
                            {
                                if (this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                                    this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile || this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                                {
                                    textToRead = lines[lineIndex];
                                }

                                if (this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile || this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile ||
                                    this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile)
                                {
                                    lines.RemoveAt(lineIndex);
                                }
                            }
                            else
                            {
                                textToRead = null;
                            }
                        }
                    }
                }
            }

            if (this.ActionType == FileActionTypeEnum.ReadFromFile ||
                this.ActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.ActionType == FileActionTypeEnum.ReadRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.CountLinesInFile)
            {
                if (!string.IsNullOrEmpty(textToRead))
                {
                    parameters.SpecialIdentifiers[this.TransferText] = textToRead;
                }
            }

            if (this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile ||
                this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine)
            {
                textToWrite = string.Join(Environment.NewLine, lines);
            }

            if (this.ActionType == FileActionTypeEnum.SaveToFile ||
                this.ActionType == FileActionTypeEnum.RemoveSpecificLineFromFile || this.ActionType == FileActionTypeEnum.RemoveRandomLineFromFile ||
                this.ActionType == FileActionTypeEnum.RemoveLineWithSpecificTextFromFile ||
                this.ActionType == FileActionTypeEnum.InsertInFileAtSpecificLine || this.ActionType == FileActionTypeEnum.InsertInFileAtRandomLine)
            {
                await ServiceManager.Get<IFileService>().SaveFile(filePath, textToWrite);
            }

            if (this.ActionType == FileActionTypeEnum.AppendToFile)
            {
                if (!string.IsNullOrEmpty(textToRead))
                {
                    textToWrite = Environment.NewLine + textToWrite;
                }
                await ServiceManager.Get<IFileService>().AppendFile(filePath, textToWrite);
            }
        }

        private async Task<string> GetTextToSave(CommandParametersModel parameters)
        {
            string textToWrite = (!string.IsNullOrEmpty(this.TransferText)) ? this.TransferText : string.Empty;
            return await ReplaceStringWithSpecialModifiers(textToWrite, parameters);
        }
    }
}
