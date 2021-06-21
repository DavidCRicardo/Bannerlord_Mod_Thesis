using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bannerlord_Mod_Test
{
    public class mostWantedSE
    {
        //public mostWantedSE(string _customAgentName, int _customId, NextSE _nextSE)
        public mostWantedSE(CustomAgent _customAgent, NextSE _nextSE)
        {
            customAgent = _customAgent;
            //CustomAgentName = _customAgentName;
            //CustomAgentID = _customId;
            nextSE = _nextSE;
        }
        public string CustomAgentName;
        public CustomAgent customAgent;
        public int CustomAgentID;
        public NextSE nextSE;
    }
}
