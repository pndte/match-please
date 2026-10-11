Shader "Bw/UI/HealthBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Size ("Size", Vector) = (84, 18, 0, 0)
        _Fill ("Fill", Range(0, 1)) = 1
        _Trail ("Trail", Range(0, 1)) = 1
        _HealFrom ("Heal From", Range(0, 1)) = 1
        _Heal ("Heal", Range(0, 1)) = 0
        _Flash ("Flash", Range(0, 1)) = 0
        _Low ("Low", Range(0, 1)) = 0
        _Notches ("Notches", Float) = 4
        _Seed ("Seed", Float) = 0
        _FillColor ("Fill", Color) = (0.47, 0.84, 0.33, 1)
        _FillShade ("Fill Shade", Color) = (0.18, 0.55, 0.2, 1)
        _TrailColor ("Trail", Color) = (1, 0.93, 0.72, 1)
        _BackColor ("Empty", Color) = (0.27, 0.24, 0.26, 1)
        _HealColor ("Heal", Color) = (0.86, 1, 0.62, 1)
        _InkColor ("Graphite", Color) = (0.165, 0.165, 0.192, 1)
        _PaperColor ("Paper", Color) = (1, 0.99, 0.97, 1)
        _Ink ("Outline Width", Float) = 1.7
        _Halo ("Halo Width", Float) = 2.3
        _Wobble ("Wobble", Float) = 0.55
        _HatchGap ("Hatch Gap", Float) = 3.4
        _HatchSpeed ("Hatch Speed", Float) = 0.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            float4 _ClipRect;
            float4 _Size;
            float _Fill;
            float _Trail;
            float _HealFrom;
            float _Heal;
            float _Flash;
            float _Low;
            float _Notches;
            float _Seed;
            fixed4 _FillColor;
            fixed4 _FillShade;
            fixed4 _TrailColor;
            fixed4 _BackColor;
            fixed4 _HealColor;
            fixed4 _InkColor;
            fixed4 _PaperColor;
            float _Ink;
            float _Halo;
            float _Wobble;
            float _HatchGap;
            float _HatchSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float a = hash(float2(i, _Seed));
                float b = hash(float2(i + 1.0, _Seed));
                return lerp(a, b, f * f * (3.0 - 2.0 * f));
            }

            float capsule(float2 p, float halfLength, float radius)
            {
                return length(float2(max(abs(p.x) - halfLength, 0.0), p.y)) - radius;
            }

            float band(float distance, float from, float to)
            {
                return smoothstep(from - 0.6, from + 0.6, distance) * (1.0 - smoothstep(to - 0.6, to + 0.6, distance));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 size = _Size.xy;
                float2 p = (i.uv - 0.5) * size;
                float margin = _Ink + _Halo;
                float radius = size.y * 0.5 - margin;
                float halfLength = size.x * 0.5 - margin;
                float wobble = (noise(p.x * 0.11 + p.y * 0.05) - 0.5) * 2.0 * _Wobble + (noise(p.y * 0.4 + 17.0) - 0.5) * _Wobble;
                float d = capsule(p, halfLength - radius, radius) + wobble;

                float inside = 1.0 - smoothstep(-_Ink * 0.5 - 0.6, -_Ink * 0.5 + 0.6, d);
                float halo = 1.0 - smoothstep(_Ink * 0.5 + _Halo - 0.6, _Ink * 0.5 + _Halo + 0.6, d);
                float ink = band(d, -_Ink * 0.5, _Ink * 0.5);

                float along = (p.x + halfLength) / (2.0 * halfLength);
                float up = saturate((p.y + radius) / (2.0 * radius));
                float grain = hash(floor(p * 1.7) + _Seed);
                float stroke = frac((p.x + p.y * 1.15) / _HatchGap - _Time.y * _HatchSpeed);
                float hatch = smoothstep(0.0, 0.18, stroke) * (1.0 - smoothstep(0.38, 0.6, stroke));

                float3 fill = lerp(_FillShade.rgb, _FillColor.rgb, smoothstep(0.05, 0.85, up));
                fill = lerp(fill, float3(1.0, 1.0, 1.0), smoothstep(0.62, 0.76, up) * (1.0 - smoothstep(0.8, 0.94, up)) * 0.38);
                fill *= 1.0 - 0.13 * hatch * (0.6 + 0.4 * grain);
                fill *= 1.0 + _Low * 0.22 * (0.5 + 0.5 * sin(_Time.y * 9.0));
                float gleam = exp(-pow((along - (frac(_Time.y * 1.3) * 1.6 - 0.3) - (up - 0.5) * 0.12) * 8.0, 2.0));
                fill = lerp(fill, _HealColor.rgb, _Heal * (0.12 + 0.5 * gleam + 0.38 * step(_HealFrom, along)));

                float3 empty = _BackColor.rgb * (0.88 + 0.12 * grain) * (1.0 - 0.1 * hatch);
                float fillEdge = _Fill * 2.0 * halfLength - halfLength;
                float trailEdge = _Trail * 2.0 * halfLength - halfLength;
                float filled = (1.0 - smoothstep(fillEdge - 0.6, fillEdge + 0.6, p.x)) * step(0.0001, _Fill);
                float trailed = (1.0 - smoothstep(trailEdge - 0.6, trailEdge + 0.6, p.x)) * step(0.0001, _Trail);
                float3 color = lerp(empty, _TrailColor.rgb * (0.92 + 0.08 * grain), saturate(trailed - filled));
                color = lerp(color, fill, filled);
                float lip = exp(-pow((p.x - fillEdge) / 1.1, 2.0)) * filled * step(_Fill, 0.999);
                color = lerp(color, float3(1.0, 1.0, 1.0), lip * 0.55);

                float cell = 2.0 * halfLength / max(_Notches, 1.0);
                float notchAt = along * _Notches;
                float notchDistance = abs(frac(notchAt + 0.5) - 0.5) * cell;
                float notch = (1.0 - smoothstep(0.3, 0.9, notchDistance)) * step(0.5, notchAt) * step(notchAt, _Notches - 0.5);
                notch *= smoothstep(0.12, 0.3, up) * (1.0 - smoothstep(0.72, 0.9, up));
                color = lerp(color, _InkColor.rgb, notch * 0.4);
                color = lerp(color, float3(1.0, 1.0, 1.0), _Flash * 0.85);

                float3 result = _PaperColor.rgb;
                float alpha = halo * _PaperColor.a;
                result = lerp(result, color, inside);
                alpha = max(alpha, inside);
                float graphite = ink * (0.72 + 0.28 * hash(floor(p * 2.3) + _Seed * 7.0));
                result = lerp(result, _InkColor.rgb, graphite);
                alpha = max(alpha, graphite);

                fixed4 output = fixed4(result, alpha) * i.color;

                #ifdef UNITY_UI_CLIP_RECT
                output.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(output.a - 0.001);
                #endif

                return output;
            }
            ENDCG
        }
    }
}
