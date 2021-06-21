using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord_Mod_Test
{
    public class Trait
    {
        public Trait(string _traitName)
        {
            traitName = _traitName;

            SetCountdownToIncreaseDecrease(traitName);
        }
        [JsonProperty("traitName")]
        public string traitName { get; set; }
        
        private bool decreaseCountdown;
        private bool increaseCountdown;
        public bool GetdecreaseCountdown()
        {
            return decreaseCountdown;
        }

        private void SetdecreaseCountdown(bool value)
        {
            decreaseCountdown = value;
        }
        public bool GetincreaseCountdown()
        {
            return increaseCountdown;
        }
        private void SetincreaseCountdown(bool value)
        {
            increaseCountdown = value;
        }

        public void SetCountdownToIncreaseDecrease(string traitName)
        {
            switch (traitName)
            {
                case "Shy":
                    SetincreaseCountdown(true); 
                    break;
                case "Brave":
                    SetdecreaseCountdown(true);
                    break;
                default:
                    break;
            }
        }
    }
}