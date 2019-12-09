// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/SkyForeground"
{
	Properties
	{
		_CloudTex ("Cloud tex", 2D) = "white" {}
		_Tint("Clouds color", Color) = (.5, .5, .5, 1)
		_Speed("Cloud speed", Float) = 1.0
		_CloudPower("Cloud power", Range(1.0, 10.0)) = 1.0
		_CloudThreshold("Cloud threshold", Range(0.0, 0.5)) = 0.25

		_BackgroundColor("Background color", Color) = (0.0, 0.0, 0.0, 0.0)

	}

	SubShader
	{
//		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
//		Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha
//		Blend One OneMinusSrcColor


		Cull off
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
			#pragma multi_compile __ NIGHT
//			#pragma glsl_no_auto_normalization
//			#pragma fragmentoption ARB_precision_hint_fastest

			// make fog work
//			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
//				float4 color : COLOR;
				float2 uv : TEXCOORD0;
//				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				//				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
//				float4 vCol : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				//				float depth : TEXCOORD1;
			};

			sampler2D _CloudTex;
			float4  _CloudTex_ST;
			sampler2D _NoiseTex;
			float	_Speed;
			float	_CloudPower;
			float	_CloudThreshold;

			float4	_Tint;
			float4	_BackgroundColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
//				o.vCol = v.color;
				o.uv2 = o.uv = TRANSFORM_TEX(v.uv, _CloudTex);
				o.uv += float2(_Time.y * _Speed, 0.0);
				o.uv2 += float2(_Time.y * _Speed * 1.5, 0.3);
				//				o.depth = mul(unity_ObjectToWorld, v.vertex).z;

				return o;
			}			
			fixed4 frag (v2f i) : SV_Target
			{
				float vy = i.uv.y / _CloudTex_ST.y;
				float intensity = tex2D(_CloudTex, i.uv).x;
//				i.uv.x += 0.4;
				intensity += tex2D(_CloudTex, i.uv2).x;
				intensity *= 0.5;
				fixed4 cloudsC = lerp(_BackgroundColor, _Tint, intensity);

				cloudsC.w = smoothstep(0.0, _CloudThreshold, intensity * (1.0 - pow(abs(vy - 0.5) * 2.0, _CloudPower))) * _Tint.w;
#if defined(NIGHT)
				return cloudsC * fixed4(0.25, 0.25, 0.5, 1.0);
#else
                return cloudsC;
#endif
			}

			ENDCG
		}
	}
}
