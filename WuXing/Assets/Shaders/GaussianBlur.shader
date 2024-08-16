Shader "Custom/FlexibleGaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
        _Steps ("Blur Steps", Int) = 2 // Steps in each direction (so total 2*steps + 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "FlexibleBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _BlurSize;
            int _Steps; // Number of steps in each direction
            float2 _MainTex_TexelSize;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color = float4(0, 0, 0, 0);
                float2 uv = i.uv;

                uv.y = 1- uv.y;

                // Calculate linear weights
                float totalWeight = 0.0;
                int steps = _Steps;
                float weightSum = 0.0;

                for (int x = -steps; x <= steps; x++)
                {
                    float weight = steps + 1.0 - abs(x);
                    weightSum += weight;

                    float2 offset = float2(x, 0) * _MainTex_TexelSize * _BlurSize;
                    color += tex2D(_MainTex, uv + offset) * weight;
                }

                // Normalize the color by dividing by the sum of weights
                color /= weightSum;

                return color;
            }
            ENDCG
        }

        Pass
        {
            Name "VerticalBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _BlurSize;
            int _Steps; // Number of steps in each direction
            float2 _MainTex_TexelSize;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color = float4(0, 0, 0, 0);
                float2 uv = i.uv;

                // Calculate linear weights
                float totalWeight = 0.0;
                int steps = _Steps;
                float weightSum = 0.0;

                for (int y = -steps; y <= steps; y++)
                {
                    float weight = steps + 1.0 - abs(y);
                    weightSum += weight;

                    float2 offset = float2(0, y) * _MainTex_TexelSize * _BlurSize;
                    color += tex2D(_MainTex, uv + offset) * weight;
                }

                // Normalize the color by dividing by the sum of weights
                color /= weightSum;

                return color;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
