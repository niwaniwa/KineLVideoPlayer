// ========== ========== ==========
//   KinelVideoPlayer AlphaHold 
//      Author : ni_rilana
// ========== ========== ==========
Shader "Hidden/KineL/AVPro/AlphaHold"
{
    Properties
    {
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            Name "AlphaHold"
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _AlphaThreshold;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                color.rgb = GammaToLinearSpace(color.rgb);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
    Fallback Off
}