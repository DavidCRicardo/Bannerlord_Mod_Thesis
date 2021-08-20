using Newtonsoft.Json;

namespace FriendlyLords
{
    public class config
    {
        [JsonProperty("RangeConversations")]
        public int RangeConversations { get; set; }

        [JsonProperty("ConversationOnBattle")]
        public bool ConversationOnBattle { get; set; }
    }
}
