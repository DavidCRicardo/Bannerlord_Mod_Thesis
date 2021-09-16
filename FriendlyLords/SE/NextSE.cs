namespace FriendlyLords
{
    public class NextSE
    {
        public NextSE(CIFManager.SEs_Enum _se, CIF_Character _Initiator, CIF_Character _Receiver, int _Volition)
        {
            se = _se;
            InitiatorAgent = _Initiator;
            ReceiverAgent = _Receiver;
            Volition = _Volition;
        }

        public CIFManager.SEs_Enum se;
        public CIF_Character InitiatorAgent;
        public CIF_Character ReceiverAgent;
        public int Volition;
    }
}