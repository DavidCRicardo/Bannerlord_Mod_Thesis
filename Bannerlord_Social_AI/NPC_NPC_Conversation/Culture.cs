using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class Culture
    {
        [JsonProperty("CultureCode")]
        public string CultureCode { get; set; }
        [JsonProperty("NPCDialogs")]
        public List<NPCDialog> NPCDialogs { get; set; }
    }
}
