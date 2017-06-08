Shader "Hungry Dragon/FireShader"
{
	Properties
	{
		_NoiseTex ("Noise", 2D) = "white" {}
		_ColorRamp("Color Ramp", 2D) = "white" {}
		_RampOffset("Ramp Offset", Range(0.0, 1.0)) = 0.2 //
		_ColorSteps("Color steps", Range(0.0, 32.0)) = 8.0	// color steps
		_AlphaThreshold("Alpha threshold", Range(0.0, 32.0)) = 2.0	// alpha threshold
		_Speed("Fire Speed", Float) = 1.0				// Fire speed
		_Power("Fire Power", Range(1.0, 10.0)) = 3.0	// Fire power
		_Seed("Random Seed", Float) = 0.0							//Randomize effect
		_Alpha("Alpha", Range(0.0, 1.0)) = 1.0	// alpha translucency
		_NoiseScale("Noise scale:", Range(0.001, 80)) = 1.0
		_ShaderTime("Shader Time:", Float) = 0.0		//Shader Time
	}

	SubShader
	{
//		Tags{ "Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Tags{ "Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderType" = "Glow" }
		LOD 100
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha
		// Blend One One
		//Blend OneMinusDstColor One
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
//			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
//				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _NoiseTex;
			float4  _NoiseTex_ST;
			sampler2D _ColorRamp;
			float4  _ColorRamp_ST;
			float	_RampOffset;
			float   _ColorSteps;
			float	_AlphaThreshold;
			float	_Speed;
			float	_Power;
			float	_Seed;
			float	_Alpha;
			float	_NoiseScale;
			float 	_ShaderTime;

			v2f vert (appdata v)
			{
				v2f o;
//				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}			

			// 2D Random
			float random(in float2 st) {
//				return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
				return frac(sin(dot(st.xy, float2(1245.9898, 7823.233))) * 43758.5453123);
			}


			// 2D Noise based on Morgan McGuire @morgan3d
			// https://www.shadertoy.com/view/4dS3Wd
			float noise(in float2 st) {
				st *= _NoiseScale;
				float2 i = floor(st);
				float2 f = frac(st);

				// Four corners in 2D of a tile
				float a = random(i);
				float b = random(i + float2(1.0, 0.0));
				float c = random(i + float2(0.0, 1.0));
				float d = random(i + float2(1.0, 1.0));

				// Smooth Interpolation

				// Cubic Hermine Curve.  Same as SmoothStep()
				float2 u = f * f * (3.0 - 2.0 * f);
//				float2 u = f;
				// u = smoothstep(0.,1.,f);

				// Mix 4 coorners porcentages
				return lerp(a, b, u.x) +
					(c - a)* u.y * (1.0 - u.x) +
					(d - b) * u.x * u.y;
			}

			#define _PI 3.1415926535897932384626433832795

			fixed4 frag (v2f i) : SV_Target
			{
//				return noise(i.uv * 10.0);
//				i.uv.y = 1.0 - i.uv.y;
//				i.uv.y *= i.uv.y * i.uv.y;
//				float intensity = tex2D(_NoiseTex, (i.uv.xy - float2(_Seed, _Time.y * _Speed))).x;
//				intensity += tex2D(_NoiseTex, (i.uv.xy - float2(-_Seed, _Time.y * _Speed * 0.333))).x;

//				i.uv.y *= 1.0 - i.uv.y;
				i.uv -= half2(0.5, 0.5);
				float t1 = frac(-_ShaderTime * _Speed);
				float t2 = frac((-_ShaderTime * _Speed * 1.5) + 0.333333);
				float t3 = frac((-_ShaderTime * _Speed * 3.0) + 0.666666);
				float i1 = abs(t1 - 0.5) * 2.0;//(0.75 + sin(_Time.y * _Speed) * 0.25);
				float i2 = abs(t2 - 0.5) * 2.0;//(0.75 + sin((_Time.y * _Speed) + _PI * 0.5) * 0.5);
				float i3 = abs(t3 - 0.5) * 2.0;//(0.75 + sin((_Time.y * _Speed) + _PI * 0.5) * 0.5);
				i1 *= i1;
				i2 *= i2;
				i3 *= i3;

				float2 uv1 = (i.uv * t1);
				float2 uv2 = (i.uv * t2);
				float2 uv3 = (i.uv * t3);

				//				i.uv.y = pow(i.uv.y, 8);

//				float intensity = noise(i.uv + float2(_Seed, _Time.y * _Speed));
//				float intensity = noise(i.uv + float2(_Seed, _Time.y * _Speed));
//				intensity += noise(i.uv + float2(-_Seed, _Time.y * _Speed * 0.3333));

//				float intensity = noise(uv1 + _Seed) * (1.0 - i1);
//				intensity += noise(uv2 + _Seed) * (1.0 - i2);
//				intensity += noise(uv3 + _Seed) * (1.0 - i3);

				float intensity;
				intensity  = tex2D(_NoiseTex, (uv1 + _Seed * 1.0)).x * i1;
				intensity += tex2D(_NoiseTex, (uv2 + _Seed * 2.0)).x * i2;
				intensity += tex2D(_NoiseTex, (uv3 + _Seed * 3.0)).x * i3;

//				float intensity = noise(uv1 + _Seed * 1.0) * i1;
//				intensity += noise(uv2 + _Seed * 2.0) * i2;
//				intensity += noise(uv3 + _Seed * 3.0) * i3;



//				return intensity;



//				// apply fog
//				UNITY_APPLY_FOG(i.fogCoord, col);

				half2 d = i.uv;

				intensity = intensity * (0.25 - dot(d, d)) * _Power;
				intensity = floor(intensity * _ColorSteps) / _ColorSteps;
				float txid = clamp(1.0 - intensity + _RampOffset, 0.0, 1.0);

//				fixed3 col = fixed3(txid, txid, txid);// tex2D(_ColorRamp, float2(txid, 0.0));
				fixed3 col =  tex2D(_ColorRamp, float2(txid, 0.0));

//				return fixed4(col, step(_AlphaThreshold / _ColorSteps, intensity));
				float threshold = _AlphaThreshold / _ColorSteps;
				return fixed4(col, step(threshold, intensity) * _Alpha);
			}

			ENDCG
		}
	}

}
