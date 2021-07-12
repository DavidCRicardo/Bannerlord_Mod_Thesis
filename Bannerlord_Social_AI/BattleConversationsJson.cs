﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class BattleConversationsJson
    {
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