using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    public sealed class WeaponFeelPreview : IDisposable
    {
        private const float StageHeight = 240f;
        private const float PlotHeight = 46f;
        private const float BarrelPlotHeight = 72f;
        private const float TargetDistance = 6f;
        private const float TargetHalfHeight = 1.6f;
        private const float FlashTime = 0.05f;
        private const float TraceFadeTime = 1.5f;
        private const float CursorRing = 6f;
        private const float CursorTick = 5f;
        private const float CursorGap = 3f;
        private const float MaxInterval = 2f;
        private const float MinPlotDegrees = 0.5f;

        private static readonly Color StageColor = new(0.11f, 0.12f, 0.14f);
        private static readonly Color GridColor = new(1f, 1f, 1f, 0.05f);
        private static readonly Color BodyColor = new(0.33f, 0.37f, 0.45f);
        private static readonly Color TargetColor = new(1f, 1f, 1f, 0.18f);
        private static readonly Color AimColor = new(1f, 1f, 1f, 0.3f);
        private static readonly Color BarrelColor = new(1f, 0.45f, 0.35f, 0.45f);
        private static readonly Color TraceColor = new(1f, 0.9f, 0.45f);
        private static readonly Color ImpactColor = new(1f, 0.55f, 0.3f);
        private static readonly Color FlashColor = new(1f, 0.9f, 0.45f, 0.9f);
        private static readonly Color CursorColor = new(0.45f, 0.95f, 0.6f);
        private static readonly Color GunColor = new(0.78f, 0.8f, 0.86f);
        private static readonly Color KickColor = new(0.4f, 0.7f, 1f);
        private static readonly Color TiltColor = new(0.98f, 0.62f, 0.25f);
        private static readonly Color BarrelPlotColor = new(1f, 0.45f, 0.35f);
        private static readonly Color PlotColor = new(0f, 0f, 0f, 0.2f);
        private static readonly Color PlayheadColor = new(1f, 1f, 1f, 0.75f);
        private static readonly Color TickColor = new(1f, 1f, 1f, 0.12f);
        private static readonly GUIContent[] Speeds = { new("¼×"), new("½×"), new("1×") };
        private static readonly float[] SpeedScales = { 0.25f, 0.5f, 1f };
        private static readonly Matrix4x4 Flip = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));
        private static readonly GUIContent IntervalLabel = new("Trigger Every (s)", "Time between trigger pulls. 0 keeps the trigger held: the gun fires as fast as its cooldown allows.");

        private static int _burst = 6;
        private static float _interval;
        private static int _speed = 2;
        private static bool _loop;

        public PreviewWeapon Weapon { get; }
        public bool Playing { get; private set; }

        private readonly PreviewStep _step;
        private readonly Action _repaint;

        private int _shots = 1;
        private double _startedAt;
        private float _time;
        private float[] _inputs;
        private PreviewTrack _track;

        public WeaponFeelPreview(PreviewWeapon weapon, PreviewStep step, Action repaint)
        {
            Weapon = weapon;
            _step = step;
            _repaint = repaint;
            _inputs = Inputs();
            _track = PreviewTrack.Simulate(weapon, step, _shots, _interval);
        }

        public void Draw()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            DrawControls();

            var track = Track();
            Advance(track);
            DrawStage(track);
            DrawPlot(track, track.Kick, "Kick", "u", KickColor);
            DrawPlot(track, track.Tilt, "Tilt", "°", TiltColor);
            DrawBarrelPlot(track);
        }

        public void Dispose() =>
            Weapon.Dispose();

        private void DrawControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶ Shot"))
                    Play(1);

                if (GUILayout.Button($"▶ Burst ×{_burst}"))
                    Play(_burst);

                _loop = GUILayout.Toggle(_loop, "Loop", EditorStyles.miniButton, GUILayout.Width(48f));
                _speed = GUILayout.Toolbar(_speed, Speeds, EditorStyles.miniButton, GUILayout.Width(96f));
            }

            _burst = EditorGUILayout.IntSlider("Burst Shots", _burst, 1, 30);
            using (new EditorGUILayout.HorizontalScope())
            {
                _interval = Mathf.Clamp(EditorGUILayout.FloatField(IntervalLabel, _interval), 0f, MaxInterval);
                if (GUILayout.Button("Hold", EditorStyles.miniButton, GUILayout.Width(52f)))
                    _interval = 0f;
            }
        }

        private PreviewTrack Track()
        {
            var inputs = Inputs();
            if (inputs.SequenceEqual(_inputs))
                return _track;

            _inputs = inputs;
            _track = PreviewTrack.Simulate(Weapon, _step, _shots, _interval);
            return _track;
        }

        private float[] Inputs()
        {
            var shooting = Weapon.Shooting.Config;
            var recoil = Weapon.Recoil;
            return new[]
            {
                _step.Duration, _shots, _interval, Weapon.Shooting.Rotation.RotationSpeed,
                shooting.ShootCooldown, shooting.ReloadTime, shooting.AmmoSettings.Max, shooting.Spread.Climb, shooting.Spread.Jitter,
                recoil.Kick.Amount, recoil.Kick.OutTime, recoil.Kick.ReturnTime, recoil.Kick.Overshoot,
                recoil.Tilt.Amount, recoil.Tilt.OutTime, recoil.Tilt.ReturnTime, recoil.Tilt.Overshoot
            };
        }

        private void Play(int shots)
        {
            _shots = shots;
            Playing = true;
            _startedAt = EditorApplication.timeSinceStartup;
        }

        private void Advance(PreviewTrack track)
        {
            if (!Playing)
            {
                _time = Mathf.Min(_time, track.Duration);
                return;
            }

            _time = (float)((EditorApplication.timeSinceStartup - _startedAt) * SpeedScales[_speed]);
            if (_time <= track.Duration)
                return;

            if (_loop)
            {
                _startedAt = EditorApplication.timeSinceStartup;
                _time = 0f;
                return;
            }

            Playing = false;
            _time = track.Duration;
        }

        private void DrawStage(PreviewTrack track)
        {
            var stage = GUILayoutUtility.GetRect(10f, StageHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(stage, StageColor);
            var scale = Mathf.Min((stage.width - 40f) / (TargetDistance + 1.5f), (stage.height - 28f) / (2f * TargetHalfHeight + 0.4f));
            var origin = new Vector2(stage.x + 20f + 0.7f * scale, stage.y + stage.height * 0.5f);
            var worldToGui = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3(scale, -scale, 1f));
            var frame = track.FrameAt(_time);
            var barrel = track.Barrel[frame];

            DrawGrid(stage, origin, scale);
            DrawBody(worldToGui);
            DrawTarget(worldToGui);
            DrawAim(worldToGui);
            DrawTraces(track, worldToGui);
            DrawBarrel(worldToGui, barrel);
            DrawGun(track, frame, worldToGui, barrel);
            DrawFlash(track, frame, worldToGui, barrel);
            DrawCursor(worldToGui, track.Cursor[frame]);
            DrawCaption(stage, track, barrel);
        }

        private static void DrawGrid(Rect stage, Vector2 origin, float scale)
        {
            Handles.color = GridColor;
            var step = scale * 0.5f;
            for (var x = origin.x % step; x < stage.width; x += step)
                Handles.DrawLine(new Vector3(stage.x + x, stage.y), new Vector3(stage.x + x, stage.yMax));

            for (var y = origin.y; y > stage.y; y -= step)
                Handles.DrawLine(new Vector3(stage.x, y), new Vector3(stage.xMax, y));

            for (var y = origin.y + step; y < stage.yMax; y += step)
                Handles.DrawLine(new Vector3(stage.x, y), new Vector3(stage.xMax, y));
        }

        private static void DrawBody(Matrix4x4 worldToGui)
        {
            var bottomLeft = worldToGui.MultiplyPoint3x4(new Vector3(-0.5f, -1f));
            var topRight = worldToGui.MultiplyPoint3x4(new Vector3(0.5f, 1f));
            EditorGUI.DrawRect(Rect.MinMaxRect(bottomLeft.x, topRight.y, topRight.x, bottomLeft.y), BodyColor);
        }

        private static void DrawTarget(Matrix4x4 worldToGui)
        {
            Handles.color = TargetColor;
            Handles.DrawDottedLine(
                worldToGui.MultiplyPoint3x4(new Vector3(TargetDistance, -TargetHalfHeight)),
                worldToGui.MultiplyPoint3x4(new Vector3(TargetDistance, TargetHalfHeight)),
                3f);
        }

        private static void DrawAim(Matrix4x4 worldToGui)
        {
            var aimPoint = worldToGui.MultiplyPoint3x4(new Vector3(TargetDistance, 0f));
            Handles.color = AimColor;
            Handles.DrawDottedLine(worldToGui.MultiplyPoint3x4(Vector3.zero), aimPoint, 3f);
            DrawCrosshair(aimPoint, AimColor);
        }

        private void DrawTraces(PreviewTrack track, Matrix4x4 worldToGui)
        {
            foreach (var shot in track.Shots)
            {
                if (shot.Time > _time)
                    break;

                var muzzle = worldToGui.MultiplyPoint3x4(MuzzleAt(shot.Aim));
                var impact = worldToGui.MultiplyPoint3x4(OnTarget(shot.Aim));
                var fade = Mathf.Clamp01((_time - shot.Time) / TraceFadeTime);
                Handles.color = new Color(TraceColor.r, TraceColor.g, TraceColor.b, Mathf.Lerp(0.85f, 0.08f, fade));
                Handles.DrawAAPolyLine(2f, muzzle, impact);
                Handles.color = ImpactColor;
                Handles.DrawSolidDisc(impact, Vector3.forward, 2.5f);
            }
        }

        private void DrawBarrel(Matrix4x4 worldToGui, float barrel)
        {
            Handles.color = BarrelColor;
            Handles.DrawAAPolyLine(1.5f, worldToGui.MultiplyPoint3x4(MuzzleAt(barrel)), worldToGui.MultiplyPoint3x4(OnTarget(barrel)));
        }

        private void DrawGun(PreviewTrack track, int frame, Matrix4x4 worldToGui, float barrel)
        {
            var visual = VisualAt(track, frame, barrel);
            foreach (var part in Weapon.Parts)
                DrawSprite(part.Sprite, worldToGui * visual * part.ToVisual);

            if (Weapon.Parts.Count == 0)
                DrawStandInGun(worldToGui * visual);
        }

        private void DrawFlash(PreviewTrack track, int frame, Matrix4x4 worldToGui, float barrel)
        {
            if (track.SinceShot(_time) > FlashTime)
                return;

            Handles.color = FlashColor;
            Handles.DrawSolidDisc(worldToGui.MultiplyPoint3x4(VisualAt(track, frame, barrel).MultiplyPoint3x4(Weapon.Muzzle)), Vector3.forward, 5f);
        }

        private Matrix4x4 VisualAt(PreviewTrack track, int frame, float barrel)
        {
            var rotation = Quaternion.Euler(0f, 0f, barrel);
            return Matrix4x4.TRS(rotation * new Vector3(Weapon.OrbitRadius, 0f), rotation, Vector3.one) *
                   Matrix4x4.TRS(new Vector3(-track.Kick[frame], 0f), Quaternion.Euler(0f, 0f, track.Tilt[frame]), Vector3.one);
        }

        private static void DrawCursor(Matrix4x4 worldToGui, float cursor) =>
            DrawCrosshair(worldToGui.MultiplyPoint3x4(Quaternion.Euler(0f, 0f, cursor) * new Vector3(TargetDistance, 0f)), CursorColor);

        private static void DrawCrosshair(Vector3 center, Color color)
        {
            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.forward, CursorRing);
            Handles.DrawAAPolyLine(2f, center + Vector3.up * (CursorRing + CursorGap), center + Vector3.up * (CursorRing + CursorGap + CursorTick));
            Handles.DrawAAPolyLine(2f, center + Vector3.down * (CursorRing + CursorGap), center + Vector3.down * (CursorRing + CursorGap + CursorTick));
            Handles.DrawAAPolyLine(2f, center + Vector3.left * (CursorRing + CursorGap), center + Vector3.left * (CursorRing + CursorGap + CursorTick));
            Handles.DrawAAPolyLine(2f, center + Vector3.right * (CursorRing + CursorGap), center + Vector3.right * (CursorRing + CursorGap + CursorTick));
        }

        private Vector3 MuzzleAt(float degrees) =>
            Quaternion.Euler(0f, 0f, degrees) * new Vector3(Weapon.OrbitRadius + Weapon.Muzzle.x, Weapon.Muzzle.y);

        private Vector3 OnTarget(float degrees)
        {
            var muzzle = MuzzleAt(degrees);
            var direction = Quaternion.Euler(0f, 0f, degrees) * Vector3.right;
            return muzzle + direction * ((TargetDistance - muzzle.x) / direction.x);
        }

        private static void DrawSprite(Sprite sprite, Matrix4x4 spriteToGui)
        {
            var texture = sprite.texture;
            var rect = sprite.textureRect;
            var pivot = sprite.pivot / sprite.pixelsPerUnit;
            var size = rect.size / sprite.pixelsPerUnit;
            var uv = new Rect(rect.x / texture.width, rect.y / texture.height, rect.width / texture.width, rect.height / texture.height);

            var previous = GUI.matrix;
            GUI.matrix = previous * spriteToGui * Flip;
            GUI.DrawTextureWithTexCoords(new Rect(-pivot.x, pivot.y - size.y, size.x, size.y), texture, uv, true);
            GUI.matrix = previous;
        }

        private static void DrawStandInGun(Matrix4x4 visualToGui)
        {
            Handles.color = GunColor;
            Handles.DrawAAConvexPolygon(
                visualToGui.MultiplyPoint3x4(new Vector3(-0.15f, -0.12f)),
                visualToGui.MultiplyPoint3x4(new Vector3(0.5f, -0.12f)),
                visualToGui.MultiplyPoint3x4(new Vector3(0.5f, 0.1f)),
                visualToGui.MultiplyPoint3x4(new Vector3(-0.15f, 0.1f)));
            Handles.DrawAAConvexPolygon(
                visualToGui.MultiplyPoint3x4(new Vector3(0.5f, -0.04f)),
                visualToGui.MultiplyPoint3x4(new Vector3(1.1f, -0.04f)),
                visualToGui.MultiplyPoint3x4(new Vector3(1.1f, 0.04f)),
                visualToGui.MultiplyPoint3x4(new Vector3(0.5f, 0.04f)));
        }

        private void DrawCaption(Rect stage, PreviewTrack track, float barrel)
        {
            var caption = $"{Weapon.Name}   {_time:0.00} s   shots {track.FiredBy(_time)}/{_shots}   barrel {barrel:+0.0;-0.0;0.0}°";
            GUI.Label(new Rect(stage.x + 6f, stage.y + 4f, stage.width - 12f, 18f), caption, EditorStyles.whiteMiniLabel);
        }

        private void DrawPlot(PreviewTrack track, IReadOnlyList<float> values, string title, string unit, Color color)
        {
            var rect = GUILayoutUtility.GetRect(10f, PlotHeight, GUILayout.ExpandWidth(true));
            Scrub(rect, track);
            if (Event.current.type != EventType.Repaint)
                return;

            var peak = Peak(values, 0.0001f);
            var zero = rect.y + rect.height * 0.62f;
            var amplitude = rect.height * 0.5f;
            DrawPlotFrame(rect, track, zero);

            var points = new Vector3[values.Count];
            for (var index = 0; index < values.Count; index++)
                points[index] = new Vector3(XAt(rect, track, index * PreviewTrack.Frame), zero - amplitude * values[index] / peak);

            Handles.color = color;
            Handles.DrawAAPolyLine(2f, points);
            DrawPlayhead(rect, track);

            var now = values[track.FrameAt(_time)];
            GUI.Label(new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, 16f), $"{title}   now {now:0.00}{unit}   peak {peak:0.00}{unit}", EditorStyles.miniLabel);
        }

        private void DrawBarrelPlot(PreviewTrack track)
        {
            var rect = GUILayoutUtility.GetRect(10f, BarrelPlotHeight, GUILayout.ExpandWidth(true));
            Scrub(rect, track);
            if (Event.current.type != EventType.Repaint)
                return;

            var peak = Peak(track.Barrel, MinPlotDegrees);
            var zero = rect.y + rect.height * 0.55f;
            var amplitude = rect.height * 0.4f;
            DrawPlotFrame(rect, track, zero);

            var points = new Vector3[track.Barrel.Count];
            for (var index = 0; index < points.Length; index++)
                points[index] = new Vector3(XAt(rect, track, index * PreviewTrack.Frame), zero - amplitude * track.Barrel[index] / peak);

            Handles.color = BarrelPlotColor;
            Handles.DrawAAPolyLine(2f, points);

            foreach (var shot in track.Shots)
            {
                Handles.color = shot.Time <= _time ? ImpactColor : TickColor;
                Handles.DrawSolidDisc(new Vector3(XAt(rect, track, shot.Time), zero - amplitude * shot.Aim / peak), Vector3.forward, 2.5f);
            }

            DrawPlayhead(rect, track);
            var now = track.Barrel[track.FrameAt(_time)];
            GUI.Label(new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, 16f), $"Barrel off the target while nobody pulls the cursor back (dots: shots)   now {now:+0.00;-0.00;0.00}°", EditorStyles.miniLabel);
        }

        private static float Peak(IReadOnlyList<float> values, float floor)
        {
            var peak = floor;
            foreach (var value in values)
                peak = Mathf.Max(peak, Mathf.Abs(value));

            return peak;
        }

        private static void DrawPlotFrame(Rect rect, PreviewTrack track, float zero)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 1f, rect.width, rect.height - 2f), PlotColor);
            Handles.color = TickColor;
            Handles.DrawLine(new Vector3(rect.x, zero), new Vector3(rect.xMax, zero));
            foreach (var shot in track.Shots)
            {
                var x = XAt(rect, track, shot.Time);
                Handles.DrawLine(new Vector3(x, rect.y + 2f), new Vector3(x, rect.yMax - 2f));
            }
        }

        private void DrawPlayhead(Rect rect, PreviewTrack track)
        {
            var playhead = XAt(rect, track, _time);
            Handles.color = PlayheadColor;
            Handles.DrawLine(new Vector3(playhead, rect.y + 1f), new Vector3(playhead, rect.yMax - 1f));
        }

        private static float XAt(Rect rect, PreviewTrack track, float time) =>
            rect.x + rect.width * Mathf.Clamp01(time / Mathf.Max(track.Duration, PreviewTrack.Frame));

        private void Scrub(Rect rect, PreviewTrack track)
        {
            var current = Event.current;
            if ((current.type != EventType.MouseDown && current.type != EventType.MouseDrag) || !rect.Contains(current.mousePosition))
                return;

            Playing = false;
            _time = Mathf.Clamp01((current.mousePosition.x - rect.x) / rect.width) * track.Duration;
            current.Use();
            _repaint();
        }
    }
}
