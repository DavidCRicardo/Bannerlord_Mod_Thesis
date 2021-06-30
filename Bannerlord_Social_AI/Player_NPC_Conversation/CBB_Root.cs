using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class CBB_Root
    {
        [JsonProperty("PlayerNPCDialogs")]
        public List<PlayerNPCDialog> PlayerNPCDialogs { get; set; }
    }
}
