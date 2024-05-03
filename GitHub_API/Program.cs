using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Newtonsoft.Json;



namespace GitHub_API{
    public class Program{
        
        public static readonly HttpClient httpClient = new HttpClient();
        public static void Main(string[] args){
            
            /* Uzimanje github tokena iz .env fajla */
            var baseDir = DirExtension.ProjectBase();
            if (baseDir != null){
                var path = Path.Combine(baseDir, ".env");
                DotEnv.Inject(path);
            }
            
            /*
             * HttpListener na localhostu, zahtevi ce se parsirati
             * i prevoditi u zahteve GitHub API-u
             */
            var listener = new HttpListener();
            listener.Prefixes
                    .Add("http://localhost:1738/");
            listener.Start();
            
            while(true)
                ThreadPool.QueueUserWorkItem(ServeRequest, listener.GetContext());


        }

        private static void ServeRequest(object? state){

            if (state == null)
                return;
            
            var context = (HttpListenerContext)state;

            try{

                /* Izvlacenje vrednosti za owner i repo */
                var vars = context.Request
                    .Url?
                    .Query
                    .Remove(0, 1)
                    .Split("&");

                if (vars == null)
                    throw new Exception("Null query exc");
                if (vars.Length != 2)
                    throw new Exception("Mora imati tacno dva parametra");

                var owner = vars[0].Split("=");
                var repo = vars[1].Split("=");

                if (owner[0] != "owner")
                    throw new Exception("Prvi argument mora biti owner");
                if (repo[0] != "repo")
                    throw new Exception("Drugi argument mora biti repo");

                /* Treba pogledati jos */
                string apiUrl = $"https://api.github.com/repos/{owner[1]}/{repo[1]}/contributors?anon=1";
                
                /* Podesavanje klijenta da salje korektan zahtev */
                httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHub_API");
                var ghToken = Environment.GetEnvironmentVariable("GH_TOKEN");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghToken);
                var res = httpClient.GetAsync(apiUrl).Result;
                
                if (!res.IsSuccessStatusCode)
                    throw new Exception($"ERROR: {res.StatusCode}");

                var content = res.Content.ReadAsStringAsync().Result;
                var contentJson = JsonConvert.SerializeObject(content);
                
                Console.WriteLine($"{contentJson}");

                //Kesirati vrednost
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }
    }
}