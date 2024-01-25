using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SpecialIdentifierActionModel : ActionModelBase
    {
        [DataMember]
        public string SpecialIdentifierName { get; set; }

        [DataMember]
        public string ReplacementText { get; set; }

        [DataMember]
        public bool MakeGloballyUsable { get; set; }

        [DataMember]
        public bool ShouldProcessMath { get; set; }

        public SpecialIdentifierActionModel(string specialIdentifierName, string replacementText, bool makeGloballyUsable, bool shouldProcessMath)
            : base(ActionTypeEnum.SpecialIdentifier)
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.ReplacementText = replacementText;
            this.MakeGloballyUsable = makeGloballyUsable;
            this.ShouldProcessMath = shouldProcessMath;
        }

        [Obsolete]
        public SpecialIdentifierActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string replacementText = await ReplaceStringWithSpecialModifiers(this.ReplacementText, parameters);

            if (replacementText.Contains("(") || replacementText.Contains(")"))
            {
                replacementText = await this.ProcessStringFunction(replacementText, "removespaces", (text) => { return Task.FromResult(text.Replace(" ", string.Empty)); });
                replacementText = await this.ProcessStringFunction(replacementText, "removecommas", (text) => { return Task.FromResult(text.Replace(",", string.Empty)); });
                replacementText = await this.ProcessStringFunction(replacementText, "tolower", (text) => { return Task.FromResult(text.ToLower()); });
                replacementText = await this.ProcessStringFunction(replacementText, "toupper", (text) => { return Task.FromResult(text.ToUpper()); });
                replacementText = await this.ProcessStringFunction(replacementText, "length", (text) => { return Task.FromResult(text.Length.ToString()); });
                replacementText = await this.ProcessStringFunction(replacementText, "urlencode", (text) => { return Task.FromResult(HttpUtility.UrlEncode(text)); });
                replacementText = await this.ProcessStringFunction(replacementText, "uriescape", (text) => { return Task.FromResult(Uri.EscapeDataString(text)); });
                replacementText = await this.ProcessStringFunction(replacementText, "replace", (text) =>
                {
                    string[] splits = text.Split(new char[] { ',' });
                    if (splits != null && splits.Length == 3)
                    {
                        return Task.FromResult(splits[0].Replace(splits[1], splits[2]));
                    }
                    return Task.FromResult(text);
                });
                replacementText = await this.ProcessStringFunction(replacementText, "count", (text) =>
                {
                    string[] splits = text.Split(new char[] { ',' });
                    if (splits != null && splits.Length == 2)
                    {
                        return Task.FromResult(Regex.Matches(splits[0], splits[1]).Count.ToString());
                    }
                    return Task.FromResult(text);
                });
            }

            if (this.ShouldProcessMath)
            {
                replacementText = MathHelper.ProcessMathEquation(replacementText).ToString();
            }

            if (this.MakeGloballyUsable)
            {
                SpecialIdentifierStringBuilder.AddGlobalSpecialIdentifier(this.SpecialIdentifierName, replacementText);
            }
            else
            {
                parameters.SpecialIdentifiers[this.SpecialIdentifierName] = replacementText;
            }
        }

        private async Task<string> ProcessStringFunction(string text, string functionName, Func<string, Task<string>> processor)
        {
            int index = 0;
            while (index >= 0)
            {
                index = text.IndexOf(functionName + "(");
                if (index >= 0)
                {
                    int functionStartIndex = index + functionName.Length;
                    int rightIndex = functionStartIndex;

                    while (rightIndex >= 0 && rightIndex < text.Length)
                    {
                        int searchRightIndex = text.IndexOf(")", rightIndex);
                        if (rightIndex >= 0)
                        {
                            rightIndex = searchRightIndex;

                            int leftCount = text.Substring(functionStartIndex, rightIndex - functionStartIndex).Count(c => c == '(');
                            int rightCount = text.Substring(functionStartIndex, rightIndex - functionStartIndex).Count(c => c == ')');

                            if (leftCount == (rightCount + 1))
                            {
                                // Successful match, reset and check again
                                text = await this.PerformStringFunction(text, functionName, processor, index, rightIndex);

                                rightIndex = -1;
                            }
                            else
                            {
                                // Too many left (, expand outward
                                rightIndex++;
                            }
                        }
                        else
                        {
                            // No matching right ), fail out
                            rightIndex = -1;
                            index = -1;
                        }
                    }

                    if (rightIndex >= 0)
                    {
                        // We've reached the end of the text, find the last ) that exists
                        rightIndex = text.LastIndexOf(")", rightIndex);

                        text = await this.PerformStringFunction(text, functionName, processor, index, rightIndex);
                    }
                }
            }
            return text;
        }

        private async Task<string> PerformStringFunction(string text, string functionName, Func<string, Task<string>> processor, int startIndex, int endIndex)
        {
            string functionText = text.Substring(startIndex, endIndex - startIndex + 1);
            string textToProcess = functionText.Substring(functionName.Length + 1, functionText.Length - functionName.Length - 2);
            return text.Replace(functionText, await processor(textToProcess));
        }
    }
}
