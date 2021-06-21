﻿using Newtonsoft.Json;

namespace Bannerlord_Mod_Test
{
    public class MemorySE
    {
        public MemorySE(string _NPC_Name, int _ID, string _SE_Name)
        {
            NPC_Name = _NPC_Name;
            NPC_ID = _ID;
            SE_Name = _SE_Name;
        }
        [JsonProperty("NPC_Name")]
        public string NPC_Name { get; set; } 
        [JsonProperty("NPC_ID")]
        public int NPC_ID { get; set; }
        [JsonProperty("SE_Name")]
        public string SE_Name { get; set; }
    }
}