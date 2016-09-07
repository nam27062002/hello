Shader "Unlit/FireShader"
{
	Properties
	{
		_NoiseTex ("Texture", 2D) = "white" {}
		_ColorRamp("Texture", 2D) = "white" {}
		_RampOffset("Ramp Offset", Range(0.0, 1.0)) = 0.2 //
		_ColorSteps("Color steps", Range(0.0, 20.0)) = 8.0	// color steps
		_AlphaThreshold("Alpha threshold", Range(0.0, 20.0)) = 2.0	// alpha threshold
		_FirePower("Fire Power", Range(1.0, 10.0)) = 3.0	// Fire power
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		// Blend One One
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
			float	_FirePower;


			v2f vert (appdata v)
			{
				v2f o;
//				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float intensity = tex2D(_NoiseTex, (i.uv.xy - float2(0.0, _Time.y * 0.75)) * 0.5).x;
				intensity += tex2D(_NoiseTex, (i.uv.xy - float2(0.91, _Time.y * 0.75) * 0.25)).x;


//				// apply fog
//				UNITY_APPLY_FOG(i.fogCoord, col);

				half2 d = i.uv - half2(0.5, 0.5);

				intensity = intensity * (0.25 - dot(d, d)) * _FirePower;
				intensity = floor(intensity * _ColorSteps) / _ColorSteps;
				float txid = clamp(1.0 - intensity + _RampOffset, 0.0, 1.0);

				fixed3 col = tex2D(_ColorRamp, float2(txid, 0.0));

				return float4(col, step(_AlphaThreshold / _ColorSteps, intensity));

			}

			ENDCG
		}
	}
}
