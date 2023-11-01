using MixItUp.API.V1.Models;
using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V1
{
    [RoutePrefix("api/commands")]
    public class CommandV1Controller : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Command> Get()
        {
            return GetAllCommands();
        }

        [Route("{commandID:guid}")]
        [HttpGet]
        public Command Get(Guid commandID)
        {
            CommandModelBase selectedCommand = FindCommand(commandID);
            if (selectedCommand == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find command: {commandID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Command ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return CommandFromCommandBase(selectedCommand);
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public Command Run(Guid commandID, [FromBody] IEnumerable<string> arguments)
        {
            CommandModelBase selectedCommand = FindCommand(commandID);
            if (selectedCommand == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find command: {commandID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Command ID not found"
                };
                throw new HttpResponseException(resp);
            }

            AsyncRunner.RunAsyncBackground((cancellationToken) => ServiceManager.Get<CommandService>().Queue(
                    selectedCommand,
                    new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.All, arguments)
                    {
                        IgnoreRequirements = true
                    }),
                new CancellationToken());

            return CommandFromCommandBase(selectedCommand);
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public Command Update(Guid commandID, [FromBody] Command commandData)
        {
            CommandModelBase selectedCommand = FindCommand(commandID);
            if (selectedCommand == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find command: {commandID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Command ID not found"
                };
                throw new HttpResponseException(resp);
            }

            selectedCommand.IsEnabled = commandData.IsEnabled;
            return CommandFromCommandBase(selectedCommand);
        }

        private CommandModelBase FindCommand(Guid commandId)
        {
            if (ChannelSession.Settings.Commands.TryGetValue(commandId, out CommandModelBase command))
            {
                return command;
            }
            return null;
        }

        private List<Command> GetAllCommands()
        {
            List<Command> allCommands = new List<Command>();

            foreach (CommandModelBase command in ServiceManager.Get<CommandService>().AllCommands)
            {
                allCommands.Add(CommandFromCommandBase(command));
            }

            return allCommands;
        }

        private Command CommandFromCommandBase(CommandModelBase baseCommand)
        {
            return new Command
            {
                ID = baseCommand.ID,
                Name = baseCommand.Name,
                IsEnabled = baseCommand.IsEnabled,
                Category = Resources.ResourceManager.GetString(baseCommand.Type.ToString()),
                GroupName = baseCommand.GroupName ?? string.Empty,
            };
        }
    }
}
