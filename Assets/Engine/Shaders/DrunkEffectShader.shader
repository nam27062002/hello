// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};


			sampler2D _MainTex;
//			float4 _MainTex_ST;
			fixed _Intensity;



			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
//				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			#define RADIUS 0.01

			#define SPEED  2.0

			fixed4 frag (v2f i) : SV_Target			
			{
//				i.uv += 0.5;
				float c = cos(_Time.y * SPEED);
				float s = sin(_Time.y * SPEED);

				fixed4 col;
//				col.x = tex2D(_MainTex, i.uv + float2(-c * RADIUS, s * RADIUS) * _Intensity).x;
//				col.y = tex2D(_MainTex, i.uv + float2(c * RADIUS, -s * RADIUS) * _Intensity).y;
//				col.z = tex2D(_MainTex, i.uv + float2(-c * RADIUS, -s * RADIUS) * _Intensity).z;

				col = tex2D(_MainTex, i.uv + float2(-c * RADIUS, s * RADIUS) * _Intensity);
				col += tex2D(_MainTex, i.uv + float2(c * RADIUS, -s * RADIUS) * _Intensity);
				col += tex2D(_MainTex, i.uv + float2(-c * RADIUS, -s * RADIUS) * _Intensity);
				col *= 0.33333;

				return col;
//				return col * 0.5;
			}

/*
#define RADIUS 0.008
#define SPEED  2.0

			fixed4 frag(v2f i) : SV_Target
			{
				fixed2 d = i.uv - 0.5;
				fixed l = length(d);
				d = normalize(d);
//				i.uv = abs(i.uv);
//				return fixed4(i.uv.x, i.uv.y, l, 1.0);
				float s = sin(_Time.y * SPEED);
				l *= 0.051 * s;
				fixed4 col;
				col.x = tex2D(_MainTex, i.uv + d * l).x;
				col.y = tex2D(_MainTex, i.uv + d * l * 2.0).y;
				col.z = tex2D(_MainTex, i.uv + d * l * 3.0).z;
				return col;

			}
*/

			ENDCG
		}
	}
}
