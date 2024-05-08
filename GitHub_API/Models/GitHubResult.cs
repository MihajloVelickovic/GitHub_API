using GitHub_API.Models;

namespace GitHub_API;

public class GitHubResult{
    public int Total{ get; set; }
    public Author? Author{ get; set; }
}