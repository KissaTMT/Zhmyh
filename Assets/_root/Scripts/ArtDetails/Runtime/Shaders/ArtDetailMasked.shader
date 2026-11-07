Shader "ArtDetails/MaskedUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture (optional)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

        // задаёт ArtDetailMaskBinder через MaterialPropertyBlock
        [HideInInspector] _MaskOn ("Mask on", Float) = 0
        [HideInInspector] _MaskCount ("Mask point count", Float) = 0
        [HideInInspector] _MaskInset ("Mask inset", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;
            float _MaskOn, _MaskCount, _MaskInset;
            float4x4 _PartWorldToLocal;       // мир → оси ArtMesh-детали (там же лежит силуэт)
            float4 _MaskPoly[64];             // до 128 точек силуэта: чётная = xy, нечётная = zw

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float3 wpos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                clip(c.a - _Cutoff);

                if (_MaskOn > 0.5)
                {
                    float3 lp = mul(_PartWorldToLocal, float4(i.wpos, 1)).xyz;
                    clip(maskDistance(lp.xy) - _MaskInset);   // вне силуэта или ближе отступа к границе: отбрасываем
                }
                return fixed4(c.rgb, 1);
            }
            ENDCG
        }
    }
    CustomEditor "ArtDetails.Editor.ArtDetailMaskedGUI"
}
