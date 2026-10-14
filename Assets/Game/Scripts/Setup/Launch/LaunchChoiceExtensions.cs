namespace Setup.Launch
{
    public static class LaunchChoiceExtensions
    {
        public static string Endpoint(this LaunchChoice.Client client) =>
            $"{client.Address}:{client.Port}";
    }
}
