import { Chat, IUser } from "../lib/index";

import { expect } from "chai";
import "mocha";

describe("Chat client function test", async () => {
    it("Should get Users", async () => {
        const result: IUser[] = await Chat.getAllUsersAsync();

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot User: " + JSON.stringify(result[0]));
    });

    it("Should send message", async () => {
        await Chat.sendMessageAsync("A message was sent.", true);
    });

    it("Should clear messages", async () => {
        await Chat.clearChatAsync();
    });
});
