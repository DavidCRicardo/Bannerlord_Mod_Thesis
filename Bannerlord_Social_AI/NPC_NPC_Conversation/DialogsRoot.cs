using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class DialogsRoot 
    {
        [JsonProperty("SocialExchange")]
        public string SocialExchange { get; set; }
        [JsonProperty("Culture")]
        public List<Culture> CultureList { get; set; }
    }
}
