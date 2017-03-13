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
//		ZWrite Off

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
			#pragma multi_compile_fwdbase
			#pragma glsl_no_auto_normalization
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
				//				UNITY_FOG_COORDS(1)
				float4 vCol : COLOR;
				float4 vertex : SV_POSITION;
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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vCol = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _CloudTex);
				return o;
			}			
			fixed4 frag (v2f i) : SV_Target
			{
//				return fixed4(i.uv.y, i.uv.y, i.uv.y, 1.0);
//				float st = star(i.uv);
//				i.uv.x -= 0.5;
//				float mon = moon(i.uv, _MoonPos, _MoonRadius);
//				i.uv.x *= ((1.0 - i.uv.y) + 0.5);
				float vy = i.uv.y / _CloudTex_ST.y;
//				float vy = i.uv.y;
//							i.uv *= 4.0;
				float intensity = tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed, 0.0))).x;
				i.uv.x += 0.4;
				intensity += tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed * 1.5, 0.3))).x;
				i.uv.x += 0.2;
				intensity += tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed * 2.0, 0.1))).x;
				i.uv.x += 0.3;
				intensity += tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed * 2.5, 0.2))).x;
				intensity *= 0.25;
				//				float alfa = clamp((intensity / (_AlphaThreshold / _ColorSteps)) - 1.0, 0.0, 1.0);
//				fixed4 cloudsC = fixed4(alfa, alfa, alfa, 1.0) * _Tint;
				fixed4 cloudsC = lerp(_BackgroundColor, _Tint, intensity);
//				return cloudsC;

//				fixed4 moonC = fixed4(max(mon, st) * _MoonColor.xyz, 1.0);
				//				clip(colf.a - 0.1);
				//colf += mon * _MoonColor;
//				cloudsC = max(cloudsC, moonC);
				cloudsC.w = smoothstep(0.0, _CloudThreshold, intensity * (1.0 - pow(abs(vy - 0.5) * 2.0, _CloudPower)));
//				cloudsC.w = intensity;
				return cloudsC;
				//return lerp(moonC, cloudsC, alfa * 1.0);
			}

			ENDCG
		}
	}
}
