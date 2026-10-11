Shader "Bw/Sprite-Lit-Highlight"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        [Header(Body)]
        _BodyColor("Body Color", Color) = (0.42, 1, 0.5, 1)
        _BodyTint("Body Tint", Range(0, 1)) = 0.35
        _BodyPulse("Body Pulse", Range(0, 1)) = 0.3
        _PulseSpeed("Pulse Speed (Hz)", Float) = 1.4

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0.15, 0.95, 0.35, 1)
        _OutlineShine("Outline Shine", Color) = (0.85, 1, 0.7, 1)
        _OutlineWidth("Outline Width (units)", Float) = 0.07
        _ShineSpeed("Shine Speed", Float) = 1.1
        _ShineScale("Shine Scale", Float) = 1.3

        [PerRendererData] _Highlight("Highlight", Range(0, 1)) = 0
        [PerRendererData] _SpriteUvRect("Sprite UV Rect", Vector) = (0, 0, 1, 1)
        [PerRendererData] _UvPerUnit("UV Per Unit", Vector) = (0.01, 0.01, 0, 0)
        [PerRendererData] _SpriteCenter("Sprite Center", Vector) = (0, 0, 0, 0)

        [MaterialToggle] _ZWrite("ZWrite", Float) = 0
        [HideInInspector] _Color("Tint", Color) = (1, 1, 1, 1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

    CBUFFER_START(UnityPerMaterial)
        half4 _Color;
        half4 _BodyColor;
        half _BodyTint;
        half _BodyPulse;
        float _PulseSpeed;
        half4 _OutlineColor;
        half4 _OutlineShine;
        float _OutlineWidth;
        float _ShineSpeed;
        float _ShineScale;
        half _Highlight;
        float4 _SpriteUvRect;
        float4 _UvPerUnit;
        float4 _SpriteCenter;
    CBUFFER_END

    half Pulse()
    {
        return 0.5h + 0.5h * sin(_Time.y * _PulseSpeed * 6.2831853);
    }

    half4 Highlighted(half4 color)
    {
        half luma = dot(color.rgb, half3(0.299h, 0.587h, 0.114h));
        half amount = saturate(_Highlight * (_BodyTint + _BodyPulse * Pulse()));
        half3 glow = _BodyColor.rgb * (0.55h + 0.8h * luma);
        return half4(lerp(color.rgb, glow, amount), color.a);
    }
    ENDHLSL

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            Stencil
            {
                Ref 128
                WriteMask 128
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                half4 color = CommonLitFragment(input, input.color);
                clip(color.a - 0.004h);
                return Highlighted(color);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Stencil
            {
                Ref 128
                ReadMask 128
                Comp NotEqual
            }

            ZWrite Off

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment

            #pragma multi_compile_instancing

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            static const int Directions = 16;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 reach : TEXCOORD1;
                float2 positionWS : TEXCOORD2;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float SpriteAlpha(float2 uv)
            {
                float2 inside = step(_SpriteUvRect.xy, uv) * step(uv, _SpriteUvRect.zw);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a * inside.x * inside.y;
            }

            Varyings OutlineVertex(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                SetUpSpriteInstanceProperties();

                float4x4 objectToWorld = GetObjectToWorldMatrix();
                float2 scale = float2(length(objectToWorld._m00_m10_m20), length(objectToWorld._m01_m11_m21));
                float2 grow = _OutlineWidth / max(scale, 0.0001);
                float2 away = sign(input.positionOS.xy - _SpriteCenter.xy) * grow * 1.5;

                float3 positionOS = float3(input.positionOS.xy + away, input.positionOS.z);
                positionOS = UnityFlipSprite(positionOS, unity_SpriteProps.xy);

                o.positionCS = TransformObjectToHClip(positionOS);
                o.positionWS = TransformObjectToWorld(positionOS).xy;
                o.uv = input.uv + away * _UvPerUnit.xy;
                o.reach = grow * _UvPerUnit.xy;
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                float own = SpriteAlpha(input.uv);
                float near = 0;
                float far = 0;

                [unroll]
                for (int index = 0; index < Directions; index++)
                {
                    float angle = index * (6.2831853 / Directions);
                    float2 direction = float2(cos(angle), sin(angle));
                    near = max(near, SpriteAlpha(input.uv + direction * input.reach * 0.5));
                    far = max(far, SpriteAlpha(input.uv + direction * input.reach));
                }

                float edge = saturate(max(near, far * 0.8) - own);
                float band = frac((input.positionWS.x * 0.6 + input.positionWS.y) * _ShineScale - _Time.y * _ShineSpeed);
                half shine = smoothstep(0.0, 0.15, band) * (1.0 - smoothstep(0.15, 0.4, band));
                half3 rgb = lerp(_OutlineColor.rgb, _OutlineShine.rgb, shine);
                half alpha = edge * _Highlight * (0.75h + 0.25h * Pulse()) * _OutlineColor.a * input.color.a;
                return half4(rgb, alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering" }

            HLSLPROGRAM
            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonNormalsVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                return CommonNormalsFragment(input, input.color);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Ref 128
                WriteMask 128
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                half4 color = CommonUnlitFragment(input, input.color);
                clip(color.a - 0.004h);
                return Highlighted(color);
            }
            ENDHLSL
        }
    }
}
