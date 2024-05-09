import {Response} from "./Models/response.js"

const ownerBox = document.querySelector(".owner");
const repoBox = document.querySelector(".repo");

const scheme = window.matchMedia("(prefers-color-scheme: dark)").matches;

if(scheme)
    document.body.classList.toggle("dark");
else
    document.body.classList.toggle("light");

const toggle = document.querySelector(".theme-button");
toggle.onclick = () => {
    if(scheme){
        document.body.classList.toggle("dark");
        document.body.classList.toggle("light");
    }
    else{
        document.body.classList.toggle("light");
        document.body.classList.toggle("dark");
    }
    
}

const clear = document.querySelector(".clear-button");
clear.onclick = () => {
    const responses = document.querySelector(".responses");
    responses.replaceChildren();
}

const button = document.querySelector(".submit");
button.onclick = async () => {

    const owner = ownerBox.value;
    const repo = repoBox.value;

    console.log(`Sending request to http://localhost:1738/?owner=${owner}&repo=${repo}`);
    
    let response;

    const res = await fetch(`http://localhost:1738/?owner=${owner}&repo=${repo}`).
                      then(res => res.json()).
                      then((data) =>{
                        response = new Response(data.Key, data.Contributors, data.TotalCommits, data.TotalTime);
                        response.drawResponse(document.querySelector(".responses"));
                    });
          

}