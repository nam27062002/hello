Shader "Hungry Dragon/SnowEffect"
{
	Properties
	{
		_WorldPosition ("Position", Vector) = (0.0, 0.0, 0.0, 0.0)
		_Aspect("Aspect", float) = 1.0
		_Scale("Scale", Range(1.0, 30.0)) = 15.0
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
//			#pragma glsl_no_auto_normalization
//			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON
			
			#include "UnityCG.cginc"

			#define PI 3.1415926
			#define SNOWSPEED 30.0
			#define SNOWRADIUS 0.01
//			#define MANUALSCALE 


			#define snowpass(x) (1.0 - step(sr, x))
//			#define snowpass(x) (x)

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 time : TEXCOORD1;
				float4 color : COLOR;
			};


			float4	_WorldPosition;
			float	_Aspect;
			float	_Scale;

			// BlobNoise (superposition of blobs in displaced-grid voronoi-cells) by Jakob Thomsen
			// Thanks to FabriceNeyret2 for simplifying the program.
			// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

			// iq's hash function from https://www.shadertoy.com/view/MslGD8
			float2 hash(float2 p) {
//				p = float2(dot(p, float2(6.413, -1.7445)), dot(p, float2(-9.532, 3.324)));
				p = float2(dot(p, float2(-2.2134, 7.74333)), dot(p, float2(4.6347, -8.723)));
				return frac(p);
//				return cos(p * 2000.0);
			}

			float simplegridnoise(float2 v, float t)
			{
				float2 fl = floor(v), fr = frac(v);
				float mindist = 1.0;
				for (float y = -1.0; y <= 1.0; y += 1.0)
				{
					for (float x = -1.0; x <= 1.0; x += 1.0)
					{
						float2 of = float2(x, y);
						//            vec2 pos = .5 + .5 * cos( PI * (T*.7 + hash(fl+offset)));
						float2 pos = .5 + .5 * cos(PI * hash(fl + of) + t * PI * 2.0);
						float2 d = pos + of - fr;
//						mindist = min(mindist, length(d));
						mindist = min(mindist, dot(d, d));
					}
				}

				return mindist;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.time.x = frac(_Time.x * 5.0);
#if defined (LOW_DETAIL_ON)
				o.time.y = fmod(_Time.x * SNOWSPEED, SNOWSPEED * 2.0);
#else
				o.time.y = _Time.x * SNOWSPEED;
#endif
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				uv *= float2(1.0, _Aspect);
				float2 of = /*(_WorldPosition.xy * 0.3) + */float2(i.time.y * 0.2, i.time.y);
				float w = 0.0;


#ifdef MANUALSCALE
				float sc = _Scale;
#else

#if defined (LOW_DETAIL_ON)
				float sc = 15.0;
#elif defined (MEDIUM_DETAIL_ON)
				float sc = 10.0;
#elif defined (HI_DETAIL_ON)
				float sc = 15.0;
#endif

#endif
				float sr = (sc / 30.0) * SNOWRADIUS; 
/*
#if defined (LOW_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * 10.0) + of, i.time.x));
#elif defined (MEDIUM_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * 10.0) + of, i.time.x));
				w += snowpass(simplegridnoise((uv * 15.0) + of, i.time.x))) * 0.75;
#elif defined (HI_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * 10.0) + of, i.time.x));
				w += snowpass(simplegridnoise((uv * 15.0) + of, i.time.x)) * 0.75;
				w += snowpass(sr, simplegridnoise((uv * 20.0) + of, i.time.x)) * 0.5;
#endif
*/

#if defined (LOW_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * sc * 0.7) + of, i.time.x));

#elif defined (MEDIUM_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * sc * 0.7) + of, i.time.x));
				w += snowpass(simplegridnoise((uv * sc) + of, i.time.x)) * 0.75;
#elif defined (HI_DETAIL_ON)
				w += snowpass(simplegridnoise((uv * sc * 0.7) + of, i.time.x));
				w += snowpass(simplegridnoise((uv * sc) + of, i.time.x)) * 0.5;
//				w += snowpass(simplegridnoise((uv * sc * 1.3) + of, i.time.x)) * 0.5;
#endif
				fixed4 col = fixed4(1.0, 1.0, 1.0, w * i.color.a);
				return col;
			}
			ENDCG
		}
	}
}
                                                           