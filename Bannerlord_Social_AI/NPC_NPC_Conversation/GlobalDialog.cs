using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class GlobalDialog
    {
        [JsonProperty("SocialExchange")]
        public string SocialExchange { get; set; }
        [JsonProperty("Culture")]
        public List<Culture> Culture { get; set; }
    }
}