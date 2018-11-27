Shader "Hungry Dragon/SnowEffect"
{
	Properties
	{
		_WorldPosition ("Position", Vector) = (0.0, 0.0, 0.0, 0.0)
		_Aspect("Aspect", float) = 1.0
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

			// BlobNoise (superposition of blobs in displaced-grid voronoi-cells) by Jakob Thomsen
			// Thanks to FabriceNeyret2 for simplifying the program.
			// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

			#define PI 3.1415926
			#define SNOWSPEED 20.0
			#define SNOWRADIUS 0.05

			#define fragmentoption 

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 time : TEXCOORD1;
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			float4 _WorldPosition;
			float	_Aspect;

			// iq's hash function from https://www.shadertoy.com/view/MslGD8
			float2 hash(float2 p) {
				float2 p2 = float2(dot(p, float2(76.413, -41.7445)), dot(p, float2(-29.532, 63.324)));
				return frac(p2);
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
						mindist = min(mindist, length(d));
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
				o.time.y = fmod(_Time.x * SNOWSPEED, SNOWSPEED * 2.0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				uv *= float2(1.0, _Aspect);
				float2 of = /*(_WorldPosition.xy * 0.3) + */float2(0.0, i.time.y);
				float w = 0.0;
#if defined (LOW_DETAIL_ON)
				w += 1.0 - step(SNOWRADIUS, simplegridnoise((uv * 10.0) + of, i.time.x));
#elif defined (MEDIUM_DETAIL_ON)
				w += 1.0 - step(SNOWRADIUS, simplegridnoise((uv * 10.0) + of, i.time.x));
				w += (1.0 - step(SNOWRADIUS, simplegridnoise((uv * 15.0) + of, i.time.x))) * 0.75;
#elif defined (HI_DETAIL_ON)
				w += 1.0 - step(SNOWRADIUS, simplegridnoise((uv * 10.0) + of, i.time.x));
				w += (1.0 - step(SNOWRADIUS, simplegridnoise((uv * 15.0) + of, i.time.x))) * 0.75;
				w += (1.0 - step(SNOWRADIUS, simplegridnoise((uv * 20.0) + of, i.time.x))) * 0.5;
#endif
				fixed4 col = fixed4(1.0, 1.0, 1.0, w);
				return col;
			}
			ENDCG
		}
	}
}
                                                           