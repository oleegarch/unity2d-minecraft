Shader "Custom/BlockHovered"
{
    Properties
    {
        _BorderWidth   ("Border Width (px)", Float)        = 1
        _OrthoSize     ("Camera Orthographic Size", Float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _BorderWidth;
            float _OrthoSize;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            v2f vert (appdata IN)
            {
                v2f OUT;

                // 1) UV [0..1] → centered [-0.5..+0.5] → world‑units [−0.5..+0.5]
                float2 centered = IN.texcoord - 0.5;
                float3 worldPos = float3(centered.xy * 1.0, 0);

                // 2) Transform объекта
                float4 wp = mul(unity_ObjectToWorld, float4(worldPos,1));
                // 3) В клип‑пространство
                OUT.pos = mul(UNITY_MATRIX_VP, wp);

                OUT.uv = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // экранных пикселей в одном world‑unit по вертикали:
                float blockScreenPx = _ScreenParams.y / (_OrthoSize * 2);

                // толщина рамки в UV‑координатах текстуры
                float t = _BorderWidth / blockScreenPx;

                // рисуем, если tex-UV близки к краям
                bool left   = IN.uv.x < t;
                bool right  = IN.uv.x > 1 - t;
                bool bottom = IN.uv.y < t;
                bool top    = IN.uv.y > 1 - t;

                if (left || right || bottom || top)
                    return IN.color;

                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}