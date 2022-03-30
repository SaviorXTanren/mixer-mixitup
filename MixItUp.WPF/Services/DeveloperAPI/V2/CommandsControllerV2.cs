using MixItUp.API.V2.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/commands")]
    public class CommandsControllerV2 : ApiController
    {
        // Get Command
        // Must have all command data (triggers, costs, restrictions, etc)
        // Run Command
        // Update Command
        // Delete Command?
        // Create Command?

        [Route("{commandId:guid}")]
        [HttpGet]
        public IHttpActionResult GetCommandById(Guid commandId)
        {
            if (!ChannelSession.Settings.Commands.TryGetValue(commandId, out var command) || command == null)
            {
                return NotFound();
            }

            return Ok(new GetSingleCommandResponse { Command = CommandMapper.ToCommand(command) });
        }

        [Route]
        [HttpGet]
        public IHttpActionResult GetAllCommands(int skip = 0, int pageSize = 25)
        {
            var allCommands = GetAllCommands();
            var commands = allCommands
                .OrderBy(c => c.ID)
                .Skip(skip)
                .Take(pageSize);

            var result = new GetListOfCommandsResponse();
            result.TotalCount = allCommands.Count();
            foreach (var command in commands)
            {
                result.Commands.Add(CommandMapper.ToCommand(command));
            }

            return Ok(result);
        }

        [Route("{commandId:guid}/state/{state:int}")]
        [HttpPatch]
        public async Task<IHttpActionResult> UpdateCommandState(Guid commandId, CommandStateOptions state)
        {
            if (!ChannelSession.Settings.Commands.TryGetValue(commandId, out var command) || command == null)
            {
                return NotFound();
            }

            if (state == CommandStateOptions.Disable)
            {
                command.IsEnabled = false;
            }
            else if (state == CommandStateOptions.Enable)
            {
                command.IsEnabled = true;
            }
            else if (state == CommandStateOptions.Toggle)
            {
                command.IsEnabled = !command.IsEnabled;
            }
            else
            {
                return BadRequest();
            }

            if (command is ChatCommandModel)
            {
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else if (command is TimerCommandModel)
            {
                await ServiceManager.Get<TimerService>().RebuildTimerGroups();
            }

            return Ok(new GetSingleCommandResponse { Command = CommandMapper.ToCommand(command) });
        }

        private IEnumerable<CommandModelBase> GetAllCommands()
        {
            IEnumerable<CommandModelBase> commands = new List<CommandModelBase>(ServiceManager.Get<CommandService>().AllCommands);
            commands = commands.Where(c => !c.IsEmbedded && c.Type != CommandTypeEnum.Custom);
            return commands;
        }
    }

    public static class CommandMapper
    {
        public static Command ToCommand(CommandModelBase command)
        {
            return new Command
            {
                ID = command.ID,
                Name = command.Name,
                Type = command.Type.ToString(),
                IsEnabled = command.IsEnabled,
                Unlocked = command.Unlocked,
                GroupName = command.GroupName,
            };
        }
    }
}
