import { Commands, ICommand } from "../lib/index";

import { expect } from "chai";
import "mocha";

describe("Commands client function test", async () => {
    let commandId: string = "";

    it("Should get commands", async () => {
        const result: ICommand[] = await Commands.getAllCommandsAsync();

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot Command: " + JSON.stringify(result[0]));
        commandId = result[0].ID;
    });

    it("Should update", async () => {
        const result: ICommand = await Commands.getCommandAsync(commandId);

        expect(result).to.not.be.equal(null);

        const beforeIsEnabled: boolean = result.IsEnabled;
        result.IsEnabled = !result.IsEnabled;

        const updated: ICommand = await Commands.updateCommandAsync(result);
        expect(updated).to.not.be.equal(null);
        expect(updated.IsEnabled).to.not.be.equal(beforeIsEnabled);

        console.log("\tGot Command: " + JSON.stringify(updated));
    });

    it("Should run", async () => {
        await Commands.runCommandAsync(commandId);
    });
});
