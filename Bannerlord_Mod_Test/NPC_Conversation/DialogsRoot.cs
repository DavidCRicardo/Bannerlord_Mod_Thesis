using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    class DialogsRoot 
    {
        [JsonProperty("SocialExchange")]
        public string SocialExchange { get; set; }
        [JsonProperty("Culture")]
        public List<Culture> CultureList { get; set; }
    }
}
