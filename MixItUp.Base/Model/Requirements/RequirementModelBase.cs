using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum RequirementErrorCooldownTypeEnum
    {
        Default,
        PerCommand,
        PerRequirement,
        Singular,
        None,
    }

    [DataContract]
    public abstract class RequirementModelBase
    {
        public static DateTimeOffset GlobalErrorCooldown { get; set; } = DateTimeOffset.MinValue;

        public static DateTimeOffset UpdateErrorCooldown() { return DateTimeOffset.Now.AddSeconds(ChannelSession.Settings.RequirementErrorsCooldownAmount); }

        public static async Task SendRequirementErrorMessage(UserV2ViewModel user, Result result)
        {
            string message = result.ToString();
            if (ChannelSession.Settings.IncludeUsernameWithRequirementErrors)
            {
                message = $"@{user.Username}: {message}";
            }
            await ServiceManager.Get<ChatService>().SendMessage(message, user.Platform);
        }

        protected DateTimeOffset individualErrorCooldown = DateTimeOffset.MinValue;

        protected abstract DateTimeOffset RequirementErrorCooldown { get; set; }

        public virtual Task<Result> Validate(CommandParametersModel parameters) { return Task.FromResult(new Result()); }

        public virtual Task Perform(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.Default)
            {
                this.individualErrorCooldown = DateTimeOffset.Now;
            }
            return Task.CompletedTask;
        }

        public virtual Task Refund(CommandParametersModel parameters) { return Task.CompletedTask; }

        public virtual void Reset() { }

        public void SetIndividualErrorCooldown(DateTimeOffset datetime) { this.individualErrorCooldown = datetime; }

        public async Task SendErrorChatMessage(UserV2ViewModel user, Result result)
        {
            if (ChannelSession.Settings.RequirementErrorsCooldownType != RequirementErrorCooldownTypeEnum.None)
            {
                bool sendError = false;
                if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.Default || ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.PerCommand)
                {
                    sendError = this.individualErrorCooldown <= DateTimeOffset.Now;
                }
                else if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.PerRequirement)
                {
                    sendError = this.RequirementErrorCooldown <= DateTimeOffset.Now;
                }
                else if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.Singular)
                {
                    sendError = RequirementModelBase.GlobalErrorCooldown <= DateTimeOffset.Now;
                }

                if (sendError)
                {
                    await RequirementModelBase.SendRequirementErrorMessage(user, result);

                    if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.Default)
                    {
                        this.individualErrorCooldown = RequirementModelBase.UpdateErrorCooldown();
                    }
                    else if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.PerRequirement)
                    {
                        this.RequirementErrorCooldown = RequirementModelBase.UpdateErrorCooldown();
                    }
                    else if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.Singular)
                    {
                        RequirementModelBase.GlobalErrorCooldown = RequirementModelBase.UpdateErrorCooldown();
                    }
                }
            }
        }
    }
}
