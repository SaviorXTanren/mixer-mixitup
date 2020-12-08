using MixItUp.API.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            selectedCommand.Perform(new CommandParametersModel(null, arguments));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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
            CommandModelBase command = ChannelSession.ChatCommands.SingleOrDefault(c => c.ID == commandId);
            if (command !=null)
            {
                category = "Chat";
                return command;
            }

            command = ChannelSession.EventCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Event";
                return command;
            }

            command = ChannelSession.TimerCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Timer";
                return command;
            }

            command = ChannelSession.TwitchChannelPointsCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "ChannelPoints";
                return command;
            }

            command = ChannelSession.ActionGroupCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "ActionGroup";
                return command;
            }

            command = ChannelSession.GameCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Game";
                return command;
            }

            command = ChannelSession.PreMadeChatCommands.SingleOrDefault(c => c.ID == commandId);
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
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.ChatCommands, "Chat"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.EventCommands, "Event"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.TimerCommands, "Timer"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.TwitchChannelPointsCommands, "ChannelPoints"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.ActionGroupCommands, "ActionGroup"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.GameCommands, "Game"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.PreMadeChatCommands, "Pre-Made"));
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
