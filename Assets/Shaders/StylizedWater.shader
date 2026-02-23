Shader "Custom/URP/StylizedWater"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_Position;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            TEXTURE2D(_SurfaceNoise);
            SAMPLER(sampler_SurfaceNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _SurfaceNoise_ST;
                float4 _DepthGradientShallow;
                float4 _DepthGradientDeep;
                float _DepthMaxDistance;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.uv = TRANSFORM_TEX(IN.uv,_SurfaceNoise);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPosition = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float rawDepth = SampleSceneDepth(IN.screenPosition.xy / IN.screenPosition.w);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthDifference = sceneEyeDepth - IN.screenPosition.w;

                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

                return waterColor + SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, IN.uv).r;
            }
            ENDHLSL
        }
    }
}
