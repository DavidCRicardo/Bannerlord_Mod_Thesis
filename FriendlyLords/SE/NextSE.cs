namespace FriendlyLords
{
    public class NextSE
    {
        public NextSE(CustomMissionNameMarkerVM.SEs_Enum _se, CustomAgent _Initiator, CustomAgent _Receiver, int _Volition)
        {
            se = _se;
            InitiatorAgent = _Initiator;
            ReceiverAgent = _Receiver;
            Volition = _Volition;
        }

        public CustomMissionNameMarkerVM.SEs_Enum se;
        public CustomAgent InitiatorAgent;
        public CustomAgent ReceiverAgent;
        public int Volition;
    }
}