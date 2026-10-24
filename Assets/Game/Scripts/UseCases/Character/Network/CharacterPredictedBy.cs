using Bw.Entities;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterPredictedBy : IPredictedBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me = new(false);
        private readonly IReadonlyControlledBy _controlledBy;
        private readonly IBubbleMembership _membership;
        private readonly IReadonlyMortal _mortal;

        public CharacterPredictedBy(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IBubbleMembership membership,
            IReadonlyMortal mortal)
        {
            _controlledBy = controlledBy;
            _membership = membership;
            _mortal = mortal;

            controlledBy.Me.Advise(lifetime, _ => Settle());
            membership.Inside.Advise(lifetime, _ => Settle());
            mortal.State.Advise(lifetime, _ => Settle());
        }

        private void Settle() =>
            _me.Value = _mortal.State.Value == LifeState.Alive && (_controlledBy.Me.Value || _membership.Inside.Value);
    }
}
