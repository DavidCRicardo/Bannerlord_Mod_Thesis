﻿using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace FriendlyLords
{
    class CustomMessageNPC
    {
        public CustomMessageNPC(CIF_SocialExchange se, Random rnd, bool _isInitiator, CultureCode culture, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation, float receiverVolition = 0)
        {
            SE = se;
            Rnd = rnd;
            IsInitiator = _isInitiator;
            CurrentCulture = culture;
            ReceiverVolition = receiverVolition;
            DialogsDictionary = _dialogsDictionary;
            CurrentLocation = _CurrentLocation;
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
            char delimiterChar = '/';
            sentences = Message.Split(delimiterChar);

            return sentences; 
        }

        internal string[] CheckSocialMoveReceiver()
        {
            SetIsAccepted();
            string id = (IsAccepted) ? "accept" : "reject";

            Message = GetAMessage(SE, CurrentCulture, id, Rnd);
            char delimiterChar = '/';
            sentences = Message.Split(delimiterChar);

            return sentences;
        }

        private void SetIsAccepted()
        {
            IsAccepted = (ReceiverVolition > 0) ? true : false;
        }

        private CIF_SocialExchange SE { get; }
        private string Message { get; set; }
        private Random Rnd { get; set; }
        private bool IsInitiator { get; }
        public float ReceiverVolition { get; }
        private Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> DialogsDictionary { get; }

        public bool IsAccepted { get; private set; }
        public string[] sentences { get; private set; }
        private CultureCode CurrentCulture { get; set; }
        private string CurrentLocation { get; set; }

        private string GetAMessage(CIF_SocialExchange SE, CultureCode culture, string id, Random rnd)
        {
            if (culture == CultureCode.Invalid)
            {
                culture = CultureCode.AnyOtherCulture;
            }

            bool containsIntention = DialogsDictionary.TryGetValue(SE.SE_Enum.ToString(), out Dictionary<string, Dictionary<string, List<string>>> dictionaryWithCultures);
            if (containsIntention)
            {
                bool containsCulture = dictionaryWithCultures.TryGetValue(culture.ToString(), out Dictionary<string, List<string>> NPCDialogs);
                if (containsCulture)
                {
                    bool containsMessageList = NPCDialogs.TryGetValue(id, out List<string> SentencesList);
                    if (containsMessageList)
                    {
                        int i = rnd.Next(SentencesList.Count);
                        string RandomMessage = SentencesList[i];
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