using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class ShotTimer
    {
        public ISource<TimedShot> Timed => _timed;

        private readonly Signal<TimedShot> _timed = new();
        private readonly IWeapon _weapon;
        private readonly IWeaponCarriers _carriers;
        private readonly ICharacterTriggers _triggers;

        private float _viewDelay;
        private float _predictedDelay;
        private int _pressLateTicks;
        private int _previousShotAge = int.MaxValue;

        public ShotTimer(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponFeed feed,
            IWeaponCarriers carriers,
            ICharacterTriggers triggers)
        {
            _weapon = weapon;
            _carriers = carriers;
            _triggers = triggers;

            feed.Fed.Advise(lifetime, _ => Note(TriggerOfCarrier()));
            weapon.Fired.Advise(lifetime, Time);
        }

        private TriggerControls TriggerOfCarrier() =>
            _triggers.ByCharacter[_carriers.ByWeapon[_weapon]].Current;

        private void Note(TriggerControls trigger)
        {
            if (trigger.Pulled)
            {
                _viewDelay = trigger.ViewDelay;
                _predictedDelay = trigger.PredictedDelay;
            }

            if (trigger.Press.Age == 0)
                _pressLateTicks = trigger.Press.Late;

            if (_previousShotAge < int.MaxValue)
                _previousShotAge++;
        }

        private void Time(WeaponShot shot)
        {
            var lateTicks = Math.Min(_pressLateTicks, _previousShotAge - 1);
            _previousShotAge = lateTicks;
            _timed.Fire(new TimedShot(shot, new ShotTiming(_viewDelay, _predictedDelay, lateTicks), _carriers.ByWeapon[_weapon]));
        }
    }
}
