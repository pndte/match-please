using Bw.Entities;
using Bw.Entities.Network.Prediction;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementInputSampler : IInputSampler<MovementInput>
    {
        private bool _jumpRequested;

        public MovementInputSampler(Lifetime lifetime, IReadonlyControlledBy controlledBy)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(UpdateJump));
        }

        public MovementInput Sample()
        {
            var input = new MovementInput(Input.GetAxisRaw("Horizontal"), _jumpRequested); //TODO: new input system
            _jumpRequested = false;
            return input;
        }

        private void UpdateJump(Unit _)
        {
            if (Input.GetKeyDown(KeyCode.Space)) //TODO: new input system
                _jumpRequested = true;
        }
    }
}
