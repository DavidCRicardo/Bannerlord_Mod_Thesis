using Newtonsoft.Json;

namespace Bannerlord_Social_AI
{
    class UserInfoJson
    {
        [JsonProperty("P_Friendly")]
        public int PFriendly { get; set; }
        [JsonProperty("P_UnFriendly")]
        public int PUnFriendly { get; set; }
        [JsonProperty("P_Hostile ")]
        public int PHostile { get; set; }
        [JsonProperty("P_Romantic ")]
        public int PRomantic { get; set; }
        [JsonProperty("P_Special ")]
        public int PSpecial { get; set; }

        [JsonProperty("N_Friendly")]
        public int NFriendly { get; set; }
        [JsonProperty("N_UnFriendly")]
        public int NUnFriendly { get; set; }
        [JsonProperty("N_Hostile ")]
        public int NHostile { get; set; }
        [JsonProperty("N_Romantic ")]
        public int NRomantic { get; set; }
        [JsonProperty("N_Special ")]
        public int NSpecial { get; set; }

        [JsonProperty("PlayerInteractedWithNPC ")]
        public int PlayerInteractedWithNPC { get; set; }
        [JsonProperty("NPCInteractedWithPlayer")]
        public int NPCInteractedWithPlayer { get; set; }
        [JsonProperty("NPCsInteractedWithNPC")]
        public int NPCsInteractedWithNPC { get; set; }
        [JsonProperty("TotalSocialExchanges")]
        public int TotalSocialExchanges { get; set; }
    }
}
