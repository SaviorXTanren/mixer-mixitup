﻿using MixItUp.Base.Commands;
using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/commands")]
    public class CommandController : BaseController
    {
        [Route("")]
        [HttpGet]
        public IEnumerable<Command> Get()
        {
            return GetAllCommands();
        }

        [Route("{commandID:guid}")]
        [HttpGet]
        public IActionResult Get(Guid commandID)
        {
            Command selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find command: {commandID.ToString()}." });
            }

            return Ok(selectedCommand);
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public IActionResult Run(Guid commandID, [FromBody] IEnumerable<string> arguments)
        {
            CommandBase selectedCommand = FindCommand(commandID, out string category);
            if (selectedCommand == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find command: {commandID.ToString()}." });
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            selectedCommand.Perform(arguments: arguments);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return Ok(CommandFromCommandBase(selectedCommand, category));
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public IActionResult Update(Guid commandID, [FromBody] Command commandData)
        {
            CommandBase selectedCommand = FindCommand(commandID, out string category);
            if (selectedCommand == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find command: {commandID.ToString()}." });
            }

            selectedCommand.IsEnabled = commandData.IsEnabled;
            return Ok(CommandFromCommandBase(selectedCommand, category));
        }

        private CommandBase FindCommand(Guid commandId, out string category)
        {
            category = null;
            CommandBase command = ChannelSession.Settings.ChatCommands.SingleOrDefault(c => c.ID == commandId);
            if (command !=null)
            {
                category = "Chat";
                return command;
            }

            command = ChannelSession.Settings.MixPlayCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "MixPlay";
                return command;
            }

            command = ChannelSession.Settings.EventCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Event";
                return command;
            }

            command = ChannelSession.Settings.TimerCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "Timer";
                return command;
            }

            command = ChannelSession.Settings.ActionGroupCommands.SingleOrDefault(c => c.ID == commandId);
            if (command != null)
            {
                category = "ActionGroup";
                return command;
            }

            command = ChannelSession.Settings.GameCommands.SingleOrDefault(c => c.ID == commandId);
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
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.ChatCommands, "Chat"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.MixPlayCommands, "Interactive"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.EventCommands, "Event"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.TimerCommands, "Timer"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.ActionGroupCommands, "ActionGroup"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.Settings.GameCommands, "Game"));
            allCommands.AddRange(CommandsFromCommandBases(ChannelSession.PreMadeChatCommands, "Pre-Made"));
            return allCommands;
        }

        private IEnumerable<Command> CommandsFromCommandBases(IEnumerable<CommandBase> baseCommands, string category)
        {
            foreach(CommandBase baseCommand in baseCommands)
            {
                yield return CommandFromCommandBase(baseCommand, category);
            }
        }

        private Command CommandFromCommandBase(CommandBase baseCommand, string category)
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
