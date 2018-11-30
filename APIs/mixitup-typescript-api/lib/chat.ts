import { RestClient } from "./restclient";
import { IUser } from "./models/user";
import { ISendChatMessage } from "./models/sendchatmessage";

export class Chat {
    public static async getAllUsersAsync(): Promise<IUser[]> {
        return await RestClient.GetAsync<IUser[]>("chat/users");
    }

    public static async clearChatAsync(): Promise<{}> {
        return await RestClient.DeleteAsync("chat/message");
    }

    public static async sendMessageAsync(message: string, sendAsStreamer: boolean): Promise<{}> {
        var model: ISendChatMessage = {
            Message: message,
            SendAsStreamer: sendAsStreamer
        };

        return await RestClient.PostAsync("chat/message", model);
    }
}
