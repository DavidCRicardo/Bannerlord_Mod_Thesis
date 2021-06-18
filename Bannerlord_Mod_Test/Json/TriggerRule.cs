using Newtonsoft.Json;

namespace Bannerlord_Mod_Test
{
    public class TriggerRule
    {
        [JsonProperty("SocialExchangeToDo")]
        public string SocialExchangeToDo { get; set; }
        [JsonProperty("NPCsOnRule")]
        public string NPC_OnRule { get; set; }

        public TriggerRule(string _socialExchangeToDoName, string _npcOnRule)
        {
            SocialExchangeToDo = _socialExchangeToDoName;
            NPC_OnRule = _npcOnRule;
        }
    }
}
