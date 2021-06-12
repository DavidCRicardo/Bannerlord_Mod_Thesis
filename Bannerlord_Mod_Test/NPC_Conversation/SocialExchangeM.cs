using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    class SocialExchangeM 
    {
        [JsonProperty("Intention")]
        public string Intention { get; set; }
        [JsonProperty("Culture")]
        public List<Culture> CultureList { get; set; }
    }
}
