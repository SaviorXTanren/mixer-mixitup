import { RestClient } from "./restclient";
import { ICommand } from "./models/command";

export class Commands {
    public static async getAllCommandsAsync(): Promise<ICommand[]> {
        return await RestClient.GetAsync<ICommand[]>("commands");
    }

    public static async getCommandAsync(commandID: string): Promise<ICommand> {
        return await RestClient.GetAsync<ICommand>(`commands/${commandID}`);
    }

    public static async runCommandAsync(commandID: string): Promise<{}> {
        return await RestClient.PostAsync<{}>(`commands/${commandID}`, null);
    }

    public static async updateCommandAsync(updatedCommand: ICommand): Promise<ICommand> {
        return await RestClient.PutAsync<ICommand>(`commands/${updatedCommand.ID}`, updatedCommand);
    }
}
