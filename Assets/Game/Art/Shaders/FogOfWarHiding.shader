Shader "Bw/Fog-Of-War-Hiding"
{
    Properties
    {
        [HideInInspector] _SightMask("Sight Mask", 2D) = "black" {}
        [HideInInspector] _SightArea("Sight Area", Vector) = (0, 0, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-1" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest Always
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SightMask);
            SAMPLER(sampler_SightMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _SightArea;
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
                float shown = SAMPLE_TEXTURE2D(_SightMask, sampler_SightMask, (input.positionWS - _SightArea.xy) * _SightArea.zw).r;
                clip(0.5 - shown); //TODO: персонаж на краю тени срезается резкой линией посреди мягкого края тумана — срез можно рассыпать штриховкой (порог по шуму)
                return 0;
            }
            ENDHLSL
        }
    }
}
