// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/FadeEffect"
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
			#define REVOLUTIONS 3.0
//			#define WIDTH 0.175
//			#define THRESHOLD 0.0015

			#define WIDTH 0.00175
			#define THRESHOLD 0.35

			float spiral(float2 uv, float w, float t, float a)
			{
//				vec2 uv = (fragCoord - iResolution.xy * 0.5) / iResolution.yy;
				uv -= 0.5;

				float c = cos(a * PI * 5.0);
				float s = sin(a * PI * 5.0);

				uv = float2(uv.x * c + uv.y * s, uv.x * s - uv.y * c);

				//    uv = fract(uv * 4.0);
				float angle = atan2(uv.y, uv.x);

				//    float an = (angle / PI);
				float an = ((angle / PI) + 1.0) * 0.5;

				float dc = length(uv);

				float rv = REVOLUTIONS;
//				float rv = REVOLUTIONS;

				float invrr = (RADIUS / rv);
				float ainvrr = abs(invrr);
				//    float rvn = floor((dc + invrr * 0.5) / invrr) / REVOLUTIONS;
				float rvn = floor(dc / invrr) / rv;

				float rv1 = clamp((rvn * RADIUS) + an * invrr, -ainvrr, RADIUS + ainvrr);
				float rv2 = clamp(rv1 + invrr, -ainvrr, RADIUS + ainvrr);
				float rv3 = clamp(rv1 - invrr, -ainvrr, RADIUS + ainvrr);

				s = smoothstep(-0.001, t, abs(dc - rv1) - w);
				s *= smoothstep(-0.001, t, abs(dc - rv2) - w);
				s *= smoothstep(-0.001, t, abs(dc - rv3) - w);

				return s;
			}

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

			float _RotSpeed;


			fixed4 frag (v2f i) : SV_Target
			{
//				i.color.a = 1.0f;
				float w = WIDTH * (1.0 - i.color.a);
				float t = THRESHOLD * (1.0 - i.color.a);
				float sp = spiral(i.uv, w, t, i.color.a);
				fixed4 col = fixed4(0.0, 0.0, 0.0, sp);

				float2 aspect = float2(1.0, _ScreenParams.y / _ScreenParams.x);
				float2 uv = i.uv * aspect;
				//				i.color.a = 1.0f;
				float s = sin(_Time.y * _RotSpeed) * 8.0;
				float c = cos(_Time.y * _RotSpeed) * 8.0;
				uv -= aspect * 0.5;
				uv = float2(uv.x * c + uv.y * s, uv.x * s - uv.y * c);
				uv += 0.5;
				fixed4 tx = tex2D(_MainTex, uv);
				col.rgb += fixed3(tx.a, tx.a, tx.a);

				return col;
			}
			ENDCG
		}
	}
}
