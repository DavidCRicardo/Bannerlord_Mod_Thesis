using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    class SEsPerformedToday
    {
        [JsonProperty("SEsPerformedList")]
        public List<SEsPerformed> SEsPerformedList { get; set; }
    }
}
