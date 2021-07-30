using Newtonsoft.Json;

namespace Bannerlord_Social_AI
{
    class UserInfoJson
    {
        [JsonProperty("DaysPassedInGame")]
        public int DaysPassedInGame { get; set; }

        [JsonProperty("P_Compliment")]
        public int PCompliment { get; set; }
        [JsonProperty("P_GiveGift")]
        public int PGiveGift { get; set; }
        [JsonProperty("P_Gratitude ")]
        public int PGratitude { get; set; }
        [JsonProperty("P_Jealous ")]
        public int PJealous { get; set; }
        [JsonProperty("P_FriendSabotage ")]
        public int PFriendSabotage { get; set; }
        [JsonProperty("P_Flirt ")]
        public int PFlirt { get; set; }
        [JsonProperty("P_Bully ")]
        public int PBully { get; set; }
        [JsonProperty("P_RomanticSabotage ")]
        public int PRomanticSabotage { get; set; }
        [JsonProperty("P_Break ")]
        public int PBreak { get; set; }
        [JsonProperty("P_AskOut ")]
        public int PAskOut { get; set; }
        [JsonProperty("P_HaveAChild ")]
        public int PHaveAChild { get; set; }

        [JsonProperty("P_Friendly_SEs ")]
        public int PFriendlySEs { get; set; }
        [JsonProperty("P_UnFriendly_SEs ")]
        public int PUnFriendlySEs { get; set; }
        [JsonProperty("P_Romantic_SEs ")]
        public int PRomanticSEs { get; set; }
        [JsonProperty("P_Hostile_SEs ")]
        public int PHostileSEs { get; set; }
        [JsonProperty("P_Special_SEs ")]
        public int PSpecialSEs { get; set; }

        [JsonProperty("N_Gratitude ")]
        public int NGratitude { get; set; }
        [JsonProperty("N_Compliment")]
        public int NCompliment { get; set; }
        [JsonProperty("N_GiveGift")]
        public int NGiveGift { get; set; }
        [JsonProperty("N_Jealous ")]
        public int NJealous { get; set; }
        [JsonProperty("N_FriendSabotage ")]
        public int NFriendSabotage { get; set; }
        [JsonProperty("N_Flirt ")]
        public int NFlirt { get; set; }
        [JsonProperty("N_Bully ")]
        public int NBully { get; set; }
        [JsonProperty("N_RomanticSabotage ")]
        public int NRomanticSabotage { get; set; }
        [JsonProperty("N_Break ")]
        public int NBreak { get; set; }
        [JsonProperty("N_AskOut ")]
        public int NAskOut { get; set; }
        [JsonProperty("N_HaveAChild ")]
        public int NHaveAChild { get; set; }

        [JsonProperty("N_Friendly_SEs ")]
        public int NFriendlySEs { get; set; }
        [JsonProperty("N_Romantic_SEs ")]
        public int NRomanticSEs { get; set; }
        [JsonProperty("N_UnFriendly_SEs ")]
        public int NUnFriendlySEs { get; set; }
        [JsonProperty("N_Hostile_SEs ")]
        public int NHostileSEs { get; set; }
        [JsonProperty("N_Special_SEs ")]
        public int NSpecialSEs { get; set; }

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
