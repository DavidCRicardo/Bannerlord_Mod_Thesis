using Newtonsoft.Json;

namespace Bannerlord_Mod_Test
{
    public class TriggerRule
    {
        [JsonProperty("SocialExchangeToDo")]
        public string SocialExchangeToDo { get; set; }
        [JsonProperty("NPC_OnRule")]
        public string NPC_OnRule { get; set; }
        [JsonProperty("NPC_ID")]
        public int NPC_ID { get; set; }

        public TriggerRule(string _socialExchangeToDoName, string _npcOnRule, int _npcID)
        {
            SocialExchangeToDo = _socialExchangeToDoName;
            NPC_OnRule = _npcOnRule;
            NPC_ID = _npcID;
        }
    }
}