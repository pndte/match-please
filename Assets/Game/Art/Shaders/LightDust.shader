Shader "Bw/Light-Dust"
{
    Properties
    {
        _MainTex("Mote", 2D) = "white" {}
        _Color("Colour", Color) = (0.84, 0.89, 1, 1)
        _Brightness("Brightness", Float) = 0.9
        _AmbientScale("Ambient Scale", Float) = 1.3
        _Gain("Gain", Float) = 2.5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend One One
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ShapeLightTexture0);
            SAMPLER(sampler_ShapeLightTexture0);
            half _AmbientLightLevel;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Brightness;
                half _AmbientScale;
                half _Gain;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 lightingUV : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color;
                output.uv = input.uv;
                output.lightingUV = ComputeScreenPos(output.positionCS / output.positionCS.w).xy;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half mote = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * input.color.a;
                half3 light = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, input.lightingUV).rgb;
                half ambient = max(_AmbientLightLevel * _AmbientScale, 0.002);
                half lit = saturate((max(light.r, max(light.g, light.b)) - ambient) * _Gain);
                return half4(_Color.rgb * _Brightness * mote * lit, 0);
            }
            ENDHLSL
        }
    }
}
