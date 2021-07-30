using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    public class RootJsonData
    {
        [JsonProperty("requiredRenown")]
        public int requiredRenown { get; set; }
        [JsonProperty("SettlementJson")]
        public List<SettlementJson> SettlementJson { get; set; }
    }
}