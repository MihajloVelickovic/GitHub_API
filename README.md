# GitHub API project

### C# console backend that gets commit info from all collaborators on a given repo

## Backend
The ```Backend``` folder contains a C# backend implemented using a ThreadPool, an HttpClient, an HttpListener, and a simple cache.

The listener listens for requests sent to the ```1738``` port.

Request example:
```
http://localhost:1738/?owner={owner}&repo={repo}
```
This will be converted to:
```
https://api.github.com/repos/{owner}/{repo}/stats/contributors
```
and presented in the app as:
```
{owner}/{repo}
Contributor0: X commits
Contributor1: Y commits
....
Total commits: Z
```
## Frontend

The ```Frontend``` folder contains an Express.JS server on the ```8080``` port.

It's a convenient way to both send requests and view results.

Requires Node.JS

Steps to run:
```bash
cd Frontend
npm i
node index.js
```