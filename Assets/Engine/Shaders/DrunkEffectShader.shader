Shader "Hungry Dragon/DrunkEffectShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
//		_Color ("Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
//			fixed4 _Color;
			fixed _Intensity;

			#define RADIUS 0.008

			#define SPEED  2.0

			fixed4 frag (v2f i) : SV_Target			
			{
//				i.uv += 0.5;
				float c = cos(_Time.y * SPEED * 1.5);
				float s = sin(_Time.y * SPEED);

				fixed4 col = tex2D(_MainTex, i.uv + float2(c * RADIUS, s * RADIUS) * _Intensity);
				col += tex2D(_MainTex, i.uv + float2(-c * RADIUS, -s * RADIUS) * _Intensity);
//				col += tex2D(_MainTex, i.uv + float2(c * RADIUS, -s * RADIUS) * _Intensity);
//				col += tex2D(_MainTex, i.uv + float2(-c * RADIUS, s * RADIUS) * _Intensity);

				return col * 0.5;
			}
			ENDCG
		}
	}
}
