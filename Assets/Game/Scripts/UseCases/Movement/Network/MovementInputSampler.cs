using Bw.Entities;
using Bw.Entities.Network.Prediction.Stream;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementInputSampler : IInputSampler<MovementInput>
    {
        private bool _jumpRequested;
        private bool _downRequested;

        public MovementInputSampler(Lifetime lifetime, IControlledBy controlledBy)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(UpdateButtons));
        }

        public MovementInput Sample()
        {
            var input = new MovementInput(Input.GetAxisRaw("Horizontal"), _jumpRequested, Input.GetKey(KeyCode.Space), _downRequested || DownHeld()); //TODO: new input system
            _jumpRequested = false;
            _downRequested = false;
            return input;
        }

        private void UpdateButtons(Unit _)
        {
            if (Input.GetKeyDown(KeyCode.Space)) //TODO: new input system
                _jumpRequested = true;

            if (DownHeld())
                _downRequested = true;
        }

        private static bool DownHeld() => //TODO: new input system
            Input.GetAxisRaw("Vertical") < 0f;
    }
}
