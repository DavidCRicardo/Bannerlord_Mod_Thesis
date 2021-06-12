using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    public class CustomAgentJson
    {
        public CustomAgentJson(string name, List<Trait> traitList, List<TriggerRule> triggerRules)
        {
            Name = name;
            TraitList = traitList;
            GoalsList = new List<Goal>();
            BeliefsList = new List<Belief>();
            ItemsList = new List<Item>();
            MemoriesList = new List<MemorySE>();
            TriggerRulesList = triggerRules;
        }

        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("TraitsList")]
        public List<Trait> TraitList { get; set; }
        [JsonProperty("GoalsList")]
        public List<Goal> GoalsList { get; set; }
        [JsonProperty("BeliefsList")]
        public List<Belief> BeliefsList { get; set; }
        [JsonProperty("ItemsList")]
        public List<Item> ItemsList { get; set; }
        [JsonProperty("MemorySEList")]
        public List<MemorySE> MemoriesList { get; set; }
        [JsonProperty("TriggerRulesList")]
        public List<TriggerRule> TriggerRulesList { get; set; }
    }
}
