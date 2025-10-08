Shader "Custom/URP/Pupil"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "black" {}
        _GradientColor1 ("Gradient Color 1", Color) = (0.2, 0.6, 1.0, 1)
        _GradientColor2 ("Gradient Color 2", Color) = (1.0, 0.3, 0.8, 1)
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 1)

        _Blob1Pos ("Gradient Blob 1", Vector) = (0.35, 0.35, 0, 0)
        _Blob2Pos ("Gradient Blob 2", Vector) = (0.65, 0.65, 0, 0)
        _Shine1Pos ("Shine 1", Vector) = (0.28, 0.28, 0, 0)
        _Shine2Pos ("Shine 2", Vector) = (0.70, 0.60, 0, 0)

        _BlobSize ("Blob Size", Float) = 0.25
        _ShineSize ("Shine Size", Float) = 0.1

        _WarpSpeed ("Warp Speed", Float) = 1.0
        _WarpAmount ("Blob Warp", Float) = 0.05
        _ShineWarpAmount ("Shine Warp", Float) = 0.03
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque"
        }

        Cull Off
        ZWrite On
        ZTest LEqual
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float3 _GradientColor1;
                float3 _GradientColor2;
                float3 _ShineColor;

                float2 _Blob1Pos;
                float2 _Blob2Pos;
                float2 _Shine1Pos;
                float2 _Shine2Pos;

                float _BlobSize;
                float _ShineSize;

                float _WarpSpeed;
                float _WarpAmount;
                float _ShineWarpAmount;
            CBUFFER_END

            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float warpedBlob(float2 uv, float2 center, float size, float timeOffset, float warp)
            {
                float2 delta = uv - center;

                float time = _Time.y;

                float warpX = sin(time * _WarpSpeed + timeOffset + delta.y * 10.0) * warp;
                float warpY = cos(time * _WarpSpeed + timeOffset + delta.x * 10.0) * warp;

                delta += float2(warpX, warpY);
                float dist = length(delta);
                return 1.0 - smoothstep(size, size * 1.3, dist);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 uv = IN.uv;
                float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

                float grad1 = warpedBlob(uv, _Blob1Pos, _BlobSize, 0.0, _WarpAmount);
                float grad2 = warpedBlob(uv, _Blob2Pos, _BlobSize, 3.14, _WarpAmount);
                float gradientMask = saturate(grad1 + grad2);

                float3 gradientColor = lerp(_GradientColor1, _GradientColor2, uv.y);
                color = lerp(color,gradientColor,gradientMask);

                float shine1 = warpedBlob(uv, _Shine1Pos, _ShineSize, 1.0, _ShineWarpAmount);
                float shine2 = warpedBlob(uv, _Shine2Pos, _ShineSize * 0.8, 4.2, _ShineWarpAmount);
                float shineMask = saturate(shine1 + shine2);

                color = lerp(color, _ShineColor, shineMask);

                return float4(color, 1);
            }

            ENDHLSL
        }
    }
}
