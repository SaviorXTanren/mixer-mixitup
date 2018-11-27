import { ICurrency } from "./currency";

export interface IUser {
    ID: number;
    UserName: string;
    ViewingMinutes?: number;
    CurrencyAmmounts: ICurrency[];
}
