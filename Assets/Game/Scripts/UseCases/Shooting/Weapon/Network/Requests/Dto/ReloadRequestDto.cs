namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public readonly struct ReloadRequestDto
    {
        public readonly ulong RequestId;

        public ReloadRequestDto(ulong requestId)
        {
            RequestId = requestId;
        }
    }
}
