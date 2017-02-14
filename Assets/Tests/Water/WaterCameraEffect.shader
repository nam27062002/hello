Shader "Hungry Dragon/Water Camera Effect"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_TexelOffset("Texel offset", Range(1, 10)) = 1.0
		_Focus("Focus", Range(0.0, 10.0)) = 0.5
		_LensOffset("Lens offset", Range(0.0, 1.0)) = 0.5
	}


	SubShader{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				Stencil
				{
					Ref 10
					Comp less
				}


				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				//			fixed4 _Color;
				fixed _Intensity;

				#define RADIUS 0.008

				#define SPEED  2.0

				fixed4 frag(v2f i) : SV_Target
				{
					return fixed4(1.0, 1.0, 0.0, 1.0);
					//				i.uv += 0.5;
					float c = cos(_Time.y * SPEED * 1.5);
					float s = sin(_Time.y * SPEED);

					fixed4 col = tex2D(_MainTex, i.uv + float2(c * RADIUS, s * RADIUS) * _Intensity);
					col += tex2D(_MainTex, i.uv + float2(-c * RADIUS, -s * RADIUS) * _Intensity);
				//				col += tex2D(_MainTex, i.uv + float2(c * RADIUS, -s * RADIUS) * _Intensity);
				//				col += tex2D(_MainTex, i.uv + float2(-c * RADIUS, s * RADIUS) * _Intensity);

					return col * 0.5;
				}
				ENDCG
			}

			Pass{

				Stencil
				{
					Ref 11
					Comp Equal
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
	//			#pragma fragmentoption ARB_precision_hint_fastest 

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				float4 _MainTex_ST;
	//			uniform float4 _Tint;
	//			uniform float4 _Tint2;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};


				v2f vert(appdata_img v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	//				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
					o.uv = v.texcoord.xy;
					return o;
				}

				float4 frag(v2f i) : COLOR{
					return float4(0.0, 0.0, 1.0, 1.0);
					float4 col = float4(1.0, 0.0, 0.0, 1.0); // tex2D(_MainTex, i.uv);// *lerp(_Tint, _Tint2, i.uv.y);
	//				return float4(1.0, 0.0, 0.0, 1.0);
					return col;
				}
				ENDCG
			}
	}
}
