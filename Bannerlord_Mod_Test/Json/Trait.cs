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

            SetSEsToIncreaseDecrease(traitName);
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


        private SocialExchangeSE.IntentionEnum intentionToIncreasePoints { get; set; }
        private SocialExchangeSE.IntentionEnum intentionToDecreasePoints { get; set; }
        public int GetValue(SocialExchangeSE.IntentionEnum _currentIntention)
        {
            if (_currentIntention == intentionToIncreasePoints)
            {
                return 2;
            }
            else if (_currentIntention == intentionToDecreasePoints)
            {
                return -2;
            }
            else
            {
                return 0;
            }
        }
        public void SetSEsToIncreaseDecrease(string traitName)
        {
            switch (traitName)
            {
                case "Friendly":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Friendly;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.UnFriendly;
                    break;
                case "Hostile":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.UnFriendly;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Friendly;
                    break;
                case "Charming":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Romantic;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Undefined;
                    break;
                case "UnCharming":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Undefined;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Romantic;
                    break;
                case "Shy":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Undefined;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.AllTypes; // decrease for all
                    break;
                case "Brave":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Undefined;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.AllTypes; // increase for all
                    break;
                case "Calm":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Friendly;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.UnFriendly;
                    break;
                case "Aggressive":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.UnFriendly;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Friendly;
                    break;
                case "Faithful":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Romantic;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Romantic; // If its dating
                    break;
                case "Unfaithful":
                    intentionToIncreasePoints = SocialExchangeSE.IntentionEnum.Romantic;
                    intentionToDecreasePoints = SocialExchangeSE.IntentionEnum.Romantic;
                    break;
                default:
                    break;
            }
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