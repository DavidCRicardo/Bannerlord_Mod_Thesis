using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bannerlord_Mod_Test
{
    public class NextSE
    {
        public NextSE(string _SEName, string _InitiatorName, string _ReceiverName, int _Volition)
        {
            SEName = _SEName;
            InitiatorName = _InitiatorName;
            ReceiverName = _ReceiverName;
            Volition = _Volition;
        }
        public string SEName;
        public string InitiatorName;
        public string ReceiverName;
        public int Volition;
    }
}
