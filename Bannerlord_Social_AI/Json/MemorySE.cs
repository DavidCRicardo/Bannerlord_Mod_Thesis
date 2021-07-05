using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    public class MemorySE
    {
        public MemorySE(List<string> _NPC_Names, List<int> _IDs, string _SE_Name)
        {
            agents = _NPC_Names;
            IDs = _IDs;
            SE_Name = _SE_Name;
        }
        [JsonProperty("agents")]
        public List<string> agents { get; set; } 
        [JsonProperty("IDs")]
        public List<int> IDs { get; set; }
        [JsonProperty("SE_Name")]
        public string SE_Name { get; set; }
    }
}