using Newtonsoft.Json;
using System.Collections.Generic;

public class SocialNetworkBelief
{
    public SocialNetworkBelief(string _relationship, List<string> _agents, List<int> _ids, float _value)
    {
        relationship = _relationship;
        agents = _agents;
        IDs = _ids;
        value = _value;
    }
    [JsonProperty("relationship")]
    public string relationship { get; set; }
    [JsonProperty("agents")]
    public List<string> agents { get; set; }
    [JsonProperty("ids")]
    public List<int> IDs { get; set; }
    [JsonProperty("value")]
    public float value { get; set; }
}