using Newtonsoft.Json;

namespace Bannerlord_Mod_Test
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
        public int Condition { get; set; }

        [JsonProperty("Consequence")]
        public int Consequence { get; set; }
    }
}
