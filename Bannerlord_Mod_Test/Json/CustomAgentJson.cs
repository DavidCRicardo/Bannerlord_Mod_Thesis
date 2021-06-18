using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    public class CustomAgentJson
    {
        public CustomAgentJson(string name, List<Trait> traitList)
        {
            Name = name;
            TraitList = traitList;
            GoalsList = new List<Goal>();
            SocialNetworkBeliefs = new List<SocialNetworkBelief>();
            ItemsList = new List<Item>();
            MemoriesList = new List<MemorySE>();
            TriggerRulesList = new List<TriggerRule>();
        }

        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("TraitsList")]
        public List<Trait> TraitList { get; set; }
        [JsonProperty("GoalsList")]
        public List<Goal> GoalsList { get; set; }
        [JsonProperty("SocialNetworkBeliefs")]
        public List<SocialNetworkBelief> SocialNetworkBeliefs { get; set; }
        [JsonProperty("ItemsList")]
        public List<Item> ItemsList { get; set; }
        [JsonProperty("MemorySEList")]
        public List<MemorySE> MemoriesList { get; set; }
        [JsonProperty("TriggerRulesList")]
        public List<TriggerRule> TriggerRulesList { get; set; }
    }
}
