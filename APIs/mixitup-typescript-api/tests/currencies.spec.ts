import { Currencies, ICurrency, IUser, IGiveUserCurrency } from "../lib/index";

import { expect } from "chai";
import "mocha";

describe("Currency client function test", async () => {
    let currencyId: string = "";
    it("Should get currencies", async () => {
        const result: ICurrency[] = await Currencies.getAllCurrenciesAsync();

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot Currency: " + JSON.stringify(result[0]));
        currencyId = result[0].ID;
    });

    it("Should get top users", async () => {
        const result: IUser[] = await Currencies.getTopUsersByCurrencyAsync(currencyId, 5);

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot User: " + JSON.stringify(result[0]));
    });

    it("Should give to user", async () => {
        const giveList: IGiveUserCurrency[] = [
            { UsernameOrID: "TyrenBot", Amount: 10 }
        ];

        const result: IUser[] = await Currencies.giveUsersCurrencyAsync(currencyId, giveList);

        expect(result).to.not.be.equal(null);
        expect(result.length).to.not.be.equal(0);

        console.log("\tGot User: " + JSON.stringify(result[0]));
    });
});
