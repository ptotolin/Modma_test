Shader "Custom/SpriteColorBlendMask"
{
    Properties
    {
        // SpriteRenderer feeds this automatically
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color       ("Tint", Color) = (1,1,1,1)

        _TargetColor ("Target Color", Color) = (1,0,0,1)
        _Blend       ("Blend", Range(0,1)) = 0
    }

    SubShader
    {
        // Sprite-friendly setup
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            fixed4 _TargetColor;
            float  _Blend;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;   // per-vertex tint from SpriteRenderer/UI
            };

            struct v2f {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color; // final tint
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;

                // Lerp RGB to target, keep sprite alpha so the shape stays
                fixed4 outCol;
                outCol.rgb = lerp(baseCol.rgb, _TargetColor.rgb, saturate(_Blend));
                outCol.a   = baseCol.a;
                return outCol;
            }
            ENDCG
        }
    }

    FallBack Off
}
