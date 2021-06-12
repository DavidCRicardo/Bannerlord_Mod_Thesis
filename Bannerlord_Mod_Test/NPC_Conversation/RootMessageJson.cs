using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
     class RootMessageJson
    {
        [JsonProperty("SocialExchangeM")]
        public List<SocialExchangeM> SocialExchangeListFromJson { get; set; }
    }
}
