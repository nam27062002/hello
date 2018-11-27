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
//				float2 p2 = float2(dot(p, float2(0.0413, -0.0712)), dot(p, float2(-0.0251, 0.0343)));

				return frac(p2);
//				return frac(sin(p2) * 210.107);
//				return frac(float2(sin(p.x) * 110.10, cos(p.y) * 210.510));

			}

	        float noise(float2 v, float of) {
//	            float T = _Time.x * 10.0;
	            float2 fl = floor(v), fr = frac(v);

	            float2 pos = .5 + .5 * cos(1.342 * PI * hash(fl + of));
	            float2 d = pos + of - fr;
	            return length(d);
	        }

	        float simplegridnoise9(float2 v, float t)
	        {
	            float3 of = float3(1.0, 0.0, -1.0);

	            float mindist = 1.0;	//noise(v, of.yy);

	            mindist = min(mindist, noise(v, of.zz));
	            mindist = min(mindist, noise(v, of.yz));
	            mindist = min(mindist, noise(v, of.xz));

	            mindist = min(mindist, noise(v, of.zy));
	            mindist = min(mindist, noise(v, of.yy));
	            mindist = min(mindist, noise(v, of.xy));

	            mindist = min(mindist, noise(v, of.zx));
	            mindist = min(mindist, noise(v, of.yx));
	            mindist = min(mindist, noise(v, of.xx));

	           	return mindist;
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
				o.time.y = fmod(_Time.x * 20.0, 40.0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			// sample the texture
//				fixed4 col = tex2D(_MainTex, i.uv);
				float2 uv = i.uv;
				uv *= float2(1.0, _Aspect);
				float2 of = /*(_WorldPosition.xy * 0.3) + */float2(0.0, i.time.y);

				float bl = simplegridnoise((uv * 10.0) + of, i.time.x);
//				float bl2 = simplegridnoise((uv * 15.0) + of, i.time.x);
//				float bl3 = simplegridnoise((uv * 20.0) + of, i.time.x);

				fixed4 col = fixed4(1.0, 1.0, 1.0, 1.0);
				float w = 0.0;

				w += 1.0 - step(SNOWRADIUS, bl);
//				w += (1.0 - step(SNOWRADIUS, bl2)) * 0.75;
//				w += (1.0 - step(SNOWRADIUS, bl3)) * 0.5;

				col.w *= w;
				return col;
			}
			ENDCG
		}
	}
}
                                                           