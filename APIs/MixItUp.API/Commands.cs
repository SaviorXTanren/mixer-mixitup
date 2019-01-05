using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/commands              Commands.GetAllCommandsAsync
    //  GET:    http://localhost:8911/api/commands/{commandID}  Commands.GetCommandAsync
    //  POST:   http://localhost:8911/api/commands/{commandID}  Commands.RunCommandAsync
    //  PUT:    http://localhost:8911/api/commands/{commandID}  Commands.UpdateCommandAsync
    public static class Commands
    {
        public static async Task<Command[]> GetAllCommandsAsync()
        {
            return await RestClient.GetAsync<Command[]>($"commands");
        }

        public static async Task<Command> GetCommandAsync(Guid commandID)
        {
            return await RestClient.GetAsync<Command>($"commands/{commandID}");
        }

        public static async Task RunCommandAsync(Guid commandID)
        {
            await RunCommandAsync(commandID, null);
        }

        public static async Task RunCommandAsync(Guid commandID, IEnumerable<string> arguments)
        {
            await RestClient.PostAsync($"commands/{commandID}", arguments);
        }

        public static async Task<Command> UpdateCommandAsync(Command updatedCommand)
        {
            return await RestClient.PutAsync<Command>($"commands/{updatedCommand.ID}", updatedCommand);
        }
    }
}
