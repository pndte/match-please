using Bw.UseCases.Movement.Physics.Abstractions;

namespace Bw.UseCases.Movement.Physics
{
    public readonly struct ShovableContact
    {
        public readonly IShovable Shovable;
        public readonly float Gap;

        public ShovableContact(IShovable shovable, float gap)
        {
            Shovable = shovable;
            Gap = gap;
        }
    }
}
