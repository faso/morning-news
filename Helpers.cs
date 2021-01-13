using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Reddit.Controllers;

namespace MorningNews2
{
    public static class Helpers
    {
        public static string FormatNumber(int num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + "K";
            if (num >= 10000)
            {
                return (num / 1000D).ToString("0.#") + "K";
            }
            return num.ToString("#,0");
        }

        public static string GetRedditPostLink(Post post)
        {
            string link = "";
            if (post is LinkPost lp)
                link = lp.URL;
            if (post is SelfPost sp)
                link = $"https://www.reddit.com/r/hiphopheads/comments/{post.Id}";

            return link;
        }
    }
}
