using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class Culture
    {
        [JsonProperty("CultureCode")]
        public string CultureCode { get; set; }
        [JsonProperty("NPCDialogs")]
        public List<NPCDialog> NPCDialogs { get; set; }
    }
}
