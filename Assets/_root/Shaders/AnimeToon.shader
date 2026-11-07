
Shader "Custom/AnimeToon"
{
    Properties
    {
        [Header(Base)]
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (0.8,0.8,0.75,1)
        _Saturation("Saturation", Range(0,2)) = 0.85
        _Contrast("Contrast", Range(0,2)) = 1.0

        [Header(Toon Lighting)]
        _ShadowColor("Shadow Color", Color) = (0.30,0.34,0.43,1)
        _MidColor("Midtone Color", Color) = (0.72,0.73,0.68,1)
        _LightColor("Light Color", Color) = (1,0.94,0.82,1)
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.3
        _LightThreshold("Light Threshold", Range(0,1)) = 0.7
        _ShadowSoftness("Shadow Softness", Range(0.001,0.3)) = 0.07

        [Header(Ambient)]
        _AmbientColor("Ambient Color", Color) = (0.55,0.58,0.68,1)
        _AmbientIntensity("Ambient Intensity", Range(0,2)) = 0.7

        [Header(Height Tint)]
        _HeightColor("Height Color", Color) = (0.65,0.78,0.65,1)
        _HeightStart("Height Start", Float) = 0
        _HeightEnd("Height End", Float) = 10
        _HeightBlend("Height Blend", Range(0,1)) = 0

        [Header(Retro Dithering)]
        _DitherIntensity("Dither Intensity", Range(0,1)) = 0
        _DitherScale("Dither Scale", Range(1,8)) = 3

        [Header(Atmosphere)]
        _FogColor("Fog Color", Color) = (0.67,0.72,0.78,1)
        _FogStart("Fog Start", Float) = 20
        _FogEnd("Fog End", Float) = 100
        _FogIntensity("Fog Intensity", Range(0,1)) = 0.5

        [Header(Outline)]
        [Toggle(_OUTLINE_ON)] _OutlineEnabled("Enable Outline", Float) = 0
        _OutlineColor("Outline Color", Color) = (0.12,0.13,0.17,1)
        _OutlineWidth("Outline Width", Range(0,0.04)) = 0.003
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        // =========================================
        // OUTLINE
        // =========================================

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #pragma shader_feature_local _OUTLINE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings OutlineVert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS =
                    TransformObjectToWorld(IN.positionOS.xyz);

                float3 normalWS =
                    TransformObjectToWorldNormal(IN.normalOS);

                float4 clipPos = TransformWorldToHClip(positionWS);

                float3 normalVS = TransformWorldToViewDir(normalWS);

                float2 offset = normalize(normalVS.xy + 0.00001);

                clipPos.xy += offset * _OutlineWidth * clipPos.w;

                OUT.positionCS = clipPos;
                return OUT;
            }

            half4 OutlineFrag(Varyings IN) : SV_Target
            {
                #if defined(_OUTLINE_ON)
                    return _OutlineColor;
                #else
                    discard;
                    return 0;
                #endif
            }
            ENDHLSL
        }

        // =========================================
        // MAIN RETRO ANIME PASS
        // =========================================

        Pass
        {
            Name "RetroAnimeForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma shader_feature_local _OUTLINE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseMap_ST;
                float4 _BaseColor;

                float _Saturation;
                float _Contrast;

                float4 _ShadowColor;
                float4 _MidColor;
                float4 _LightColor;

                float _ShadowThreshold;
                float _LightThreshold;
                float _ShadowSoftness;

                float4 _AmbientColor;
                float _AmbientIntensity;

                float4 _HeightColor;
                float _HeightStart;
                float _HeightEnd;
                float _HeightBlend;

                float _DitherIntensity;
                float _DitherScale;

                float4 _FogColor;
                float _FogStart;
                float _FogEnd;
                float _FogIntensity;

                float4 _OutlineColor;
                float _OutlineWidth;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float4 color : TEXCOORD4;
                float fogDepth : TEXCOORD5;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs norm =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS = norm.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(pos);
                OUT.color = IN.color;
                OUT.fogDepth = -TransformWorldToView(pos.positionWS).z;

                return OUT;
            }

            float GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            float3 AdjustSaturation(float3 color, float saturation)
            {
                float gray = GetLuminance(color);
                return lerp(gray.xxx, color, saturation);
            }

            float DitherNoise(float2 position)
            {
                float2 p = floor(position * _DitherScale);

                return frac(
                    52.9829189 *
                    frac(dot(p, float2(0.06711056, 0.00583715)))
                );
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);

                // Base texture and vertex color
                float4 tex = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    IN.uv
                );

                float3 baseColor =
                    tex.rgb * _BaseColor.rgb * IN.color.rgb;

                baseColor = AdjustSaturation(
                    baseColor,
                    _Saturation
                );

                baseColor = (baseColor - 0.5) * _Contrast + 0.5;

                // Main directional light
                Light mainLight = GetMainLight(IN.shadowCoord);

                float3 L = normalize(mainLight.direction);

                float NdotL = saturate(dot(N, L));

                float shadowAtten =
                    mainLight.shadowAttenuation;

                float lighting = NdotL * shadowAtten;

                // Three-step retro anime lighting
                float shadowBand = smoothstep(
                    _ShadowThreshold - _ShadowSoftness,
                    _ShadowThreshold + _ShadowSoftness,
                    lighting
                );

                float lightBand = smoothstep(
                    _LightThreshold - _ShadowSoftness,
                    _LightThreshold + _ShadowSoftness,
                    lighting
                );

                float3 shadowMid = lerp(
                    _ShadowColor.rgb,
                    _MidColor.rgb,
                    shadowBand
                );

                float3 toonColor = lerp(
                    shadowMid,
                    _LightColor.rgb,
                    lightBand
                );

                // Main light
                float3 direct = toonColor * mainLight.color;

                // Ambient and sky contribution
                float3 ambient =
                    SampleSH(N) *
                    _AmbientIntensity *
                    _AmbientColor.rgb;

                float3 finalColor =
                    baseColor * (direct + ambient);

                // Height-based tint
                float heightFactor = saturate(
                    (IN.positionWS.y - _HeightStart) /
                    max(0.001, _HeightEnd - _HeightStart)
                );

                float3 heightTint = lerp(
                    float3(1,1,1),
                    _HeightColor.rgb,
                    heightFactor * _HeightBlend
                );

                finalColor *= heightTint;

                // Retro dithering
                float noise = DitherNoise(IN.positionCS.xy) - 0.5;

                finalColor += noise * _DitherIntensity * 0.08;

                // Distance haze
                float fogFactor = saturate(
                    (IN.fogDepth - _FogStart) /
                    max(0.001, _FogEnd - _FogStart)
                );

                finalColor = lerp(
                    finalColor,
                    _FogColor.rgb,
                    fogFactor * _FogIntensity
                );

                return half4(saturate(finalColor), tex.a * _BaseColor.a);
            }

            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Universal Render Pipeline/Unlit"
}
