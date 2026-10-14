using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.FogOfWar.View.Fog;
using Bw.UseCases.FogOfWar.View.Sight;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.FogOfWar.View
{
    public sealed class FogOfWarView //TODO: туман только рисуется: сервер шлёт клиенту всех, и клиент со снятым туманом видит сквозь стены; честная видимость — отсев на сервере (NGO CheckObjectVisibility)
    {
        private readonly ICameraTargets _targets;
        private readonly UnityEngine.Camera _camera;
        private readonly FogOfWarConfig _config;
        private readonly LineOfSight _sight;
        private readonly SightMask _mask;
        private readonly FogCover _fog;

        private FogOfWarView(
            Lifetime lifetime,
            ICameraTargets targets,
            UnityEngine.Camera camera,
            FogOfWarConfig config,
            LineOfSight sight,
            SightMask mask,
            FogCover fog)
        {
            _targets = targets;
            _camera = camera;
            _config = config;
            _sight = sight;
            _mask = mask;
            _fog = fog;

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(Draw);
        }

        public static FogOfWarView Create(
            Lifetime lifetime,
            UnityEngine.Camera camera,
            ICameraTargets targets,
            FogOfWarConfig config)
        {
            var area = ViewArea(camera, config);
            var sight = new LineOfSight(camera.gameObject.scene.GetPhysicsScene2D(), config);
            var mask = SightMask.Create(lifetime, MaskSize(area, config), config); //TODO: размер маски берётся от экрана один раз: после смены разрешения или пропорций окна её тексели растягиваются и край тумана становится мягче
            var fog = FogCover.Create(lifetime, camera, mask, config);
            fog.Cover(area);
            return new FogOfWarView(lifetime, targets, camera, config, sight, mask, fog);
        }

        private void Draw(Unit _)
        {
            if (_targets.Count == 0) //TODO: пока своего тела нет (труп убран, ждём респауна), туман застывает на последнем кадре: тени движущихся платформ не обновляются, а маска, потерянная при сбросе видеокарты, до респауна покажет мусор
                return;

            var area = ViewArea(_camera, _config);
            var eye = (Vector2)_targets[_targets.Count - 1].position + _config.EyeOffset;
            _sight.Trace(eye, area);
            _mask.Draw(_sight, eye, area);
            _fog.Cover(area);
        }

        private static Rect ViewArea(UnityEngine.Camera camera, FogOfWarConfig config)
        {
            var half = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize) + Vector2.one * config.ViewMargin;
            return new Rect((Vector2)camera.transform.position - half, half * 2f);
        }

        private static Vector2Int MaskSize(Rect area, FogOfWarConfig config) =>
            new(Mathf.CeilToInt(area.width * config.MaskTexelsPerUnit), Mathf.CeilToInt(area.height * config.MaskTexelsPerUnit));
    }
}
