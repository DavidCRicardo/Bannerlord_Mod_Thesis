using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
     class RootMessageJson
    {
        [JsonProperty("DialogsRoot")]
        public List<DialogsRoot> SocialExchangeListFromJson { get; set; }
    }
}
