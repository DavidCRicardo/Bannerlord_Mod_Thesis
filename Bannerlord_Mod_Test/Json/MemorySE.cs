using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    public class MemorySE
    {
        public MemorySE(string _NPC_Name, string _SE_Name)
        {
            NPC_Name = _NPC_Name;
            SE_Name = _SE_Name;
        }
        [JsonProperty("NPC_Name")]
        public string NPC_Name { get; set; }
        [JsonProperty("SE_Name")]
        public string SE_Name { get; set; }
    }
}
