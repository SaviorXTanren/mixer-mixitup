import { RestClient } from "./restclient";
import { IUser } from "./models/user";
import { IAdjustCurrency } from "./models/adjustcurrency";

export class Users {
    public static async getUserByUserIdAsync(userId: number): Promise<IUser> {
        return await RestClient.GetAsync<IUser>(`users/${userId}`);
    }

    public static async getUserByUserNameAsync(userName: string): Promise<IUser> {
        return await RestClient.GetAsync<IUser>(`users/${userName}`);
    }

    public static async updateUserAsync(user: IUser): Promise<IUser> {
        return await RestClient.PutAsync<IUser>(`users/${user.ID}`, user);
    }

    public static async getUsersAsync(userNamesOrIds: string[]): Promise<IUser[]> {
        if (userNamesOrIds.length === 0) {
            return [];
        }

        return await RestClient.PostAsync<IUser[]>("users", userNamesOrIds);
    }

    public static async getTopUsersByViewingTimeAsync(count?: number): Promise<IUser[]> {
        let uri: string = "users/top";
        if (count !== undefined) {
            uri += `?count=${count}`;
        }

        return await RestClient.GetAsync<IUser[]>(uri);
    }

    public static async adjustUserCurrencyByUserNameAsync(userName: string, currencyID: string, offsetAmount: number): Promise<IUser> {
        const adjustCurrency: IAdjustCurrency = {
            Amount: offsetAmount
        };

        return await RestClient.PutAsync<IUser>(`users/${userName}/currency/${currencyID}/adjust`, adjustCurrency);
    }

    public static async adjustUserCurrencyByUserIdAsync(userId: number, currencyID: string, offsetAmount: number): Promise<IUser> {
        const adjustCurrency: IAdjustCurrency = {
            Amount: offsetAmount
        };

        return await RestClient.PutAsync<IUser>(`users/${userId}/currency/${currencyID}/adjust`, adjustCurrency);
    }
}
