using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
     class RootMessageJson
    {
        [JsonProperty("DialogsRoot")]
        public List<DialogsRoot> SocialExchangeListFromJson { get; set; }
    }
}
