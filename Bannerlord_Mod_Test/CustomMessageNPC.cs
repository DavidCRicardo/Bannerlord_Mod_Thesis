﻿using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace Bannerlord_Mod_Test
{
    class CustomMessageNPC
    {
        public CustomMessageNPC(SocialExchangeSE se, Random rnd, bool _isInitiator, CultureCode culture, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary, int receiverVolition = 0)
        {
            SE = se;
            Rnd = rnd;
            IsInitiator = _isInitiator;
            CurrentCulture = culture;
            ReceiverVolition = receiverVolition;
            MegaDictionary = megaDictionary;
        }

        public string[] MainSocialMove()
        {
            if (IsInitiator)
            {
                return CheckSocialMoveInitiator();
            }
            else
            {
                return CheckSocialMoveReceiver();
            }
        }
        internal string[] CheckSocialMoveInitiator()
        {
            Message = GetAMessage(SE, CurrentCulture, "start", Rnd);

            //char[] delimiterChars = { '.', '!', '?' };
            char delimiterChar = '/';
            sentences = Message.Split(delimiterChar);

            return sentences; // Message
        }
        internal string[] CheckSocialMoveReceiver()
        {
            SetIsAccepted();
            string id = (IsAccepted) ? "accept" : "reject";

            Message = GetAMessage(SE, CurrentCulture, id, Rnd);
            //char[] delimiterChars = { '.', '!', '?' };
            char delimiterChar = '/';
            sentences = Message.Split(delimiterChar);
            return sentences; // Message
        }
        private void SetIsAccepted()
        {
            IsAccepted = (ReceiverVolition > 0) ? true : false;
        }
        private SocialExchangeSE SE { get; }
        private string Message { get; set; }
        private Random Rnd { get; set; }
        private bool IsInitiator { get; }
        public int ReceiverVolition { get; }
        private Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> MegaDictionary { get; }
        public bool IsAccepted { get; private set; }
        public string[] sentences { get; private set; }
        private CultureCode CurrentCulture { get; set; }

        private string GetAMessage(SocialExchangeSE SE, CultureCode culture, string id, Random rnd)
        {
            if (culture == CultureCode.Invalid)
            {
                culture = CultureCode.AnyOtherCulture;
            }

            bool containsIntention = MegaDictionary.TryGetValue(SE.SEName, out Dictionary<string, Dictionary<string, List<string>>> a);
            if (containsIntention)
            {
                bool containsCulture = a.TryGetValue(culture.ToString(), out Dictionary<string, List<string>> b);

                if (containsCulture)
                {
                    bool containsMessageList = b.TryGetValue(id, out List<string> aa);

                    if (containsMessageList)
                    {
                        int i = rnd.Next(aa.Count);
                        string RandomMessage = aa[i];
                        return RandomMessage;
                    }
                    else { return "Hi there"; }
                }
                else { return "Hi there"; }
            }
            else { return "Hi there"; }

        }
    }
}