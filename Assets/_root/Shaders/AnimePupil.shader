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

        _Cutoff("Alpha Cutout", Range(0.0, 1.0)) = 0.5

        // задаёт ArtDetailMaskBinder через MaterialPropertyBlock
        [HideInInspector] _MaskOn ("Mask on", Float) = 0
        [HideInInspector] _MaskCount ("Mask point count", Float) = 0
        [HideInInspector] _MaskInset ("Mask inset", Float) = 0
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
            #pragma target 3.5
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
                float3 positionWS : TEXCOORD1;

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

                float _Cutoff;
                float _MaskOn;
                float _MaskCount;
                float _MaskInset;
            CBUFFER_END

            float4x4 _PartWorldToLocal;   // мир → оси ArtMesh-детали (там же лежит силуэт)
            float4 _MaskPoly[64];         // до 128 точек силуэта: чётная = xy, нечётная = zw

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float2 polyPoint(int i)
            {
                float4 q = _MaskPoly[i >> 1];
                return (i & 1) ? q.zw : q.xy;
            }

            // Расстояние до границы силуэта со знаком: > 0 внутри, < 0 снаружи (чётно-нечётное правило + ближайший отрезок).
            float maskDistance(float2 p)
            {
                int n = (int)_MaskCount;
                bool inside = false;
                float minD2 = 1e20;
                float2 prev = polyPoint(n - 1);
                [loop] for (int i = 0; i < n; i++)
                {
                    float2 cur = polyPoint(i);
                    if (((cur.y > p.y) != (prev.y > p.y)) &&
                        (p.x < (prev.x - cur.x) * (p.y - cur.y) / (prev.y - cur.y) + cur.x))
                        inside = !inside;

                    float2 e = prev - cur;
                    float2 w = p - cur;
                    float t = saturate(dot(w, e) / max(dot(e, e), 1e-12));
                    float2 d = w - e * t;
                    minD2 = min(minD2, dot(d, d));
                    prev = cur;
                }
                return inside ? sqrt(minD2) : -sqrt(minD2);
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
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float grad1 = warpedBlob(uv, _Blob1Pos, _BlobSize, 0.0, _WarpAmount);
                float grad2 = warpedBlob(uv, _Blob2Pos, _BlobSize, 3.14, _WarpAmount);
                float gradientMask = saturate(grad1 + grad2);

                float4 gradientColor = lerp(float4(_GradientColor1,1), float4(_GradientColor2,1), uv.y);
                color = lerp(color,gradientColor,gradientMask);

                float shine1 = warpedBlob(uv, _Shine1Pos, _ShineSize, 1.0, _ShineWarpAmount);
                float shine2 = warpedBlob(uv, _Shine2Pos, _ShineSize * 0.8, 4.2, _ShineWarpAmount);
                float shineMask = saturate(shine1 + shine2);

                color = lerp(color, float4(_ShineColor,1), shineMask);

                clip(color.a - _Cutoff);

                if (_MaskOn > 0.5)
                {
                    float3 lp = mul(_PartWorldToLocal, float4(IN.positionWS, 1)).xyz;
                    clip(maskDistance(lp.xy) - _MaskInset);
                }

                return color;
            }

            ENDHLSL
        }
    }
}
