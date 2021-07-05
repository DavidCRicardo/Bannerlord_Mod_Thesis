﻿using Newtonsoft.Json;

namespace Bannerlord_Social_AI
{
    public class Goal
    {
        public Goal(string _relationship, string _targetName, int _value)
        {
            relationship = _relationship;
            targetName = _targetName;
            value = _value;
        }   

        [JsonProperty("relationship")]
        public string relationship { get; set; }

        [JsonProperty("targetName")]
        public string targetName { get; set; }

        [JsonProperty("value")]
        public int value { get; set; }
    }
}