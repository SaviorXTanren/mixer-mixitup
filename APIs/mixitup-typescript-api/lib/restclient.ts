import * as WebRequest from "web-request";

export class RestClient {
    private static readonly BaseUri: string = "http://localhost:8911/api/";

    public static async GetAsync<T>(path: string): Promise<T> {
        const options: WebRequest.RequestOptions = {
            method: "GET"
        };

        return await WebRequest.json<T>(RestClient.BaseUri + path);
    }

    public static async PostAsync<T>(path: string, model: any): Promise<T> {
        const options: WebRequest.RequestOptions = {
            method: "POST",
            body: model
        };

        return await WebRequest.json<T>(RestClient.BaseUri + path, options);
    }

    public static async DeleteAsync(path: string): Promise<{}> {
        return await WebRequest.delete(RestClient.BaseUri + path);
    }
}
