Shader "Hungry Dragon/Particles/FlowmapTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise", 2D) = "white" {}
		_Speed ("Speed", Float) = 1.0
		_Intensity ("Intensity", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			float _Speed;
			float _Intensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float2 off = (tex2D(_NoiseTex, i.uv).xy * 2.0f - 1.0f) * _Intensity;
				float time = _Time.y * _Speed;
				float phase0 = frac(time * 0.5f + 0.5f) ;
				float phase1 = frac(time * 0.5f + 1.0f);

				fixed4 col = tex2D(_MainTex, i.uv + off * phase0);
				fixed4 col2 = tex2D(_MainTex, i.uv + off * phase1);

				float flowLerp = abs((0.5f - phase0) / 0.5f);

				return lerp(col, col2, flowLerp);
			}
			ENDCG
		}
	}
}
