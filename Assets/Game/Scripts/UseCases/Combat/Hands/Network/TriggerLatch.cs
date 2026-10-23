namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class TriggerLatch //TODO: кнопку, зажатую до того, как оружие оказалось в руках, отсекает только клиент: изменённый клиент может стрелять сразу после смены оружия
    {
        public bool Latched { get; private set; }

        private bool _heldOver;

        public void Begin(bool held)
        {
            Latched = false;
            _heldOver = held;
        }

        public void Latch()
        {
            Latched = true;
            _heldOver = false;
        }

        public void Clear() =>
            Latched = false;

        public bool Pulled(bool held) =>
            Latched || (held && !_heldOver);
    }
}
