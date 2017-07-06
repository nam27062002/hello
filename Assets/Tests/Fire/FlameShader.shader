Shader "Hungry Dragon/FlameShader"
{
	Properties
	{
		_NoiseTex ("Noise", 2D) = "white" {}
		_ColorRamp("Color Ramp", 2D) = "white" {}
		_RampOffset("Ramp Offset", Range(0.0, 1.0)) = 0.2 //
		_ColorSteps("Color steps", Range(0.0, 16.0)) = 16.0	// color steps
		_AlphaThreshold("Alpha threshold", Range(0.0, 8.0)) = 2.0	// alpha threshold

		_Speed("Fire Speed", Float) = 1.0				// Fire speed
		_Power("Fire Power", Range(0.0, 10.0)) = 3.0	// Fire power
		_Seed("Random Seed", Float) = 0.0							//Randomize effect
		_Alpha("Alpha", Range(0.0, 1.0)) = 1.0	// alpha translucency

	}

	SubShader
	{
		Tags{ "Queue" = "Transparent+8" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		Pass
		{
/*
			Stencil
			{
				Ref 5
				Comp always
				Pass Replace
				ZFail keep
			}
*/

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			// make fog work
//			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
//				UNITY_FOG_COORDS(1)
				float4 vCol : COLOR;
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


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vCol = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
				return o;
			}			

			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.y = 1.0 - (i.uv.y * 0.75);
				i.uv.y *= i.uv.y;
				float intensity = tex2D(_NoiseTex, (i.uv.xy + float2(_Seed, _Time.y * _Speed))).x;
				intensity += tex2D(_NoiseTex, (i.uv.xy + float2(-_Seed, _Time.y * _Speed * 0.333) + float2(0.0, intensity * 0.5))).x;// +pow(i.uv.y, 3.0);

//				// apply fog

				half2 d = i.uv - half2(0.5, 0.5);

				intensity = intensity * (0.25 - dot(d, d)) * _Power;
				intensity = floor(intensity * _ColorSteps) / _ColorSteps;
				float txid = clamp(1.0 - intensity + _RampOffset, 0.0, 1.0);

				fixed3 col =  tex2D(_ColorRamp, float2(txid, 0.0));

//				float alfa = clamp((intensity / (_AlphaThreshold / _ColorSteps)) - 1.0, 0.0, 1.0);
				float alfa = clamp((intensity) -_AlphaThreshold, 0.0, 1.0);
				fixed4 colf = fixed4(col, alfa * _Alpha) * i.vCol;
				clip(colf.a - 0.01);
				//colf.a = 0.5;
				return colf;
			}

			ENDCG
		}
	}
}
