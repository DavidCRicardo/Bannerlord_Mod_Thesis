using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class BattleConversationsJson
    {
        [JsonProperty("RangeConversations")]
        public int RangeConversations    { get; set; }

        [JsonProperty("PlayerTeamLimitSpeakers")]
        public int PlayerTeamLimitSpeakers { get; set; }

        [JsonProperty("OtherTeamLimitSpeakers")]
        public int OtherTeamLimitSpeakers { get; set; }

        [JsonProperty("Winning")]
        public List<string> Winning { get; set; }

        [JsonProperty("Neutral")]
        public List<string> Neutral { get; set; }

        [JsonProperty("Losing")]
        public List<string> Losing { get; set; }
    }
}
