namespace Bw.Entities.Network
{
    public abstract class LocalPlayerId
    {
        public static readonly LocalPlayerId Unknown = new UnknownId();

        private LocalPlayerId()
        {
        }

        public static LocalPlayerId Of(int id) =>
            new KnownId(id);

        public abstract bool IsKnown { get; }

        public abstract bool Is(int id);

        private sealed class UnknownId : LocalPlayerId
        {
            public override bool IsKnown => false;

            public override bool Is(int id) =>
                false;
        }

        private sealed class KnownId : LocalPlayerId
        {
            public override bool IsKnown => true;

            private readonly int _id;

            public KnownId(int id)
            {
                _id = id;
            }

            public override bool Is(int id) =>
                _id == id;
        }
    }
}
