namespace Bannerlord_Mod_Test
{
    public class Status
    {
        public Status(string _statusName, double _intensity = 0)
        {
            this.statusName = _statusName;
            this.intensity = _intensity;
        }

        public string statusName { get; set; }
        public double intensity { get; set; }
    }
}
