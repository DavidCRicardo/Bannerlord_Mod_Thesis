﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    public class Item
    {
        public Item(string _itemName, int _quantity)
        {
            itemName = _itemName;
            quantity = _quantity;
        }
        [JsonProperty("itemName")]
        public string itemName { get; set; }
        [JsonProperty("quantity")]
        public int quantity { get; set; }
    }
}