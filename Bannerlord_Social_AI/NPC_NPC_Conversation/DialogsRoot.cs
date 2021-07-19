using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class DialogsRoot 
    {
        [JsonProperty("Location")]
        public string Location { get; set; }
        [JsonProperty("GlobalDialogs")]
        public List<GlobalDialog> GlobalDialogs { get; set; }
    }
}
