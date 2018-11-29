import { RestClient } from "./restclient";
import { ICurrency } from "./models/currency";
import { IUser } from "./models/user";
import { IGiveUserCurrency } from "./models/giveusercurrency";

export class Currencies {
    public static async getAllCurrenciesAsync(): Promise<ICurrency[]> {
        return await RestClient.GetAsync<ICurrency[]>("currency");
    }

    public static async getTopUsersByCurrencyAsync(currencyID: string, count?: number): Promise<IUser[]> {
        let uri: string = `currency/${currencyID}/top`;
        if (count !== undefined) {
            uri += `?count=${count}`;
        }

        return await RestClient.GetAsync<IUser[]>(uri);
    }

    public static async giveUsersCurrencyAsync(currencyID: string, giveList: IGiveUserCurrency[]): Promise<IUser[]> {
        if (giveList.length === 0) {
            return [];
        }

        return await RestClient.PostAsync<IUser[]>(`currency/${currencyID}/give`, giveList);
    }
}
