using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    public class SettlementJson
    {
        public SettlementJson(string _currentSettlement, string _locationWithId, List<CustomAgentJson> _customAgents)
        {
            this.Name = _currentSettlement;
            this.LocationWithId = _locationWithId;
            this.CustomAgentJsonList = _customAgents;
        }

        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("LocationWithId")]
        public string LocationWithId { get; set; }
        [JsonProperty("CustomAgent")]
        public List<CustomAgentJson> CustomAgentJsonList { get; set; }
    }
}