using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bannerlord_Mod_Test
{
    public class mostWantedSE
    {
        public mostWantedSE(string _customAgentName, NextSE _nextSE)
        {
            CustomAgentName = _customAgentName;
            nextSE = _nextSE;
        }
        public string CustomAgentName;
        public NextSE nextSE;
    }
}
