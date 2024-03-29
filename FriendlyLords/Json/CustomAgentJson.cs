﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace FriendlyLords
{
    public class CustomAgentJson
    {
        public CustomAgentJson(string name, int id, List<Trait> traitList, List<Item> itemList, List<SocialNetworkBelief> networkBeliefs = null)
        {
            Name = name;
            Id = id;
            TraitList = traitList;

            SocialNetworkBeliefs = new List<SocialNetworkBelief>();
            if (networkBeliefs != null)
            {
                SocialNetworkBeliefs = networkBeliefs;
            }
            
            ItemsList = itemList;
            SocialExchangeMemory = new List<MemorySE>();
        }

        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("TraitsList")]
        public List<Trait> TraitList { get; set; }
        [JsonProperty("SocialNetworkBeliefs")]
        public List<SocialNetworkBelief> SocialNetworkBeliefs { get; set; }
        [JsonProperty("ItemsList")]
        public List<Item> ItemsList { get; set; }
        [JsonProperty("SocialExchangeMemory")]
        public List<MemorySE> SocialExchangeMemory { get; set; }
    }
}