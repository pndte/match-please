Shader "Bw/Fog-Of-War"
{
    Properties
    {
        [HideInInspector] _SightMask("Sight Mask", 2D) = "black" {}
        [HideInInspector] _SightArea("Sight Area", Vector) = (0, 0, 1, 1)
        _FogColor("Fog Color", Color) = (0.9, 0.88, 0.84, 0.9)
        _Softness("Softness", Float) = 0.35
        _Grain("Grain", Range(0, 1)) = 0.1
        _HatchColor("Hatch Color", Color) = (0.3, 0.3, 0.35, 0.32)
        _HatchAcross("Hatch Across", Vector) = (-0.7071, 0.7071, 0, 0)
        _HatchSpacing("Hatch Spacing", Float) = 0.13
        _HatchWidth("Hatch Width", Range(0, 1)) = 0.32
        _HatchLength("Hatch Length", Float) = 1.3
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SightMask);
            SAMPLER(sampler_SightMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _SightArea;
                float4 _FogColor;
                float _Softness;
                float _Grain;
                float4 _HatchColor;
                float4 _HatchAcross;
                float _HatchSpacing;
                float _HatchWidth;
                float _HatchLength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 positionWS : TEXCOORD0;
            };

            float FogHash(float2 p)
            {
                float3 q = frac(p.xyx * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }

            float FogNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float bottom = lerp(FogHash(cell), FogHash(cell + float2(1.0, 0.0)), u.x);
                float top = lerp(FogHash(cell + float2(0.0, 1.0)), FogHash(cell + float2(1.0, 1.0)), u.x);
                return lerp(bottom, top, u.y);
            }

            float Cleared(float2 uv)
            {
                return SAMPLE_TEXTURE2D_LOD(_SightMask, sampler_SightMask, uv, 0).g;
            }

            float SoftCleared(float2 positionWS)
            {
                float2 uv = (positionWS - _SightArea.xy) * _SightArea.zw;
                float2 radius = _Softness * _SightArea.zw;
                float sum = Cleared(uv) * 2.0;
                [unroll] for (int tap = 0; tap < 6; tap++)
                {
                    float inner = tap * 1.0471976;
                    float outer = inner + 0.5235988;
                    sum += Cleared(uv + float2(cos(inner), sin(inner)) * radius * 0.5) * 1.5;
                    sum += Cleared(uv + float2(cos(outer), sin(outer)) * radius);
                }
                return sum / 17.0;
            }

            float Hatch(float2 p)
            {
                float2 along = float2(_HatchAcross.y, -_HatchAcross.x);
                float across = dot(p, _HatchAcross.xy) / _HatchSpacing;
                float row = floor(across);
                float run = dot(p, along) / _HatchLength + FogHash(float2(row, 3.7)) * 5.0;
                float dash = floor(run);
                float seed = FogHash(float2(row, dash));
                float drawn = step(0.22, seed);
                float travel = frac(run);
                float end = 0.5 + 0.45 * FogHash(float2(dash + 0.5, row));
                float taper = smoothstep(0.0, 0.06, travel) * (1.0 - smoothstep(end - 0.25, end, travel));
                float wave = (FogNoise(float2(run * 2.0, row * 0.7)) - 0.5) * 0.12;
                float shift = (FogHash(float2(dash, row + 0.5)) - 0.5) * 0.3 + wave;
                float gap = abs(frac(across + shift) - 0.5) * 2.0;
                float width = _HatchWidth * (0.7 + 0.6 * seed);
                float blur = max(fwidth(across) * 2.0, 0.0001);
                float stroke = 1.0 - smoothstep(width - blur, width + blur, gap);
                float tooth = smoothstep(0.15, 0.75, FogNoise(p * 35.0));
                return stroke * drawn * taper * (0.45 + 0.55 * tooth);
            }

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS.xy;
                return output;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                float2 p = input.positionWS;
                float fog = 1.0 - SoftCleared(p);
                float grain = FogNoise(p * 23.0) * 0.6 + FogNoise(p * 57.0) * 0.4;
                float3 paper = _FogColor.rgb * (1.0 + (grain - 0.5) * _Grain);
                float3 color = lerp(paper, _HatchColor.rgb, Hatch(p) * _HatchColor.a);
                return float4(color, fog * _FogColor.a);
            }
            ENDHLSL
        }
    }
}
