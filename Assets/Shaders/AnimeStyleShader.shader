Shader "Custom/AnimeStyleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.03)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Основной цвет текстуры
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Проверяем соседние пиксели для создания контура
                float2 uv = i.uv;
                fixed4 outline = _OutlineColor;
                outline.a = 0;

                float2 offsets[8] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1, 0), float2(1, 0),
                    float2(-1, 1), float2(0, 1), float2(1, 1)
                };

                for (int j = 0; j < 8; j++)
                {
                    float2 neighborUV = uv + offsets[j] * _OutlineThickness;
                    fixed4 neighborCol = tex2D(_MainTex, neighborUV);
                    if (neighborCol.a < 0.5)
                    {
                        outline.a = 1;
                        break;
                    }
                }

                // Смешиваем основной цвет с контуром
                col.rgb = lerp(col.rgb, outline.rgb, outline.a);

                return col;
            }
            ENDCG
        }
    }
}