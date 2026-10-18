using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Props.Extensions;
using UnityEngine;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class ThrowLaunch
    {
        private readonly Transform _item;
        private readonly ReleaseSpots _spots;
        private readonly PropThrowConfig _config;

        public ThrowLaunch(BodyBox shape, ReleaseSpots spots, PropThrowConfig config)
        {
            _item = shape.transform;
            _spots = spots;
            _config = config;
        }

        public RigidState FromHands(float aim) =>
            _config.Thrown(_spots.NearestToHolder(), aim, _item.HolderService<IReadonlyMovement>().State.Value.Body.Velocity);
    }
}
