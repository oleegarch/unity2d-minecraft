Shader "Custom/BlockOpaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        ZWrite On
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 uvRect : TEXCOORD1;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 uvRect : TEXCOORD1;
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvRect = v.uvRect;
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 halfTexel = 0.5 * _MainTex_TexelSize.xy;
                float2 uvMin = i.uvRect.xy + halfTexel;
                float2 uvMax = i.uvRect.zw - halfTexel;
                float2 uv    = clamp(i.uv, uvMin, uvMax);

                return tex2D(_MainTex, uv) * i.color;
            }
            ENDCG
        }
    }
}