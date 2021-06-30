namespace Bannerlord_Social_AI
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
        public int Volition;
    }
}