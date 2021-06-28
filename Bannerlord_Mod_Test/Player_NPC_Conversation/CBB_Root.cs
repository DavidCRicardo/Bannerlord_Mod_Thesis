using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    class CBB_Root
    {
        [JsonProperty("PlayerNPCDialogs")]
        public List<PlayerNPCDialog> PlayerNPCDialogs { get; set; }
    }
}
