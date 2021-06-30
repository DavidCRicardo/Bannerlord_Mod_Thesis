using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class NPCDialog
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("message")]
        public List<string> messages { get; set; }
    }
}
