using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class NPCDialog
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("message")]
        public List<string> messages { get; set; }
    }
}
