namespace Bannerlord_Mod_Test
{
    public class NextSE
    {
        public NextSE(string _SEName, CustomAgent _Initiator, CustomAgent _Receiver, int _Volition)
        {
            SEName = _SEName;
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