using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    public sealed class CharacterRig : MonoBehaviour
    {
        private const float SettledAge = 10f;

        private static readonly int BendId = Shader.PropertyToID("_Bend");
        private static readonly int BendUvPerUnitId = Shader.PropertyToID("_BendUvPerUnit");
        private static readonly int BendRectId = Shader.PropertyToID("_BendRect");
        private static readonly int BendTopId = Shader.PropertyToID("_BendTop");
        private static readonly int BendSpanId = Shader.PropertyToID("_BendSpan");
        private static readonly int BendPowerId = Shader.PropertyToID("_BendPower");

        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterRigLook _look;
        [SerializeField] private CharacterRigFeel _feel;
        [SerializeField] private CharacterRigParts _parts;
        [SerializeField] private float _turn;
        [SerializeField] private float _hairDx;
        [SerializeField] private float _hairDy;
        [SerializeField] private float _bob;
        [SerializeField] private float _expression;
        [SerializeField] private float _decal;

        private RigPose _pose;
        private RigSpring _hair;
        private MaterialPropertyBlock _hairBlock;
        private int _groundedId;
        private int _verticalSpeedId;
        private int _state;
        private float _clock;
        private float _holdStart;
        private float _holdEnd;
        private bool _grounded;
        private float _fall;
        private float _impact;
        private float _liftAge = SettledAge;
        private float _landAge = SettledAge;

        private void Awake()
        {
            _pose = new RigPose(_parts.Posed, _parts.Drawn);
            _hair = new RigSpring(HairTarget());
            _hairBlock = new MaterialPropertyBlock();
            _groundedId = Animator.StringToHash(_feel.GroundedParameter);
            _verticalSpeedId = Animator.StringToHash(_feel.VerticalSpeedParameter);
            _grounded = _animator.GetBool(_groundedId);
        }

        private void LateUpdate() => Step(Time.deltaTime);

        private void Step(float deltaTime)
        {
            _clock += deltaTime;
            Track(deltaTime);
            var hairTarget = HairTarget();
            if ((hairTarget - _hair.Target).magnitude > _feel.HairSnap)
                _hair = new RigSpring(hairTarget);

            _hair.Step(hairTarget, _feel.HairFrequency, _feel.HairDamping, deltaTime);
            var state = TargetState();
            if (state != _state)
            {
                _state = state;
                _holdStart = _clock;
                _holdEnd = _clock;
            }

            if (!_feel.Held || _clock >= _holdEnd)
                Pose();

            _pose.Apply();
            ShowBend(_pose.Bend);
        }

        private void Track(float deltaTime)
        {
            _liftAge = Mathf.Min(_liftAge + deltaTime, SettledAge);
            _landAge = Mathf.Min(_landAge + deltaTime, SettledAge);
            var grounded = _animator.GetBool(_groundedId);
            var verticalSpeed = _animator.GetFloat(_verticalSpeedId);
            if (grounded && !_grounded)
            {
                _landAge = 0f;
                _impact = Mathf.Clamp01(-_fall / _feel.FullImpact);
            }

            if (!grounded && _grounded && verticalSpeed > 0f)
                _liftAge = 0f;

            if (!grounded)
                _fall = verticalSpeed;

            _grounded = grounded;
        }

        private void Pose()
        {
            _holdEnd = _holdStart + (Mathf.Floor((_clock - _holdStart) * _feel.HoldsPerSecond) + 1f) / _feel.HoldsPerSecond;
            Dress((int)(_clock * _feel.LinesPerSecond) % _look.Eggs.Length);
            var squash = 1f + Squash();
            var body = _parts.Body.localScale;
            _parts.Body.localScale = new Vector3(body.x * squash, body.y / squash, body.z);
            RigLegPlacer.Place(_parts.Far, transform, _look);
            RigLegPlacer.Place(_parts.Near, transform, _look);
            var sway = HairSway();
            Flap(sway.y);
            //TODO: в воздухе тень пропадает вместе с клипом; в прототипе она остаётся на полу и тает с высотой, для этого нужна высота над полом
            _pose.Capture(HairBend(sway));
        }

        private void Dress(int line)
        {
            var turn = _look.Turns[Mathf.RoundToInt(_turn)];
            var decal = _look.Decals[Mathf.RoundToInt(_decal)];
            _parts.Egg.sprite = _look.Eggs[line];
            _parts.Skin.sprite = decal.Skins[line];
            _parts.Face.sprite = turn.Faces[Mathf.RoundToInt(_expression)].Lines[line];
            _parts.Hair.sprite = turn.Hairs[line];
            _parts.Back.sprite = turn.Backs[line];
            _parts.Over.sprite = decal.Overs[line];
            for (var i = 0; i < _look.Flaps.Length; i++)
                _parts.Flaps[i].sprite = _look.Flaps[i].Lines[line];

            _parts.Shadow.sprite = _look.Shadows[line];
            _parts.Far.Shoe.sprite = _look.FarShoes[line];
            _parts.Far.Leg.sprite = _look.Legs[line];
            _parts.Far.Tick.sprite = _look.Ticks[line];
            _parts.Near.Shoe.sprite = _look.NearShoes[line];
            _parts.Near.Leg.sprite = _look.Legs[line];
            _parts.Near.Tick.sprite = _look.Ticks[line];
        }

        private float Squash() =>
            -_feel.LiftStretch * Mathf.Exp(-_liftAge / _feel.LiftDecay)
            + _feel.LandSquash * _impact * Mathf.Exp(-_landAge / _feel.LandDecay) * Mathf.Cos(2f * Mathf.PI * _feel.LandFrequency * _landAge);

        private Vector2 HairTarget()
        {
            var position = transform.position * _look.PixelsPerUnit;
            return new Vector2(position.x * _feel.HairFollow.x, -position.y * _feel.HairFollow.y);
        }

        private Vector2 HairSway()
        {
            var bend = _look.HairBend;
            var offset = _hair.Offset;
            var facing = Mathf.Sign(transform.lossyScale.x);
            return new Vector2(
                _hairDx + Mathf.Clamp(offset.x * facing, -_feel.HairLimit, _feel.HairLimit) / bend.DxGain,
                _hairDy + Mathf.Clamp(offset.y, -_feel.HairLimit, _feel.HairLimit) / bend.DyGain);
        }

        private Vector2 HairBend(Vector2 sway) =>
            new Vector2(sway.x * _look.HairBend.DxGain, -sway.y * _look.HairBend.DyGain) / _look.PixelsPerUnit;

        private void Flap(float hairDy)
        {
            var droop = _look.FlapHairGain * hairDy + _look.FlapBobGain * _bob;
            var angle = Mathf.Atan2(droop, _look.FlapReach) * Mathf.Rad2Deg;
            for (var i = 0; i < _look.Flaps.Length; i++)
                _parts.Flaps[i].transform.localRotation = Quaternion.Euler(0f, 0f, -_look.Flaps[i].Side * angle);
        }

        private void ShowBend(Vector2 bend)
        {
            var hair = _parts.Hair;
            var sprite = hair.sprite;
            var size = new Vector2(sprite.texture.width, sprite.texture.height);
            var rect = sprite.textureRect;
            hair.GetPropertyBlock(_hairBlock);
            _hairBlock.SetVector(BendId, bend);
            _hairBlock.SetVector(BendUvPerUnitId, new Vector2(sprite.pixelsPerUnit, sprite.pixelsPerUnit) / size);
            _hairBlock.SetVector(BendRectId, new Vector4(
                (rect.xMin + 0.5f) / size.x,
                (rect.yMin + 0.5f) / size.y,
                (rect.xMax - 0.5f) / size.x,
                (rect.yMax - 0.5f) / size.y));
            _hairBlock.SetFloat(BendTopId, _look.HairBend.Top);
            _hairBlock.SetFloat(BendSpanId, _look.HairBend.Span);
            _hairBlock.SetFloat(BendPowerId, _look.HairBend.Power);
            hair.SetPropertyBlock(_hairBlock);
        }

        private int TargetState() =>
            _animator.IsInTransition(0)
                ? _animator.GetNextAnimatorStateInfo(0).fullPathHash
                : _animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }
}
