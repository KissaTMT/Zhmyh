Shader "ArtMesh/FlatDeform"
{
    Properties
    {
        _Tint ("Fill color", Color) = (1,1,1,1)
        _OutlineColor ("Outline color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline width (local units)", Float) = 0.03

        // всё ниже задаёт ArtMeshRenderBinder через MaterialPropertyBlock
        [HideInInspector] _Scale ("View scale", Vector) = (1,1,1,0)
        [HideInInspector] _Offset ("View offset (z = depth)", Vector) = (0,0,0,0)
        [HideInInspector] _UseLattice ("Use lattice", Float) = 0
        [HideInInspector] _LatMin ("Lattice min", Vector) = (0,0,0,0)
        [HideInInspector] _LatSize ("Lattice size", Vector) = (1,1,0,0)
        [HideInInspector] _LatDim ("Lattice cols, rows", Vector) = (2,2,0,0)
        [HideInInspector] _VertTex ("Vertex deltas (xyz)", 2D) = "black" {}
        [HideInInspector] _VertTexW ("Vertex deltas width", Float) = 1
        [HideInInspector] _UseVertEdit ("Use vertex deltas", Float) = 0
        [HideInInspector] _IsOutline ("Is outline material", Float) = 0
        [HideInInspector] _OutlineOn ("Outline enabled", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            fixed4 _Tint, _OutlineColor;
            float _OutlineWidth, _IsOutline, _OutlineOn;
            float4 _Scale, _Offset;
            float _UseLattice, _UseVertEdit, _VertTexW;
            float4 _LatMin, _LatSize, _LatDim;
            sampler2D _VertTex;
            float4 _Lattice[64];            // xyz = смещение контрольной точки (до 8x8)

            struct appdata
            {
                float4 vertex : POSITION;   // базовая форма
                float2 nrm    : TEXCOORD0;  // внешняя нормаль вершины (только внешнее кольцо), для обводки
                uint   vid    : SV_VertexID;
            };

            struct v2f { float4 pos : SV_POSITION; };

            // --- сетка деформации: бикубический Catmull-Rom по смещениям узлов ---
            float4 crw(float t)
            {
                float t2 = t * t, t3 = t2 * t;
                return float4(-0.5 * t3 + t2 - 0.5 * t,
                               1.5 * t3 - 2.5 * t2 + 1.0,
                              -1.5 * t3 + 2.0 * t2 + 0.5 * t,
                               0.5 * t3 - 0.5 * t2);
            }

            float3 latFetch(int i, int j, int cols, int rows)
            {
                i = clamp(i, 0, cols - 1);
                j = clamp(j, 0, rows - 1);
                return _Lattice[j * cols + i].xyz;
            }

            float3 latEval(float2 p)
            {
                int cols = (int)_LatDim.x;
                int rows = (int)_LatDim.y;
                float2 dim1 = float2(cols - 1, rows - 1);
                float2 g = (p - _LatMin.xy) / max(_LatSize.xy, float2(1e-5, 1e-5)) * dim1;
                g = clamp(g, float2(0, 0), dim1 - 1e-3);
                int2 c = int2(floor(g));
                float2 f = g - float2(c);
                float4 wx = crw(f.x);
                float4 wy = crw(f.y);
                float3 r = float3(0, 0, 0);
                [unroll] for (int j = 0; j < 4; j++)
                {
                    float3 row = float3(0, 0, 0);
                    [unroll] for (int i = 0; i < 4; i++)
                        row += wx[i] * latFetch(c.x - 1 + i, c.y - 1 + j, cols, rows);
                    r += wy[j] * row;
                }
                return r;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float2 p = v.vertex.xy;
                float3 q = float3(p, 0);

                // 1) сетка деформации (xyz, z = глубина)
                if (_UseLattice > 0.5) q += latEval(p);

                // 2) финальные правки вершин поверх сетки (индекс = SV_VertexID)
                if (_UseVertEdit > 0.5)
                    q += tex2Dlod(_VertTex, float4((v.vid + 0.5) / _VertTexW, 0.5, 0, 0)).xyz;

                // 3) трансформ вида: scale, затем offset (offset.z = глубина вида)
                q = q * _Scale.xyz + _Offset.xyz;

                // 4) обводка: внешнее кольцо выдвигается по нормали и отодвигается от камеры вдоль оси Z детали
                if (_IsOutline > 0.5)
                {
                    if (_OutlineOn < 0.5)
                    {   // обводка выключена: вырожденная геометрия за пределами клипа
                        o.pos = float4(2, 2, 2, 1);
                        return o;
                    }
                    float l = length(v.nrm);
                    float2 n = l > 1e-4 ? v.nrm / l : float2(0, 0);
                    q.xy += n * _OutlineWidth;

                    float3 camLocal = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                    q.z += (camLocal.z > q.z ? -1.0 : 1.0) * 0.002;
                }

                o.pos = UnityObjectToClipPos(float4(q, 1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _IsOutline > 0.5 ? fixed4(_OutlineColor.rgb, 1) : fixed4(_Tint.rgb, 1);
            }
            ENDCG
        }
    }
}
