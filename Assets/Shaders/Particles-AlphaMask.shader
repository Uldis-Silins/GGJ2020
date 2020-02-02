Shader "Particles/Alpha Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_NoiseTex1("Noise 1", 2D) = "white" {}
		_ScrollSpeed1("Scroll 1", Range(0, 1)) = 0.2
		_NoiseTex2("Noise 2", 2D) = "white" {}
		_ScrollSpeed2("Scroll 2", Range(0, 1)) = 0.3
    }
    SubShader
    {
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
			Blend One OneMinusSrcAlpha
			ColorMask RGB
			Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _NoiseTex1;
			sampler2D _NoiseTex2;
			fixed _ScrollSpeed1;
			fixed _ScrollSpeed2;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 noise1 = tex2D(_NoiseTex1, float2(i.uv.x + frac(_Time.y * _ScrollSpeed1), i.uv.y));
				fixed4 noise2 = tex2D(_NoiseTex2, float2(i.uv.x + frac(_Time.y * _ScrollSpeed2), i.uv.y));

				col.a *= noise1.r * (noise2.r * 2.0);
                return col;
            }
            ENDCG
        }
    }
}
