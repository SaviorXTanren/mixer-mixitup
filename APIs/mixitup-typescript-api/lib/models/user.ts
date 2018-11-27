import { ICurrencyAmount } from "./currencyamount";

export interface IUser {
    ID: number;
    UserName: string;
    ViewingMinutes?: number;
    CurrencyAmounts: ICurrencyAmount[];
}
