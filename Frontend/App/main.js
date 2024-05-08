const button = document.querySelector(".submit");

const ownerBox = document.querySelector(".owner");
const repoBox = document.querySelector(".repo");

button.onclick = async () => {

    const owner = ownerBox.value;
    const repo = repoBox.value;

    console.log(`Sending request to http://localhost:1738/?owner=${owner}&repo=${repo}`);

    const res = await fetch(`http://localhost:1738/?owner=${owner}&repo=${repo}`);
    
}