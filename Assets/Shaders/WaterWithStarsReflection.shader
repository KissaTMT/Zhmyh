Shader "Custom/URP/WaterWithStarsReflection"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _ReflectionTex ("Reflection Texture", 2D) = "black" { }
        _StarTex ("Stars Texture", 2D) = "white" { }
        _NormalTex ("Normal Map", 2D) = "bump" { }
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.5
        _TilingStars ("Tiling Stars", Range(0, 1)) = 0.5
        _Reflectivity ("Reflectivity", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" } // Ensure compatibility with URP

            // Start of the shader code
            HLSLPROGRAM
            #pragma target 4.5
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shading.hlsl"

            // Texture samplers
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);
            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);

            // Material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _WaveStrength;
                float _TilingStars;
                float _Reflectivity;
            CBUFFER_END

            // Vertex structure
            struct Attributes
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            // Vertex Shader
            Varyings vert(Attributes v)
            {
                float waveOffset = sin(v.position.x * 0.1 + _Time.y) * _WaveStrength; // Simple sine wave based on vertex X position and time
                v.position.y += waveOffset;

                Varyings o;
                o.pos = TransformObjectToHClip(v.position);
                o.uv = v.uv;

                // World space position and normal
                o.worldPos = TransformObjectToWorld(v.position).xyz;
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            // Fragment Shader
            float4 frag(Varyings i) : SV_Target
            {
                // Compute reflection
                float3 reflectionDir = reflect(i.worldPos, normalize(i.worldNormal));
                float2 reflectionUV = i.uv + reflectionDir.xz * _TilingStars;

                // Fetch reflection texture
                float4 reflectionColor = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, reflectionUV);

                // Add star reflection
                //float4 starColor = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, i.uv + _Time.y * 0.05);

                // Combine reflections with transparency
                float4 finalColor = lerp(_BaseColor, reflectionColor, _Reflectivity);
                //finalColor.rgb += starColor.rgb * 0.5; // Add stars to reflection

                // Water wave effect (simple distortion)
                finalColor.rgb *= 1.0 + sin(i.worldPos.x * 0.1 + _Time.y) * 0.05;

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "UniversalForward"
}
