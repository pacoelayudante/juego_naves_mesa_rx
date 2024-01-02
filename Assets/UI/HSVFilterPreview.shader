Shader "Unlit/HSVFilterPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _HSVMin ("HSV Min", Color) = (0,0,0,0)
        _HSVMax ("HSV Max", Color) = (1,1,1,1)

        _Stencil("Stencil ID", Float) = 0
        _StencilComp("StencilComp", Float) = 8
        _StencilOp("StencilOp", Float) = 0
        _StencilReadMask("StencilReadMask", Float) = 255
        _StencilWriteMask("StencilWriteMask", Float) = 255
        _ColorMask("ColorMask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        ZTest [unity_GUIZTestMode]
        
        Stencil{
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

            fixed4 _HSVMin;
            fixed4 _HSVMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed3 hsv2rgb(fixed3 c)
            {
                fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            fixed3 rgb2hsv(fixed3 c)
            {
                fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
                fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 hsv = tex2D(_MainTex, i.uv).bgra;
                hsv.r = GammaToLinearSpace(hsv.r);
                hsv.g = GammaToLinearSpace(hsv.g);
                hsv.b = GammaToLinearSpace(hsv.b);

                fixed3 enRango = step(_HSVMin.rgb, hsv.rgb);
                hsv.g = enRango.x * enRango.y * enRango.z;
                enRango = step(hsv.rgb, _HSVMax.rgb);
                hsv.g *= enRango.x * enRango.y * enRango.z;

                hsv.r = LinearToGammaSpace(hsv.r);
                hsv.g = LinearToGammaSpace(hsv.g);
                hsv.b = LinearToGammaSpace(hsv.b);
                hsv.r = hsv.r*(255.0/179.0);

                fixed4 col = fixed4( hsv2rgb(hsv.rgb), 1);
                
                return col;
            }
            ENDCG
        }
    }
}
