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

        [DataMember]
        public bool ReplaceSpecialIdentifiersInFunctions { get; set; }

        public SpecialIdentifierActionModel(string specialIdentifierName, string replacementText, bool makeGloballyUsable, bool shouldProcessMath, bool replaceSpecialIdentifiersInFunctions)
            : base(ActionTypeEnum.SpecialIdentifier)
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.ReplacementText = replacementText;
            this.MakeGloballyUsable = makeGloballyUsable;
            this.ShouldProcessMath = shouldProcessMath;
            this.ReplaceSpecialIdentifiersInFunctions = replaceSpecialIdentifiersInFunctions;
        }

        [Obsolete]
        public SpecialIdentifierActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string replacementText = this.ReplacementText;
            if (!this.ReplaceSpecialIdentifiersInFunctions)
            {
                replacementText = await ReplaceStringWithSpecialModifiers(replacementText, parameters);
            }

            if (replacementText.Contains("(") || replacementText.Contains(")"))
            {
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "removespaces", 1, (arguments) => { return Task.FromResult(arguments.First().Replace(" ", string.Empty)); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "removecommas", 1, (arguments) => { return Task.FromResult(arguments.First().Replace(",", string.Empty)); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "tolower", 1, (arguments) => { return Task.FromResult(arguments.First().ToLower()); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "toupper", 1, (arguments) => { return Task.FromResult(arguments.First().ToUpper()); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "length", 1, (arguments) => { return Task.FromResult(arguments.First().Length.ToString()); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "urlencode", 1, (arguments) => { return Task.FromResult(HttpUtility.UrlEncode(arguments.First())); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "urldecode", 1, (arguments) => { return Task.FromResult(HttpUtility.UrlDecode(arguments.First())); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "uriescape", 1, (arguments) => { return Task.FromResult(Uri.EscapeDataString(arguments.First())); });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "replace", 3, (arguments) =>
                {
                    if (arguments.Count() == 3)
                    {
                        return Task.FromResult(arguments.ElementAt(0).Replace(arguments.ElementAt(1), arguments.ElementAt(2)));
                    }
                    return Task.FromResult<string>(null);
                });
                replacementText = await this.ProcessStringFunction(parameters, replacementText, "count", 2, (arguments) =>
                {
                    if (arguments.Count() == 2)
                    {
                        return Task.FromResult(Regex.Matches(arguments.ElementAt(0), arguments.ElementAt(1)).Count.ToString());
                    }
                    return Task.FromResult<string>(null);
                });
            }

            if (this.ShouldProcessMath)
            {
                replacementText = MathHelper.ProcessMathEquation(replacementText).ToString();
            }

            if (this.ReplaceSpecialIdentifiersInFunctions)
            {
                replacementText = await ReplaceStringWithSpecialModifiers(replacementText, parameters);
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

        private async Task<string> ProcessStringFunction(CommandParametersModel parameters, string text, string functionName, int expectedArgumentNumber, Func<IEnumerable<string>, Task<string>> processor)
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
                                text = await this.PerformStringFunction(parameters, text, functionName, expectedArgumentNumber, processor, index, rightIndex);

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

                        text = await this.PerformStringFunction(parameters, text, functionName, expectedArgumentNumber, processor, index, rightIndex);
                    }
                }
            }
            return text;
        }

        private async Task<string> PerformStringFunction(CommandParametersModel parameters, string text, string functionName, int expectedArgumentNumber, Func<IEnumerable<string>, Task<string>> processor, int startIndex, int endIndex)
        {
            string functionText = text.Substring(startIndex, endIndex - startIndex + 1);
            string textToProcess = functionText.Substring(functionName.Length + 1, functionText.Length - functionName.Length - 2);

            List<string> arguments = new List<string>();
            if (expectedArgumentNumber > 1)
            {
                string[] splits = textToProcess.Split(new char[] { ',' });
                if (splits != null && splits.Length == expectedArgumentNumber)
                {
                    arguments.AddRange(splits);
                }
            }
            else
            {
                arguments.Add(textToProcess);
            }

            // Arguments failed to process, abort
            if (arguments.Count == 0)
            {
                return textToProcess;
            }

            if (this.ReplaceSpecialIdentifiersInFunctions)
            {
                for (int i = 0; i < arguments.Count; i++)
                {
                    arguments[i] = await ReplaceStringWithSpecialModifiers(arguments[i], parameters);
                }
            }

            text = text.Replace(functionText, await processor(arguments));
            if (text == null)
            {
                text = textToProcess;
            }
            return text;
        }
    }
}
