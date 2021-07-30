using Newtonsoft.Json;

namespace FriendlyLords
{
    public class SEsPerformed
    {
        public SEsPerformed(string name, int id, string socialExchange)
        {
            Hero_Name = name;
            Hero_ID = id;
            SocialExchange = socialExchange;
        }

        [JsonProperty("Hero_Name")]
        public string Hero_Name { get; set; }
        [JsonProperty("Hero_ID")]
        public int Hero_ID { get; set; }
        [JsonProperty("SocialExchange")]
        public string SocialExchange { get; set; }
    }
}