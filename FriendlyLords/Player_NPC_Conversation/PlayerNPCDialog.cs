using Newtonsoft.Json;

namespace FriendlyLords
{
    public class PlayerNPCDialog
    {
        [JsonProperty("PlayerDialog")]
        public bool PlayerDialog { get; set; }

        [JsonProperty("InputToken")]
        public string InputToken { get; set; }

        [JsonProperty("OutputToken")]
        public string OutputToken { get; set; }

        [JsonProperty("Text")]
        public string Text { get; set; }

        [JsonProperty("Condition")]
        public string Condition { get; set; }

        [JsonProperty("Consequence")]
        public string Consequence { get; set; }
    }
}
