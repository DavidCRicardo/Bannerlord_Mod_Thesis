﻿
namespace Bannerlord_Social_AI
{
    public class mostWantedSE
    {
        public mostWantedSE(CustomAgent _customAgent, NextSE _nextSE)
        {
            customAgent = _customAgent;
            nextSE = _nextSE;
        }

        public CustomAgent customAgent;
        public NextSE nextSE;
    }
}