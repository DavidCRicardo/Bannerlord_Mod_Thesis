namespace Bannerlord_Social_AI
{
    public class NextSE
    {
        public NextSE(string _SEName, CustomMissionNameMarkerVM.SEs_Enum _se, CustomAgent _Initiator, CustomAgent _Receiver, int _Volition)
        {
            SEName = _SEName;
            se = _se;
            InitiatorAgent = _Initiator;
            ReceiverAgent = _Receiver;
            Volition = _Volition;
        }
        public string SEName;
        public CustomMissionNameMarkerVM.SEs_Enum se;
        public CustomAgent InitiatorAgent;
        public CustomAgent ReceiverAgent;
        public int Volition;
    }
}