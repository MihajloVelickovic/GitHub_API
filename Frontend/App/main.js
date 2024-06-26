import {Response} from "./Models/response.js"
import { ErrorResponse } from "./Models/errorResponse.js";

let scheme = localStorage.getItem("scheme");

if(!scheme)
    scheme = window.matchMedia("(prefers-color-scheme: dark)").matches;
else
    scheme = (scheme === "true");

if(scheme)
    document.body.classList.toggle("dark");

else
    document.body.classList.toggle("light");

const toggle = document.querySelector(".theme-button");
toggle.onclick = () => {
    if(scheme){
        document.body.classList.toggle("dark");
        document.body.classList.toggle("light");
        localStorage.setItem("scheme", "false");
    }
    else{
        document.body.classList.toggle("light");
        document.body.classList.toggle("dark");
        localStorage.setItem("scheme", "true");
    }

}

const clear = document.querySelector(".clear-button");
clear.onclick = () => {
    const responses = document.querySelector(".responses");
    responses.replaceChildren();
}

const ownerBox = document.querySelector(".owner");
const repoBox = document.querySelector(".repo");

const button = document.querySelector(".submit");
button.onclick = async () => {

    const owner = ownerBox.value;
    const repo = repoBox.value;

    console.log(`Sending request to http://localhost:1738/?owner=${owner}&repo=${repo}`);
    
    let response;

    const res = await fetch(`http://localhost:1738/?owner=${owner}&repo=${repo}`).
                                    then(res => {
                                        if (!res.ok) {
                                            return res.json().then(errorData => {
                                                throw new Error(`Error: ${errorData.Message}`);
                                            });
                                        }
                                        return res.json();
                                    })
                                    .then(data => {
                                        const response = new Response(data.Key, data.Contributors, data.TotalCommits, data.TotalTime);
                                        response.drawResponse(document.querySelector(".responses"));
                                    })
                                    .catch(err => {
                                        console.log(err);
                                        const errorResponse = new ErrorResponse(err.message);
                                        errorResponse.drawResponse(document.querySelector(".responses"));
                                    });
}