using Jace;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SpecialIdentifierActionModel : ActionModelBase
    {
        private const string TextProcessorFunctionRegexPatternFormat = "{0}\\([^)]+\\)";

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

        internal SpecialIdentifierActionModel(MixItUp.Base.Actions.SpecialIdentifierAction action)
            : base(ActionTypeEnum.SpecialIdentifier)
        {
            this.SpecialIdentifierName = action.SpecialIdentifierName;
            this.ReplacementText = action.SpecialIdentifierReplacement;
            this.MakeGloballyUsable = action.MakeGloballyUsable;
            this.ShouldProcessMath = action.SpecialIdentifierShouldProcessMath;
        }

        private SpecialIdentifierActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string replacementText = await this.ReplaceStringWithSpecialModifiers(this.ReplacementText, parameters);

            replacementText = await this.ProcessStringFunction(replacementText, "removespaces", (text) => { return Task.FromResult(text.Replace(" ", string.Empty)); });
            replacementText = await this.ProcessStringFunction(replacementText, "removecommas", (text) => { return Task.FromResult(text.Replace(",", string.Empty)); });
            replacementText = await this.ProcessStringFunction(replacementText, "tolower", (text) => { return Task.FromResult(text.ToLower()); });
            replacementText = await this.ProcessStringFunction(replacementText, "toupper", (text) => { return Task.FromResult(text.ToUpper()); });

            if (this.ShouldProcessMath)
            {
                try
                {
                    replacementText = replacementText.Replace("random(", "customrandom(");

                    // Process Math
                    CalculationEngine engine = new CalculationEngine(new System.Globalization.CultureInfo("en-US"));
                    engine.AddFunction("customrandom", Random);
                    engine.AddFunction("randomrange", RandomRange);

                    double result = engine.Calculate(replacementText);
                    replacementText = result.ToString();
                }
                catch (Exception ex)
                {
                    // Calculation failed, log and set to 0
                    Logger.Log(ex);
                    replacementText = "0";
                }
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
            foreach (Match match in Regex.Matches(text, string.Format(TextProcessorFunctionRegexPatternFormat, functionName)))
            {
                string textToProcess = match.Value.Substring(functionName.Length + 1);
                textToProcess = textToProcess.Substring(0, textToProcess.Length - 1);
                text = text.Replace(match.Value, await processor(textToProcess));
            }
            return text;
        }

        private double Random(double max)
        {
            return this.RandomRange(1, max);
        }

        private double RandomRange(double min, double max)
        {
            return RandomHelper.GenerateRandomNumber((int)min, (int)max);
        }
    }
}
