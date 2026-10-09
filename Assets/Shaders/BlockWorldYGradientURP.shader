Shader "KnockBallsCase/BlockWorldYGradientURP"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _BaseMap("Base Map", 2D) = "white" {}
        _ThemeBlockLowColor("Low Color", Color) = (0.75, 0.45, 0.3, 1)
        _ThemeBlockHighColor("High Color", Color) = (1, 0.8, 0.6, 1)
        _ThemeBlockMinY("Min Y", Float) = 0
        _ThemeBlockInvRange("Inv Range", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ThemeBlockLowColor;
                float4 _ThemeBlockHighColor;
                float _ThemeBlockMinY;
                float _ThemeBlockInvRange;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vp.positionCS;
                OUT.positionWS = vp.positionWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 albedo = mainTex * baseMap;

                float t = saturate((IN.positionWS.y - _ThemeBlockMinY) * _ThemeBlockInvRange);
                half4 gradientColor = lerp(_ThemeBlockLowColor, _ThemeBlockHighColor, t);

                return half4(albedo.rgb * gradientColor.rgb, albedo.a * gradientColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
