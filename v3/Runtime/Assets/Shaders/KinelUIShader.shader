// ========== ========== ==========
//   KinelVideoPlayer AlphaHold 
//      Author : ni_rilana
// ========== ========== ==========
Shader "Kinel/MSDF"
{
    Properties
    {
        _MainTex ("MSDF Texture", 2D) = "white" {}
        _FGColor ("Foreground Color", Color) = (1,1,1,1)
        _BGColor ("Background Color", Color) = (0,0,0,0)
        _UseBG ("Use Background Color (0/1)", Float) = 0
        _AlphaMultiplier ("Alpha Multiplier", Range(0,1)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8 // Equal
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0 // Keep
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            fixed4 _FGColor, _BGColor;
            float _UseBG, _AlphaMultiplier;
            float4 _ClipRect;

            struct appdata
            {
                float4 vertex:POSITION;
                float4 color:COLOR;
                float2 uv:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                fixed4 color:COLOR;
                float4 worldPos:TEXCOORD1; // Object space pos（UI ではこれでOK）
            };

            float median(float r, float g, float b) { return max(min(r, g), min(max(r, g), b)); }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                float3 msd = tex2D(_MainTex, i.uv).rgb;
                float sd = median(msd.r, msd.g, msd.b);
                float w = fwidth(sd);
                float opacity = smoothstep(0.5 - w, 0.5 + w, sd);

                fixed4 col = _FGColor * i.color;
                col.a *= opacity * _AlphaMultiplier;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}