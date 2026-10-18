using System;
using Bw.UseCases.Props.View.Flight;
using UnityEngine;

namespace Bw.UseCases.Props.View
{
    [Serializable]
    public sealed class PropVfxConfig
    {
        public PropFlightFx FlightFx;
        public ParticleSystem ArcDots;
        public GameObject Dust;

        [Header("Flight")]
        [Min(0f)] public float TrailSpeed = 16f;
        [Range(0f, 1f)] public float TrailHold = 0.7f;
        [Min(0.01f)] public float GhostInterval = 0.035f;

        [Header("Impact")]
        [Min(0f)] public float ImpactSpeed = 8f;
        [Min(0f)] public float ImpactCooldown = 0.15f;
        public Vector3 LandPunch = new(0.22f, -0.18f, 0f);
        [Min(0.01f)] public float LandPunchTime = 0.22f;

        [Header("Throw")]
        [Min(0f)] public float ThrowKick = 0.12f;

        [Header("Arc")]
        [Min(0.01f)] public float ArcTime = 0.2f;
        [Min(1)] public int ArcDotCount = 7;
        [Min(0.01f)] public float ArcDotSize = 0.16f;
        [Range(0f, 1f)] public float ArcDotTaper = 0.7f;
        [Range(0f, 1f)] public float ArcDotFade = 0.15f;
    }
}
