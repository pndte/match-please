namespace Entities.Network
{
    public class HealthDataConnector
    {
        public HealthDataConnector(IHealth health, HealthConfig healthConfig)
        {
            health.Value = healthConfig.Max;
        }
    }
}