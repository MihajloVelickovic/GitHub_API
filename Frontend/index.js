import express from "express"

const app = express();

app.use(express.static("App"));

app.listen(8080, () => console.log("Listening"));