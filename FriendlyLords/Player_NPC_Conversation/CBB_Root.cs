using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class CBB_Root
    {
        [JsonProperty("PlayerNPCDialogs")]
        public List<PlayerNPCDialog> PlayerNPCDialogs { get; set; }
    }
}
