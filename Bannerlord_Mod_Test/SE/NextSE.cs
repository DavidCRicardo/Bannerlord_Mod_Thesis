using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bannerlord_Mod_Test
{
    public class NextSE
    {
        //public NextSE(string _SEName, string _InitiatorName, string _ReceiverName, int _ReceiverId, int _Volition)
        public NextSE(string _SEName, CustomAgent _Initiator, CustomAgent _Receiver, int _Volition)
        {
            SEName = _SEName;
            //InitiatorName = _InitiatorName;
            //ReceiverName = _ReceiverName;
            //ReceiverId = _ReceiverId;
            InitiatorAgent = _Initiator;
            ReceiverAgent = _Receiver;
            Volition = _Volition;
        }
        public string SEName;
        public CustomAgent InitiatorAgent;
        public CustomAgent ReceiverAgent;
        public string InitiatorName;
        public string ReceiverName;
        public int ReceiverId;
        public int Volition;
    }
}
