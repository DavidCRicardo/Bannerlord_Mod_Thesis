namespace Bannerlord_Social_AI
{
    public class Status
    {
        public Status(string _statusName, double _intensity = 0)
        {
            this.Name = _statusName;
            this.intensity = _intensity;
        }

        public string Name { get; set; }
        public double intensity { get; set; }
    }
}