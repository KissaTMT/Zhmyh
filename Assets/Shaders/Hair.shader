Shader "Custom/Hair"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _ShadeColor ("Shade Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularPower ("Specular Power", Range(1, 64)) = 16
        _ToonThreshold ("Toon Threshold", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull Off
        ZWrite Off
        ZTest LEqual
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "Universal2D"  }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShadeColor;
            float4 _SpecColor;
            float _SpecularPower;
            float _ToonThreshold;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float ndl = dot(normal, lightDir);

                // Toon shading (step between shade and lit)
                float lightIntensity = step(_ToonThreshold, ndl);

                float4 texColor = tex2D(_MainTex, IN.uv);
                float3 diffuse = lerp(_ShadeColor.rgb, texColor.rgb, lightIntensity);

                // Specular (anime style highlight)
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normal, halfDir)), _SpecularPower);
                spec = smoothstep(0.5, 1.0, spec); // toon specular band

                float3 finalColor = diffuse + spec * _SpecColor.rgb;

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
