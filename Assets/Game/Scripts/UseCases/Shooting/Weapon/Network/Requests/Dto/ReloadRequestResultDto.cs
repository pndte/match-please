namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public readonly struct ReloadRequestResultDto
    {
        public readonly ulong RequestId;
        public readonly bool Accepted;

        public ReloadRequestResultDto(ulong requestId, bool accepted)
        {
            RequestId = requestId;
            Accepted = accepted;
        }
    }
}
