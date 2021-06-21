
namespace Bannerlord_Mod_Test
{
    public class mostWantedSE
    {
        public mostWantedSE(CustomAgent _customAgent, NextSE _nextSE)
        {
            customAgent = _customAgent;
            nextSE = _nextSE;
        }
        public string CustomAgentName;
        public CustomAgent customAgent;
        public int CustomAgentID;
        public NextSE nextSE;
    }
}