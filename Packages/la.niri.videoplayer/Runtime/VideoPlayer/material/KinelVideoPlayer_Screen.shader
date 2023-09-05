// ========== ========== ==========
//   KinelVideoPlayer_Screen  ver.1.2
//      Author : にしおかすみす！
//      Twitter : @nsokSMITHdayo
// ========== ========== ==========

Shader "Video/KinelVideoPlayer/Screen"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("デフォルトイメージ: DefaultImage", 2D) = "white" {}
        // v1.1
        _BgColor ("背景色: BackgroundColor", Color) = (0,0,0,0)
            // v1.2
            // [Enum(Landscape,0,Portrait,1,Square,2)]_AspectRatio ("アスペクト比: AspectRatio", Int) = 0
            //
        //
        [Enum(Off,2,On,0)]_CullMode ("裏面描画: BackfaceRendering", Int) = 2
        [Toggle]_NoBackfaceInversion ("裏面反転無効: NoBackfaceInversion", Int) = 0
        [Toggle]_NoMirrorInversion("鏡反転無効: NoMirrorInversion", Int) = 0
        _Brightness ("輝度: Brightness", range(0, 1)) = 1
        _Transparency ("透過: Transparency", range(0, 1)) = 0
        [Enum(Disable,4,Enable,8)]_DisplayInFront ("常に手前に表示: AlwaysDisplayInFront", Int) = 1
        [Toggle]_IsAVPRO("AVPro", Int) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            // v1.2
            "PreviewType"="Plane"
            //
        }
        LOD 100
        // blend SrcAlpha OneMinusSrcAlpha
        Cull[_CullMode]
        ZTest[_DisplayInFront]

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

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float inMirror : TEXCOORD1;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Brightness;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                //

                o.vertex = UnityObjectToClipPos(v.vertex);
                // v1.2
                // o.uv = v.uv;
                float aspect = _MainTex_TexelSize.z / 1.77777778;
                o.uv.x = _MainTex_TexelSize.w < aspect
                    ? v.uv.x
                    : ((v.uv.x - 0.5) / (aspect / _MainTex_TexelSize.w)) + 0.5;
                o.uv.y = _MainTex_TexelSize.w < aspect
                    ? ((v.uv.y - 0.5) / (_MainTex_TexelSize.w / aspect)) + 0.5
                    : v.uv.y;
                //
                float3 crossFwd = cross(UNITY_MATRIX_V[0], UNITY_MATRIX_V[1]);
                o.inMirror = dot(crossFwd, UNITY_MATRIX_V[2]);
                return o;
            }

            // v1.1
            fixed4 _BgColor;
                // v1.2
                // int _AspectRatio;
                //
            //
            int _NoBackfaceInversion;
            int _NoMirrorInversion;
            int _IsAVPRO;

            fixed4 frag (v2f i, fixed facing:VFACE) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                //
                
                float2 uv = i.uv;

                // _NoMirrorInversion
                int inMirror = step(0, i.inMirror);
                inMirror = lerp(0, inMirror, _NoMirrorInversion);
                uv.x = lerp(uv.x, 1 - uv.x, inMirror);

                // _IsAVPRO
                uv.y = lerp(uv.y, 1 - uv.y, _IsAVPRO);
                
                // _NoBackfaceInversion
                int face = step(facing, 0);
                face = lerp(0, face, _NoBackfaceInversion);
                uv.x = lerp(uv.x, 1 - uv.x, face);
                
                // v1.2
                //     // v.1.1
                //     // _AspectRatio
                //     if(_AspectRatio == 1)
                //     {
                //         uv.x *= 3.16049383;
                //         uv.x -= 1.10814667;
                //     }
                //     else if(_AspectRatio == 2)
                //     {
                //         uv.x *= 1.77777778;
                //         uv.x -= 0.38888889;
                //     }
                //
                //     int v = (int)((uv.x - 0.5) * 2);
                //     float isInRange = abs((float)v / (v - 0.00001));
                //
                //     fixed4 col = lerp(tex2D(_MainTex, uv), _BgColor, isInRange);
                //     //
                //
                int isInRange = any(uv < 0 || 1 < uv);
                fixed4 col = lerp(tex2D(_MainTex, uv), _BgColor, isInRange);
                //

                // IsAVPRO
                col.rgb = lerp(col.rgb, pow(col.rgb,2.2), _IsAVPRO);

                col.rgb *= _Brightness;
                col.a = (1 - _Transparency) * col.a;
                return col;
            }
            ENDCG
        }
    }
}
