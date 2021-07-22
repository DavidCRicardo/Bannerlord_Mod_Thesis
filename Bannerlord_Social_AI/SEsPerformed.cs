using Newtonsoft.Json;

namespace Bannerlord_Social_AI
{
    public class SEsPerformed
    {
        public SEsPerformed(string name, string socialExchange)
        {
            Hero_Name = name;
            SocialExchange = socialExchange;
        }

        [JsonProperty("Hero_Name")]
        public string Hero_Name { get; set; }
        [JsonProperty("SocialExchange")]
        public string SocialExchange { get; set; }
    }
}