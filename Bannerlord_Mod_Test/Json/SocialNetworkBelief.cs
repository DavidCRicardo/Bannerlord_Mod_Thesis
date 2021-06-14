using Newtonsoft.Json;
using System.Collections.Generic;
public class SocialNetworkBelief
{
    public SocialNetworkBelief(string _relationship, List<string> _agents, int _value)
    {
        relationship = _relationship;
        agents = _agents;
        value = _value;
    }
    [JsonProperty("relationship")]
    public string relationship { get; set; }
    [JsonProperty("agents")]
    public List<string> agents { get; set; }
    [JsonProperty("value")]
    public int value { get; set; }
}