# GitHub API project

C# console app that gets commit info from all collaborators on a given repo

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
