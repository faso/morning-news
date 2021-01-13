using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MorningNews2
{
    public class Config
    {
        public bool Hackernews { get; set; }
        public IEnumerable<SubredditConfig> Subreddits { get; set; }
        public IEnumerable<string> Links { get; set; }
    }

    public class SubredditConfig
    {
        public string Name { get; set; }
        public int Cutoff { get; set; }
    }
}
