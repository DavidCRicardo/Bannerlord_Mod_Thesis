using Newtonsoft.Json;

namespace FriendlyLords
{
    public class config
    {
        [JsonProperty("RangeConversations")]
        public int RangeConversations { get; set; }

        [JsonProperty("ConversationDelay")]
        public int ConversationDelay { get; set; }

        [JsonProperty("NPCCountdownMultiplier")]
        public float NPCCountdownMultiplier { get; set; }

        [JsonProperty("ConversationOnBattle")]
        public bool ConversationOnBattle { get; set; }

        [JsonProperty("SpeakersOnBattlePerTeamLimit")]
        public int SpeakersOnBattlePerTeamLimit { get; set; }
    }
}
