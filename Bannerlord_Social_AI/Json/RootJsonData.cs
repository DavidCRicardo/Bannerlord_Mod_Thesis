using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    public class RootJsonData
    {
        [JsonProperty("requiredRenown")]
        public int requiredRenown { get; set; }
        [JsonProperty("SettlementJson")]
        public List<SettlementJson> SettlementJson { get; set; }
    }
}