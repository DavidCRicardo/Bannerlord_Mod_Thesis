using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class RootJsonData
    {
        [JsonProperty("SettlementJson")]
        public List<SettlementJson> SettlementJson { get; set; }
    }
}