using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/commands")]
    public class CommandController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Command> Get()
        {
            var allCommands = GetAllCommands();

            List<Command> commands = new List<Command>();
            foreach (var command in allCommands)
            {
                commands.Add(CommandFromCommandBase(command));
            }

            return commands;
        }

        [Route("{commandID:guid}")]
        [HttpGet]
        public Command Get(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return CommandFromCommandBase(selectedCommand);
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public Command Run(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            selectedCommand.Perform();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return CommandFromCommandBase(selectedCommand);
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public Command Update(Guid commandID, [FromBody] CommandBase commandData)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            selectedCommand.IsEnabled = commandData.IsEnabled;
            return CommandFromCommandBase(selectedCommand);
        }

        private List<CommandBase> GetAllCommands()
        {
            List<CommandBase> allCommands = new List<CommandBase>();
            allCommands.AddRange(ChannelSession.Settings.ChatCommands);
            allCommands.AddRange(ChannelSession.Settings.InteractiveCommands);
            allCommands.AddRange(ChannelSession.Settings.EventCommands);
            allCommands.AddRange(ChannelSession.Settings.TimerCommands);
            allCommands.AddRange(ChannelSession.Settings.ActionGroupCommands);
            allCommands.AddRange(ChannelSession.Settings.GameCommands);
            return allCommands;
        }

        private Command CommandFromCommandBase(CommandBase baseCommand)
        {
            return new Command
            {
                ID = baseCommand.ID,
                Name = baseCommand.Name,
                IsEnabled = baseCommand.IsEnabled
            };
        }
    }
}
