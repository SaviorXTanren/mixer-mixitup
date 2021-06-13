using MixItUp.API.Models;
using MixItUp.Base;
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

namespace MixItUp.WPF.Services.DeveloperAPI
{
    [RoutePrefix("api/commands")]
    public class CommandController : ApiController
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
            Command selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find command: {commandID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Command ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return selectedCommand;
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public Command Run(Guid commandID, [FromBody] IEnumerable<string> arguments)
        {
            CommandModelBase selectedCommand = FindCommand(commandID, out string category);
            if (selectedCommand == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find command: {commandID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Command ID not found"
                };
                throw new HttpResponseException(resp);
            }

            AsyncRunner.RunAsyncBackground((cancellationToken) => ServiceManager.Get<CommandService>().Queue(selectedCommand, new CommandParametersModel(user: null, arguments)), new CancellationToken());

            return CommandFromCommandBase(selectedCommand, category);
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public Command Update(Guid commandID, [FromBody] Command commandData)
        {
            CommandModelBase selectedCommand = FindCommand(commandID, out string category);
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
            return CommandFromCommandBase(selectedCommand, category);
        }

        private CommandModelBase FindCommand(Guid commandId, out string category)
        {
            category = null;
            CommandModelBase command = ChannelSession.Services.Command.ChatCommands.SingleOrDefault(c => c.ID == commandId);
            if (command !=null)
            {
                category = "Chat";
                return command;
            }

            command = ChannelSession.Services.Command.EventCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Event";
                return command;
            }

            command = ChannelSession.Services.Command.TimerCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Timer";
                return command;
            }

            command = ChannelSession.Services.Command.TwitchChannelPointsCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "ChannelPoints";
                return command;
            }

            command = ChannelSession.Services.Command.ActionGroupCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "ActionGroup";
                return command;
            }

            command = ChannelSession.Services.Command.GameCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Game";
                return command;
            }

            command = ChannelSession.Services.Command.PreMadeChatCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Pre-Made";
                return command;
            }

            return null;
        }

        private List<Command> GetAllCommands()
        {
            List<Command> allCommands = new List<Command>();
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.ChatCommands, "Chat"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.EventCommands, "Event"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.TimerCommands, "Timer"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.TwitchChannelPointsCommands, "ChannelPoints"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.ActionGroupCommands, "ActionGroup"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.GameCommands, "Game"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Services.Command.PreMadeChatCommands, "Pre-Made"));
            return allCommands;
        }

        private IEnumerable<Command> CommandsFromCommandBases(IEnumerable<CommandModelBase> baseCommands, string category)
        {
            foreach (CommandModelBase baseCommand in baseCommands)
            {
                yield return CommandFromCommandBase(baseCommand, category);
            }
        }

        private Command CommandFromCommandBase(CommandModelBase baseCommand, string category)
        {
            return new Command
            {
                ID = baseCommand.ID,
                Name = baseCommand.Name,
                IsEnabled = baseCommand.IsEnabled,
                Category = category,
                GroupName = baseCommand.GroupName ?? string.Empty,
            };
        }
    }
}
