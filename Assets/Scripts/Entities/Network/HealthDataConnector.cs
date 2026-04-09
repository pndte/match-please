namespace Bw.Entities.Network
{
    public class HealthDataConnector
    {
        public HealthDataConnector(IHealth health, HealthConfig healthConfig)
        {
            health.Current.Value = healthConfig.Max;
        }
    }
}