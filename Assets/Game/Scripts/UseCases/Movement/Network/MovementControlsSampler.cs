using Bw.Entities;
using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Movement.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementControlsSampler : IInputSampler<MovementControls>
    {
        private bool _jumpRequested;
        private bool _downRequested;
        private bool _upHeld;

        public MovementControlsSampler(Lifetime lifetime, IControlledBy controlledBy)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(UpdateButtons));
        }

        public MovementControls Sample()
        {
            var controls = new MovementControls(Input.GetAxisRaw("Horizontal"), _jumpRequested, UpHeld(), _downRequested || DownHeld()); //TODO: new input system
            _jumpRequested = false;
            _downRequested = false;
            return controls;
        }

        private void UpdateButtons(Unit _)
        {
            var upHeld = UpHeld();
            if (upHeld && !_upHeld)
                _jumpRequested = true;

            _upHeld = upHeld;
            if (DownHeld())
                _downRequested = true;
        }

        private static bool UpHeld() => //TODO: new input system
            Input.GetAxisRaw("Vertical") > 0f;

        private static bool DownHeld() => //TODO: new input system
            Input.GetAxisRaw("Vertical") < 0f;
    }
}
