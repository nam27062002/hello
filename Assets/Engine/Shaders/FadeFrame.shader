Shader "Hungry Dragon/FadeFrame"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
//		_Color ("Color", Color) = (1,1,1,1)
//		_Intensity ("Intensity", float) = 1
	}
	SubShader
	{

		Tags{ "ForceSupported" = "True" "RenderType" = "Overlay" }

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
			#define WIDTH 0.125
			#define THRESHOLD 0.0025

			float spiral(float2 uv, float w, float t)
			{
//				vec2 uv = (fragCoord - iResolution.xy * 0.5) / iResolution.yy;
				uv -= 0.5;
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

				float rv1 = clamp((rvn * RADIUS) + an * invrr, 0.0, RADIUS + ainvrr);
				float rv2 = clamp((rvn * RADIUS) + (an * invrr) + invrr, -ainvrr, RADIUS + ainvrr);
				float rv3 = clamp((rvn * RADIUS) + (an * invrr) - invrr, -ainvrr, RADIUS + ainvrr);

				float s = smoothstep(0.0, t, abs(dc - rv1) - w);
				s *= smoothstep(0.0, t, abs(dc - rv2) - w);
				s *= smoothstep(0.0, t, abs(dc - rv3) - w);

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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _Tint;

			fixed4 frag (v2f i) : SV_Target
			{
				float w = WIDTH * (1.0 - i.color.a);
				float t = THRESHOLD * (1.0 - i.color.a);
				float s = spiral(i.uv, w, t);
				fixed4 col = fixed4(0.0, 0.0, 0.0, s);
				return col;
			}
			ENDCG
		}
	}
}
