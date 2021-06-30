using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
     class RootMessageJson
    {
        [JsonProperty("DialogsRoot")]
        public List<DialogsRoot> SocialExchangeListFromJson { get; set; }
    }
}
