﻿Shader "Hungry Dragon/WindShader"
{
	Properties
	{
		_MainTex ("Noise", 2D) = "white" {}
		_NoiseTex2("Noise2", 2D) = "white" {}

//		_Speed("Fire Speed", Float) = 1.0				// Fire speed
		_Power("Fire Power", Range(0.0, 10.0)) = 3.0	// Fire power
		_Alpha("Alpha", Range(0.0, 1.0)) = 1.0	// alpha translucency

	}

	SubShader
	{
		Tags{ "Queue" = "Transparent+8" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha One
//		Blend SrcAlpha OneMinusSrcAlpha
//		Blend One OneMinusSrcColor


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
				float2 uv2 : TEXCOORD1;
		};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				//				UNITY_FOG_COORDS(1)
				float4 vCol : COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4  _MainTex_ST;
			sampler2D _NoiseTex2;
			float4  _NoiseTex2_ST;

//			float	_Speed;
			float	_Power;
//			float	_Seed;
			float	_Alpha;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vCol = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _NoiseTex2);
				return o;
			}			

			fixed4 frag (v2f i) : SV_Target
			{
//				float intensity = tex2D(_MainTex, (i.uv.xy + float2(_Time.y * _Speed, _Seed))).x;
//				intensity += tex2D(_NoiseTex2, (i.uv.xy + float2(_Time.y * _Speed * 0.333, -_Seed) + float2(intensity * 0.5, 0.0))).x;// +pow(i.uv.y, 3.0);
//				i.uv = frac(i.uv);
//				float intensity = tex2D(_MainTex, i.uv.xy * float2(0.5, 2.0)).x;
				//intensity += tex2D(_NoiseTex2, (i.uv2.xy * float2(0.33, 2.0))).x;// +pow(i.uv.y, 3.0);
				float intensity = tex2D(_MainTex, i.uv.xy * float2(1.0, 1.0)).x;
				intensity += tex2D(_NoiseTex2, (i.uv.xy * float2(0.5, 1.0))).x;// +pow(i.uv.y, 3.0);


				half2 d = i.uv - half2(0.5, 0.5);
				intensity = intensity * (0.5 - abs(d.y)) * _Power;

//				intensity = intensity * (0.25 - (i.uv.y - 0.5)) * _Power;
				//				float alfa = clamp((intensity / (_AlphaThreshold / _ColorSteps)) - 1.0, 0.0, 1.0);
				float alfa = intensity;
				fixed4 colf = fixed4(alfa, alfa, alfa, alfa * _Alpha) * i.vCol;
//				clip(colf.a - 0.1);
				return colf;
			}

			ENDCG
		}
	}

//	Fallback "Diffuse"
	CustomEditor "GlowMaterialInspector"

}
