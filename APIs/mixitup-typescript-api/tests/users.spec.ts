import { Users, IUser } from "../lib/index";

import { expect } from "chai";
import "mocha";

describe("Users client function test", async () => {
    let userId: number = 0;
    let currencyID: string = "";

    it("Should get user by name", async () => {
        const result: IUser = await Users.getUserByUserNameAsync("TyrenBot");

        expect(result).to.not.be.equal(null);

        console.log("\tGot User: " + JSON.stringify(result));
        userId = result.ID;
        currencyID = result.CurrencyAmounts[0].ID;
    });

    it("Should get user by id", async () => {
        const result: IUser = await Users.getUserByUserIdAsync(userId);

        expect(result).to.not.be.equal(null);

        console.log("\tGot User: " + JSON.stringify(result));
    });

    it("Should update", async () => {
        const result: IUser = await Users.getUserByUserIdAsync(userId);

        expect(result).to.not.be.equal(null);

        const beforeUpdate: number = result.CurrencyAmounts[0].Amount;
        result.CurrencyAmounts[0].Amount += 100;

        const updated: IUser = await Users.updateUserAsync(result);
        expect(updated).to.not.be.equal(null);
        expect(updated.CurrencyAmounts[0].Amount).to.not.be.equal(beforeUpdate);

        console.log("\tGot User: " + JSON.stringify(updated));
    });

    it("Should Get Users bulk", async () => {
        const result: IUser[] = await Users.getUsersAsync(["TyrenDes", "TyrenBot"]);

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot User: " + JSON.stringify(result[0]));
    });

    it("Should Get top Users", async () => {
        const result: IUser[] = await Users.getTopUsersByViewingTimeAsync(5);

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot User: " + JSON.stringify(result[0]));
    });

    it("Should adjust currency by username", async () => {
        const result: IUser = await Users.adjustUserCurrencyByUserNameAsync("TyrenBot", currencyID, 5);

        expect(result).to.not.be.equal(null);

        console.log("\tGot User: " + JSON.stringify(result));
    });

    it("Should adjust currency by userid", async () => {
        const result: IUser = await Users.adjustUserCurrencyByUserIdAsync(userId, currencyID, 5);

        expect(result).to.not.be.equal(null);

        console.log("\tGot User: " + JSON.stringify(result));
    });
});
