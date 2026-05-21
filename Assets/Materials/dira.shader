Shader "Custom/TwoBoxHoles"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        _Hole1 ("Hole 1 Center", Vector) = (0,0,0,0)
        _Size1 ("Hole 1 Size", Vector) = (0.3,0.3,0.3,0)

        _Hole2 ("Hole 2 Center", Vector) = (0,0,0,0)
        _Size2 ("Hole 2 Size", Vector) = (0.3,0.3,0.3,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0;
            };

            fixed4 _Color;

            float4 _Hole1;
            float4 _Size1;

            float4 _Hole2;
            float4 _Size2;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // KLÍČ: local space (relativní k cube)
                o.localPos = v.vertex.xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 p = i.localPos;

                // --- Hole 1 (box mask) ---
                float3 d1 = abs(p - _Hole1.xyz);
                bool hole1 =
                    (d1.x < _Size1.x &&
                     d1.y < _Size1.y &&
                     d1.z < _Size1.z);

                // --- Hole 2 (box mask) ---
                float3 d2 = abs(p - _Hole2.xyz);
                bool hole2 =
                    (d2.x < _Size2.x &&
                     d2.y < _Size2.y &&
                     d2.z < _Size2.z);

                // --- cutout ---
                if (hole1 || hole2)
                {
                    clip(-1);
                }

                return _Color;
            }
            ENDCG
        }
    }
}