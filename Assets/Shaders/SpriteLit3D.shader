Shader "Custom/URP/SpriteLit3D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutout", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" "Queue"="AlphaTest"
        }
        Cull Off
        ZTest LEqual
        ZWrite On

        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 rotatedUV : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Cutoff;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4x4 objectToWorld = GetObjectToWorldMatrix();

                float4 worldPos = mul(objectToWorld, IN.positionOS);
                
                float2x2 rotationMatrix = float2x2(
                    objectToWorld[0].x, objectToWorld[0].y,
                    objectToWorld[1].x, objectToWorld[1].y
                );
                
                float2 right = normalize(rotationMatrix[0]);
                float2 up = normalize(rotationMatrix[1]);
                rotationMatrix = float2x2(right, up);
                
                float2 centeredUV = IN.uv - 0.5;

                OUT.positionHCS = TransformWorldToHClip(worldPos.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.rotatedUV = mul(rotationMatrix, centeredUV) + 0.5;
                
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
            
                Light mainLight = GetMainLight();
            
                float3 lightNormals = float3(0,0,-1);
                float NdotL = max(0, dot(lightNormals, mainLight.direction));
                float3 lighting = mainLight.color.rgb * NdotL;
                clip(texColor.a - _Cutoff);
                float shade = max(IN.rotatedUV.y*1.5,0.2);
            
                return float4(texColor.rgb * lighting * shade, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
