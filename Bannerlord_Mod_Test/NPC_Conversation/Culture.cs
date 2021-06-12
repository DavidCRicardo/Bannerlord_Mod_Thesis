using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    class Culture
    {
        [JsonProperty("CultureCode")]
        public string CultureCode { get; set; }
        [JsonProperty("NPCDialogs")]
        public List<NPCDialog> NPCDialogs { get; set; }
    }
}
