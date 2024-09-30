using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum ArgumentsRequirementItemTypeEnum
    {
        User,
        Number,
        Decimal,
        Text
    }

    [DataContract]
    public class ArgumentsRequirementItemModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ArgumentsRequirementItemTypeEnum Type { get; set; }

        [DataMember]
        public bool Optional { get; set; }

        public ArgumentsRequirementItemModel(string name, ArgumentsRequirementItemTypeEnum type, bool optional)
        {
            this.Name = name;
            this.Type = type;
            this.Optional = optional;
        }

        [Obsolete]
        public ArgumentsRequirementItemModel() { }

        public string DisplayName
        {
            get
            {
                string display = this.Name;
                if (this.Type == ArgumentsRequirementItemTypeEnum.User)
                {
                    display = "@" + display;
                }
                return this.Optional ? $"[{display}]" : $"<{display}>";
            }
        }

        public Task<Result> Validate(CommandParametersModel parameters, string argument)
        {
            if (this.Optional && string.IsNullOrEmpty(argument))
            {
                return Task.FromResult<Result>(new Result());
            }

            if (this.Type == ArgumentsRequirementItemTypeEnum.User)
            {
                if (!string.IsNullOrEmpty(argument))
                {
                    UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(parameters.Platform, platformUsername: argument);
                    if (user != null)
                    {
                        return Task.FromResult<Result>(new Result());
                    }
                    return Task.FromResult<Result>(new Result(argument[0] == '@'));
                }
            }
            else if (this.Type == ArgumentsRequirementItemTypeEnum.Number)
            {
                return Task.FromResult<Result>(new Result(int.TryParse(argument, out int n)));
            }
            else if (this.Type == ArgumentsRequirementItemTypeEnum.Decimal)
            {
                return Task.FromResult<Result>(new Result(double.TryParse(argument, out double d)));
            }
            else if (this.Type == ArgumentsRequirementItemTypeEnum.Text)
            {
                return Task.FromResult<Result>(new Result(!string.IsNullOrEmpty(argument)));
            }
            return Task.FromResult<Result>(new Result(success: false));
        }
    }

    [DataContract]
    public class ArgumentsRequirementModel : RequirementModelBase
    {
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public List<ArgumentsRequirementItemModel> Items { get; set; } = new List<ArgumentsRequirementItemModel>();

        [DataMember]
        public bool AssignToSpecialIdentifiers { get; set; }

        public ArgumentsRequirementModel(IEnumerable<ArgumentsRequirementItemModel> items, bool assignToSpecialIdentifiers)
        {
            this.Items = new List<ArgumentsRequirementItemModel>(items);
            this.AssignToSpecialIdentifiers = assignToSpecialIdentifiers;
        }

        public ArgumentsRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return ArgumentsRequirementModel.requirementErrorCooldown; } set { ArgumentsRequirementModel.requirementErrorCooldown = value; } }

        public override async Task<Result> Validate(CommandParametersModel parameters)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                parameters.Arguments.TryGetValue(i, out string argument);

                ArgumentsRequirementItemModel argumentItem = this.Items[i];
                Result result = await argumentItem.Validate(parameters, argument);
                if (!result.Success)
                {
                    return new Result($"{MixItUp.Base.Resources.Usage}: {string.Join(" ", this.Items.Select(a => a.DisplayName))}");
                }

                if (this.AssignToSpecialIdentifiers)
                {
                    parameters.SpecialIdentifiers[SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(argumentItem.Name)] = argument;
                }
            }
            return new Result();
        }
    }
}
