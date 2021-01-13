using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MorningNews2.Models;

using Reddit;
using Reddit.Controllers;

namespace MorningNews2.Controllers
{
    [Route("[action]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Settings _settings;

        public HomeController(ILogger<HomeController> logger, IOptions<Settings> config)
        {
            _logger = logger;
            _settings = config.Value;
        }

        public IActionResult Index()
        {
            return Redirect($"https://www.reddit.com/api/v1/authorize?client_id={_settings.AppId}&response_type=code&state=jeff&redirect_uri={HttpUtility.UrlEncode(_settings.AppUrl)}%2FLoginCallback&duration=permanent&scope=identity,edit,flair,history,modconfig,modflair,modlog,modposts,modwiki,mysubreddits,privatemessages,read,report,save,submit,subscribe,vote,wikiedit,wikiread");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("{npointId}")]
        public async Task<IActionResult> Dashboard(string npointId)
        {
            var r = new RedditClient(appId: _settings.AppId, appSecret: _settings.AppSecret, accessToken: _settings.AccessToken, refreshToken: _settings.RefreshToken);
            var vm = new NewsViewModel();

            Config config;
            using (var http = new HttpClient())
            {
                var result = await http.GetStringAsync("https://api.npoint.io/" + npointId);
                config = JsonSerializer.Deserialize<Config>(result);
            }

            // hackernews
            if (config.Hackernews)
            {
                using (var http = new HttpClient())
                {
                    var topStories = await (await http.GetAsync("https://hacker-news.firebaseio.com/v0/topstories.json")).Content.ReadAsStringAsync();
                    var storyIds = JsonSerializer.Deserialize<int[]>(topStories);

                    foreach (var id in storyIds.Take(60))
                    {
                        var storyRaw = await (await http.GetAsync($"https://hacker-news.firebaseio.com/v0/item/{id}.json")).Content.ReadAsStringAsync();
                        var story = JsonSerializer.Deserialize<HackerNewsResult>(storyRaw);
                        if (story.score > 100)
                            vm.HackerNews.Add(story);
                    }

                    vm.HackerNews = vm.HackerNews.OrderByDescending(o => o.score).ToList();
                }
            }

            // reddit
            foreach (var subreddit in config.Subreddits)
            {
                vm.Subreddits.Add(
                    subreddit.Name,
                    r.Subreddit(subreddit.Name).Posts.Hot.Where(o => o.Score > subreddit.Cutoff).OrderByDescending(o => o.Score));
            }

            // links
            vm.Links.AddRange(config.Links);

            return View(vm);
        }
    }

    public class NewsViewModel
    {
        public NewsViewModel()
        {
            Subreddits = new Dictionary<string, IEnumerable<Post>>();
            HackerNews = new List<HackerNewsResult>();
            Links = new List<string>();
        }

        public Dictionary<string, IEnumerable<Post>> Subreddits { get; set; }
        public List<HackerNewsResult> HackerNews { get; set; }
        public List<string> Links { get; set; }
    }

    public class OauthResult
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class HackerNewsResult
    {
        public string by { get; set; }
        public int descendants { get; set; }
        public int id { get; set; }
        public List<int> kids { get; set; }
        public int score { get; set; }
        public int time { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }
}
