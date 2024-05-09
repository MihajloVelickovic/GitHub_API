import { Author } from "./author.js"

export class Contributor{
    constructor(total, author){
        this.Total = total,
        this.Author = new Author(author.Login);
    }
}