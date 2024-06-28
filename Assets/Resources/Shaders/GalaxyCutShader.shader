Shader "Unlit/GalaxyCutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Float) = 1
        _Brightness("Brightness", Range(1, 10)) = 1
        _RIntensity("Red Intensity", Range(0.01, 1)) = 1
        _GIntensity("Green Intensity", Range(0.01, 1)) = 1
        _BIntensity("Blue Intensity", Range(0.01, 1)) = 1
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scale;
            float _Brightness;
            float _RIntensity;
            float _GIntensity;
            float _BIntensity;

            // Function to manipulate color with brightness and intensity
            fixed4 ApplyColorManipulation(fixed4 color, float brightness, float rInt, float gInt, float bInt)
            {
                return fixed4(color.r * rInt * brightness, color.g * gInt * brightness, color.b * bInt * brightness, color.a);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv -= 0.5;
                uv *= _Scale / 1000;
                uv += 0.5;

                // Sample the texture
                fixed4 col = tex2D(_MainTex, uv);

                // Apply color manipulation
                float brightness = _Brightness;
                float rInt = _RIntensity;
                float gInt = _GIntensity;
                float bInt = _BIntensity;
                col = ApplyColorManipulation(col, brightness, rInt, gInt, bInt);

                return col;
            }
            ENDCG
        }
    }
}
