using MixItUp.Base;
using MixItUp.Base.Commands;
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
        public IEnumerable<CommandBase> Get()
        {
            return GetAllCommands();
        }

        [Route("{commandID:guid}")]
        [HttpGet]
        public CommandBase Get(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return selectedCommand;
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public CommandBase Run(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            selectedCommand.Perform();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return selectedCommand;
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public CommandBase Update(Guid commandID, [FromBody] CommandBase commandData)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            selectedCommand.IsEnabled = commandData.IsEnabled;
            return selectedCommand;
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
    }
}
