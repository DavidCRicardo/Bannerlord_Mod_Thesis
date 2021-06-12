using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    public class TriggerRule
    {
        [JsonProperty("RelationshipName")]
        public string RelationshipName { get; set; }
        [JsonProperty("NPCsOnRule")]
        public List<string> NPCsOnRule { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }

        public TriggerRule(string _relationshipName, List<string> _npcsOnRule, int _value)
        {
            RelationshipName = _relationshipName;
            NPCsOnRule = _npcsOnRule;
            Value = _value;
        }
    }
}
