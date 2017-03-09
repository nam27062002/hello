Shader "Hungry Dragon/SkyForeground"
{
	Properties
	{
		_CloudTex ("Cloud tex", 2D) = "white" {}
		_Tint("Clouds color", Color) = (.5, .5, .5, 1)
		_Speed("Cloud speed", Float) = 1.0
		_CloudPower("Cloud power", Range(1.0, 10.0)) = 1.0

		_MoonPos("Moon position", Vector) = (0.0, 0.0, 0.0)
		_MoonRadius("Moon radius", Float) = 0.1			//
		_MoonColor("Moon color", Color) = (0.75, 0.25, 0.0, 1.0)			//
		_BackgroundColor("Background color", Color) = (0.0, 0.0, 0.0, 0.0)

		_NoiseTex("Noise tex", 2D) = "white" {}
		_StarsPInches("Stars per inches", Range(1, 100)) = 4
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

			float4	_Tint;
			float2	_MoonPos;
			float	_MoonRadius;
			float4	_MoonColor;
			float4	_BackgroundColor;
			float	_StarsPInches;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vCol = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _CloudTex);
				return o;
			}			

			half moon(float2 uv, float2 sunPos, float radius)
			{
				float2 dv = uv - sunPos;
				float d = dot(dv, dv);
				half r = 1.0 - smoothstep(radius, radius + 0.0025, d);
				dv = uv - (sunPos + sin(_Time.y) * 0.075);
				d = dot(dv, dv);
				r -= 1.0 - smoothstep(radius, radius + 0.0025, d);
				return clamp(r, 0.0, 1.0);
			}

			float2 hash22(float2 uv)
			{
				return frac(sin(uv * float2(361424.0, 51675.0)) * float2(23234434.0, 3463247.0));
			}

			half star(float2 uv)
			{
//				float intensity = tex2D(_MainTex, (i.uv.xy + float2(_Time.y * _Speed * persp, 0.0))).x;
//				intensity = tex2D(_MainTex, (i.uv.xy + float2(_Time.y * _Speed * persp, 0.0))).x;
				float2 si = floor(uv * _StarsPInches) / _StarsPInches;
//				float2 so = float2(tex2D(_MainTex, si + _Time.yy).x , tex2D(_NoiseTex2, si + _Time.yy).x) / _StarsPInches;
				float2 so = tex2D(_NoiseTex, si).xy;
//				float2 so = hash22(si);
				float2 d = (si + so / _StarsPInches) - uv;

//				return clamp((1.0 - smoothstep(0.00000, 0.00002, dot(d, d) * (so.x + so.y) * 4.0)) * so.x, 0.0, 1.0);
				return (1.0 - smoothstep(0.00000, 0.00002, dot(d, d) * (so.x + so.y) * 4.0)) * so.x;
			}


			fixed4 frag (v2f i) : SV_Target
			{
//				return fixed4(i.uv.y, i.uv.y, i.uv.y, 1.0);
//				float st = star(i.uv);
				i.uv.x -= 0.5;
//				float mon = moon(i.uv, _MoonPos, _MoonRadius);
//				i.uv.x *= ((1.0 - i.uv.y) + 0.5);
				float intensity = tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed, 0.0))).x;
				i.uv.x += 0.35;
				intensity += tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed * 1.5, 0.3))).x;
				i.uv.x += 0.25;
				intensity += tex2D(_CloudTex, (i.uv.xy + float2(_Time.y * _Speed * 2.0, 0.1))).x;
				i.uv.x += 0.15;
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
				cloudsC.w = smoothstep(0.0, 0.25, intensity * (1.0 - pow(abs(i.uv.y - 0.5) * 2.0, _CloudPower)));
//				cloudsC.w = intensity;
				return cloudsC;
				//return lerp(moonC, cloudsC, alfa * 1.0);
			}

			ENDCG
		}
	}
}
