import { Contributor } from "./contributor.js";

export class Response{
    constructor(key, contributors, totalcomm, totaltime){
        this.Key = key,
        this.Contributors = [],
        contributors.forEach(contrib => {
            this.Contributors.push(new Contributor(contrib.Total, contrib.Author));
        })
        this.TotalCommits = totalcomm,
        this.TotalTime = totaltime;
    }

    drawResponse(container){

        const resultDiv = document.createElement("div");
        resultDiv.classList.add("response");
        const keyDiv = document.createElement("div");
        
        const keylab = document.createElement("label");  
        const keySplit = this.Key.split("/");
        keylab.innerHTML = `<a href=https://www.github.com/${keySplit[0]}/${keySplit[1]}><b>${keySplit[0]}</b>/<i>${keySplit[1]}</i></a>`;
        keylab.classList.add("margin-10");
        
        keyDiv.appendChild(keylab);
        resultDiv.appendChild(keyDiv);
        
        this.Contributors.forEach(cont => {
            
            let contdiv = document.createElement("div");
            contdiv.classList.add("form-gr");
            
            const contribAuthor = document.createElement("label");
            contribAuthor.classList.add("margin-10")
            contribAuthor.innerHTML = `<a href=https://www.github.com/${cont.Author.Login}>${cont.Author.Login}: `;
            
            const contribCommits = document.createElement("label");
            contribCommits.classList.add("margin-10")
            contribCommits.innerHTML = cont.Total + " commits"; 
            
            contdiv.appendChild(contribAuthor);
            contdiv.appendChild(contribCommits);
            resultDiv.appendChild(contdiv);

        });

        const totalDiv = document.createElement("div");
        totalDiv.classList.add("form-gr");
            
        const contribAuthor = document.createElement("label");
        contribAuthor.classList.add("margin-10")
        contribAuthor.innerHTML = "Total Commits: ";
            
        const contribCommits = document.createElement("label");
        contribCommits.classList.add("margin-10")
        contribCommits.innerHTML = this.TotalCommits; 
        
        totalDiv.appendChild(contribAuthor);
        totalDiv.appendChild(contribCommits);
        resultDiv.appendChild(totalDiv);

        const timeDiv = document.createElement("div");
        timeDiv.classList.add("form-gr");
            
        const totalTime = document.createElement("label");
        totalTime.classList.add("margin-10")
        totalTime.innerHTML = "Total Time: ";
            
        const timeNumber = document.createElement("label");
        timeNumber.classList.add("margin-10")
        timeNumber.innerHTML = this.TotalTime+"ms"; 
        
        timeDiv.appendChild(totalTime);
        timeDiv.appendChild(timeNumber);
        resultDiv.appendChild(timeDiv);

        container.appendChild(resultDiv);
    }
    
}