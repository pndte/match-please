using System;
using System.Collections.Generic;
using Bw.UseCases.Character.View.Abstractions;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Character.View.Grips
{
    public sealed class GripView : IGripView
    {
        private readonly Lifetime _lifetime;
        private readonly SpriteRenderer[] _hands;

        private Lifetime _gripping = Lifetime.Terminated;

        public GripView(Lifetime lifetime, SpriteRenderer[] hands)
        {
            _lifetime = lifetime;
            _hands = hands;
        }

        public void Grip(Lifetime lifetime, IReadOnlyList<HandGrip> grips, SpriteRenderer held)
        {
            if (grips.Count > _hands.Length)
                throw new ArgumentException($"A holder has {_hands.Length} hands, yet the held item asks for {grips.Count} grips.", nameof(grips));

            if (_gripping.IsAlive)
                throw new InvalidOperationException("The hands already grip an item: an item is gripped only after the previous one is let go.");

            _gripping = _lifetime.Intersect(lifetime);
            Place(grips, held);
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, _gripping).Subscribe(_ => Place(grips, held));
            _gripping.OnTermination(Release);
        }

        private void Place(IReadOnlyList<HandGrip> grips, SpriteRenderer held)
        {
            for (var index = 0; index < _hands.Length; index++)
            {
                var hand = _hands[index];
                if (index >= grips.Count)
                {
                    hand.enabled = false;
                    continue;
                }

                var grip = grips[index];
                var pose = grip.transform;
                var scale = pose.lossyScale;
                if (pose.up.y < 0f)
                    scale.y = -scale.y;

                hand.transform.SetPositionAndRotation(pose.position, pose.rotation);
                hand.transform.localScale = scale;
                hand.sprite = grip.Hand;
                hand.sortingLayerID = held.sortingLayerID;
                hand.sortingOrder = grip.Behind ? held.sortingOrder - 1 : held.sortingOrder + 1;
                hand.enabled = true;
            }
        }

        private void Release()
        {
            foreach (var hand in _hands)
                hand.enabled = false;
        }
    }
}
