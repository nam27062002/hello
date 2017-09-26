// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/FadeEffect2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("Color", Color) = (1,1,1,1)
		_RotSpeed("Rotation Speed", Float) = 0.25
		//		_Intensity ("Intensity", float) = 1
	}
	SubShader
	{

		Tags{ "Queue" = "Overlay" "ForceSupported" = "True" "RenderType" = "Overlay" }

		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"


			#define PI 3.14159265359
			#define RADIUS 0.5
			#define REVOLUTIONS 1.5
//			#define WIDTH 0.0125
			#define WIDTH 0.175
			#define THRESHOLD 0.0015

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _RotSpeed;

			fixed4 frag (v2f i) : SV_Target
			{
		//				float2 aspect = float2(1.0, _ScreenParams.y / _ScreenParams.x) * 5.0;
				fixed4 col = fixed4(0.0, 0.0, 0.0, i.color.a);
				for (float it = 1.0; it < 8.0; it += it) {
					float2 aspect = float2(1.0, _ScreenParams.y / _ScreenParams.x) * it;
					float2 uv = i.uv * aspect;
	//				i.color.a = 1.0f;
					float s = sin(_Time.y * _RotSpeed * -it);
					float c = cos(_Time.y * _RotSpeed * it);
					uv -= aspect * 0.5;
					uv = float2(uv.x * c + uv.y * s, uv.x * s - uv.y * c);
					uv += 0.5;
					fixed4 tx = tex2D(_MainTex, uv);
					col.rgb += fixed3(tx.a * s, tx.a * c, tx.a * c * s ) * s;
				}
				return col;
			}
			ENDCG
		}
	}
}
