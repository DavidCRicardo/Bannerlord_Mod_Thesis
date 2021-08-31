using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class BattleConversationsJson
    {
        [JsonProperty("Winning")]
        public List<string> Winning { get; set; }

        [JsonProperty("Neutral")]
        public List<string> Neutral { get; set; }

        [JsonProperty("Losing")]
        public List<string> Losing { get; set; }
    }
}
