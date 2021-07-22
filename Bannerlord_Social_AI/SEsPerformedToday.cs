using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Social_AI
{
    class SEsPerformedToday
    {
        [JsonProperty("SEsPerformedList")]
        public List<SEsPerformed> SEsPerformedList { get; set; }
    }
}
