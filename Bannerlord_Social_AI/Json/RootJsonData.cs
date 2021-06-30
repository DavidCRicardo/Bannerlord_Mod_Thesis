using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class RootJsonData
    {
        [JsonProperty("SettlementJson")]
        public List<SettlementJson> SettlementJson { get; set; }
    }
}